import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright E2E configuration for the Blazor frontend.
 *
 * Prerequisites:
 *   npm install
 *   npx playwright install chromium
 *
 * Running:
 *   The API (https://localhost:7001) and Blazor dev server
 *   (http://localhost:5173) must be running first. See the
 *   root README for startup instructions.
 *
 *   npx playwright test
 */
export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  retries: process.env.CI ? 2 : 0,
  workers: 1, // sequential: tests share database state via the API
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
