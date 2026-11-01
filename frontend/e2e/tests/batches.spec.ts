import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

const uniqueBatch = () => `B-E2E-${Date.now()}`;

test.describe('Batches', () => {
  test.beforeEach(async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
  });

  test('batches list loads with real data', async ({ page }) => {
    await page.goto('/batches');
    await expect(page.locator('h1', { hasText: 'Batches' })).toBeVisible();
    await expect(page.locator('tbody tr').first()).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('table')).toContainText(/B-2026/);
  });

  test('creates a batch and sees it in list', async ({ page }) => {
    const batchNumber = uniqueBatch();
    await page.goto('/batches/new');

    // Wait for ANY input on the page (form is rendered)
    await expect(page.locator('form input').first()).toBeVisible({ timeout: 10_000 });

    const inputs = page.locator('form input');
    const count = await inputs.count();
    console.log(`[DEBUG] Found ${count} inputs on /batches/new`);

    // Fill by position — order: batchNumber, workOrderId, productId, quantity, unit
    await inputs.nth(0).fill(batchNumber);
    await inputs.nth(1).fill('11111111-1111-1111-1111-111111111111');
    await inputs.nth(2).fill('22222222-2222-2222-2222-222222222222');
    await inputs.nth(3).fill('50');
    if (count > 4) {
      await inputs.nth(4).fill('pcs');
    }

    // Submit (try multiple selectors)
    const submitBtn = page.locator('button[type="submit"], button:has-text("Create"), button:has-text("Create Batch")').first();
    await expect(submitBtn).toBeVisible({ timeout: 5_000 });
    await submitBtn.click();

    // Should navigate away from /batches/new
    await expect(page).not.toHaveURL(/\/batches\/new/, { timeout: 15_000 });

    // New batch should appear somewhere (list or details)
    await expect(page.locator(`text=${batchNumber}`)).toBeVisible({ timeout: 15_000 });
  });
});
