import { test as setup, expect } from '@playwright/test';
import { ApiHelpers } from '@umbraco-cms/acceptance-test-helpers';

const storageState = process.env.STORAGE_STATE_PATH ?? 'playwright/.auth/user.json';

/**
 * Logs in once and caches the authenticated session, so every spec starts signed in.
 * Mirrors the "setup" project in Umbraco's own acceptance test config.
 *
 * Credentials come from UMBRACO_USER_LOGIN / UMBRACO_USER_PASSWORD, which
 * @umbraco-cms/acceptance-test-helpers reads internally (see its umbraco.config).
 */
setup('authenticate as admin', async ({ page }) => {
  expect(
    process.env.UMBRACO_USER_LOGIN,
    'UMBRACO_USER_LOGIN is not set — copy .env.example to .env (see README).',
  ).toBeTruthy();

  const umbracoApi = new ApiHelpers(page);
  await umbracoApi.loginToAdminUser();

  // Prove the session actually works before caching it, otherwise every downstream
  // spec fails with an unhelpful timeout instead of a clear auth error here.
  const response = await umbracoApi.get('/umbraco/seoToolkit/modules');
  expect(
    response.status(),
    'Could not reach the SeoToolkit management API as an authenticated admin.',
  ).toBe(200);

  await page.context().storageState({ path: storageState });
});
