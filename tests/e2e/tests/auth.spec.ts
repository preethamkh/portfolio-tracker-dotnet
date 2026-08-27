/**
 * Auth flow E2E tests: registration, logout and login.
 *
 * Uses a unique email per run so tests stay independent of database state.
 * The API must be reachable at the URL configured in BlazorFrontend's
 * wwwroot/appsettings.json (default https://localhost:7001).
 */
import { test, expect } from '@playwright/test';

// Password must satisfy the backend's policy (upper/lower/digit/special)
const password = 'Passw0rd!e2e';

// Unique email per run keeps repeat executions green against a shared DB
const email = `e2e-${Date.now()}@test.com`;

test.describe('Authentication', () => {
  test('registers a new account and lands on the dashboard', async ({ page }) => {
    await page.goto('/register');

    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Password', { exact: true }).fill(password);
    await page.getByLabel('Confirm password').fill(password);
    await page.getByRole('button', { name: 'Create account' }).click();

    // Redirects to the dashboard after auto-login
    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
    await expect(page.getByText(email)).toBeVisible();
  });

  test('logs out and redirects to login', async ({ page }) => {
    await page.goto('/register');
    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Password', { exact: true }).fill(password);
    await page.getByLabel('Confirm password').fill(password);
    await page.getByRole('button', { name: 'Create account' }).click();
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();

    await page.getByRole('button', { name: 'Sign out' }).click();

    await expect(page).toHaveURL(/\/login/);
  });

  test('logs in with valid credentials', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill(email);
    await page.getByLabel('Password').fill(password);
    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  test('shows an error for invalid credentials', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('Email').fill('wrong@example.com');
    await page.getByLabel('Password').fill('WrongP@ss1');
    await page.getByRole('button', { name: 'Sign in' }).click();

    await expect(page.locator('.alert-danger')).toBeVisible();
  });

  test('redirects unauthenticated visitors to login', async ({ page }) => {
    await page.goto('/portfolios');
    await expect(page).toHaveURL(/\/login/);
  });
});
