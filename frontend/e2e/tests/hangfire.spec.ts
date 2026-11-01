import { test, expect } from '@playwright/test';

test.describe('Hangfire Dashboard', () => {
  test('dashboard is accessible on localhost', async ({ request }) => {
    const resp = await request.get('http://localhost:5268/hangfire');
    expect(resp.status()).toBe(200);
    const html = await resp.text();
    expect(html.toLowerCase()).toContain('hangfire');
  });
});
