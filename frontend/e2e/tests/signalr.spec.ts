import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

test.describe('SignalR Real-time', () => {
  test('new batch via MCP appears on dashboard without refresh', async ({ context, page }) => {
    // Login once (shared localStorage across pages in same context)
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });

    // Dashboard ready
    await page.goto('/dashboard');
    await expect(page.locator('text=Total Batches')).toBeVisible();
    await expect(page.locator('table tbody tr').first()).toBeVisible({ timeout: 10_000 });

    // Open MCP Playground in second page
    const mcpPage = await context.newPage();
    await mcpPage.goto('/mcp');
    await expect(mcpPage.locator('h1', { hasText: 'MCP Playground' })).toBeVisible();

    // Create batch via MCP
    const batchNumber = `B-RT-${Date.now()}`;
    const createCard = mcpPage.locator('.card', { hasText: 'create_batch' });
    await createCard.locator('input').first().fill(batchNumber);
    await createCard.locator('button:has-text("Execute")').click();
    await expect(mcpPage.locator('text=Batch Created Successfully')).toBeVisible({ timeout: 15_000 });

    // ⚡ Dashboard should update WITHOUT manual refresh
    await expect(page.locator(`text=${batchNumber}`)).toBeVisible({ timeout: 15_000 });
  });
});
