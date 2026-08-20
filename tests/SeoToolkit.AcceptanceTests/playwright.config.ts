import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';

dotenv.config({ path: '.env' });

/**
 * Mirrors the shape of Umbraco's own acceptance test config
 * (tests/Umbraco.Tests.AcceptanceTest/playwright.config.ts) so that
 * @umbraco-cms/acceptance-test-helpers behaves the way it expects.
 */
const storageState = process.env.STORAGE_STATE_PATH ?? 'playwright/.auth/user.json';
const baseURL = process.env.URL ?? 'https://localhost:44339';

export default defineConfig({
  testDir: './tests',
  timeout: 60 * 1000,
  expect: { timeout: 10 * 1000 },

  // The backoffice is a single shared instance with shared server-side state,
  // so tests must not race each other.
  fullyParallel: false,
  workers: 1,

  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,

  reporter: process.env.CI
    ? [['github'], ['html', { open: 'never' }]]
    : [['list'], ['html', { open: 'never' }]],

  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    // Umbraco marks its elements with data-mark, not data-testid.
    testIdAttribute: 'data-mark',
    trace: 'on-first-retry',
    video: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'setup',
      testMatch: /auth\.setup\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'smoke',
      testDir: './tests/smoke',
      dependencies: ['setup'],
      use: {
        ...devices['Desktop Chrome'],
        storageState,
      },
    },
  ],
});
