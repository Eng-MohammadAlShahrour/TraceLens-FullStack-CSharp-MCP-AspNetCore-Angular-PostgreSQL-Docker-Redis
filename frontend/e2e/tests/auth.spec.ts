import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

test.describe('Authentication', () => {
  test('redirects unauthenticated user to /login', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login/, { timeout: 10_000 });
    const loginPage = new LoginPage(page);
    await loginPage.expectOnLoginPage();
  });

  test('valid credentials → dashboard + OAuth token', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('admin', 'Admin123!');
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 15_000 });
    await expect(page.locator('text=Total Batches')).toBeVisible({ timeout: 10_000 });

    const token = await page.evaluate(() => localStorage.getItem('tracelens.token'));
    expect(token).toBeTruthy();
    expect(token!.length).toBeGreaterThan(2000); // OAuth JWE
  });

  test('invalid credentials → error message', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('admin', 'wrong-password');
    await expect(page.locator('text=/Invalid username or password/i')).toBeVisible({ timeout: 10_000 });
    await expect(page).toHaveURL(/\/login/);
  });
});
