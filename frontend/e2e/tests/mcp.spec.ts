import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

test.describe('MCP Playground', () => {
  test.beforeEach(async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
  });

  test('get_work_orders returns data', async ({ page }) => {
    await page.goto('/mcp');
    await expect(page.locator('h1', { hasText: 'MCP Playground' })).toBeVisible();

    // Click Execute on get_work_orders card
    const workOrdersCard = page.locator('.card', { hasText: 'get_work_orders' });
    await workOrdersCard.locator('button:has-text("Execute")').click();

    // Output shows WO number
    await expect(page.locator('text=/WO-2026/').first()).toBeVisible({ timeout: 15_000 });

    // Success badge
    await expect(page.locator('text=Success').first()).toBeVisible();
  });

  test('create_batch via MCP → batch visible', async ({ page }) => {
    const batchNumber = `B-MCP-${Date.now()}`;
    await page.goto('/mcp');

    // Find create_batch card and fill its batchNumber input
    const createCard = page.locator('.card', { hasText: 'create_batch' });
    await createCard.locator('input').first().fill(batchNumber);
    await createCard.locator('button:has-text("Execute")').click();

    // Success
    await expect(page.locator('text=Batch Created Successfully')).toBeVisible({ timeout: 15_000 });
    await expect(page.locator(`text=${batchNumber}`)).toBeVisible();
  });
});
