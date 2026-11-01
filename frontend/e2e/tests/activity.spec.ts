import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

test.describe('Activity Stream', () => {
  test.beforeEach(async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
  });

  test('activity page loads with header', async ({ page }) => {
    await page.goto('/activity');
    await expect(page.locator('h1', { hasText: 'Activity Stream' })).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('text=Real-time audit log powered by RabbitMQ consumers')).toBeVisible();
  });

  test('new batch via MCP appears in activity stream automatically', async ({ context, page }) => {
    // 1. Open /activity in first tab
    await page.goto('/activity');
    await expect(page.locator('h1', { hasText: 'Activity Stream' })).toBeVisible({ timeout: 10_000 });

    // 2. Create a batch via MCP in second tab
    const mcpPage = await context.newPage();
    await mcpPage.goto('/mcp');
    await expect(mcpPage.locator('h1', { hasText: 'MCP Playground' })).toBeVisible();

    const batchNumber = `B-E2E-ACT-${Date.now()}`;
    const createCard = mcpPage.locator('.card', { hasText: 'create_batch' });
    await createCard.locator('input').first().fill(batchNumber);
    await createCard.locator('button:has-text("Execute")').click();
    await expect(mcpPage.locator('text=Batch Created Successfully')).toBeVisible({ timeout: 15_000 });

    // 3. Verify the batch appears in /activity automatically (within ~5s — Worker + polling)
    // No manual refresh — SignalR + 3-poll cycle handles it
    await expect(page.locator(`text=${batchNumber}`)).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('text=Batch Created').first()).toBeVisible();
  });
});
