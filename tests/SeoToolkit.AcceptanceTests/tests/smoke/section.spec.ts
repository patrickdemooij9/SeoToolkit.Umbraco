import { test, expect } from '../../helpers/test';
import { SEOTOOLKIT_SECTION_URL, ApiRoute } from '../../helpers/seoToolkitConstants';

test.describe('SeoToolkit section', () => {
  test('the section is registered and reachable @smoke', async ({ umbracoUi, page }) => {
    await umbracoUi.goToBackOffice();
    await page.goto(SEOTOOLKIT_SECTION_URL);

    // The sidebar menu only renders once the Common bundle's section + sidebar
    // manifests have loaded, so this covers the whole manifest registration path.
    await expect(page.locator('umb-section-sidebar')).toBeVisible();
    await expect(page).toHaveURL(new RegExp(`${SEOTOOLKIT_SECTION_URL}`, 'i'));
  });

  test('the welcome dashboard renders the installed modules @smoke', async ({ umbracoUi, page }) => {
    await umbracoUi.goToBackOffice();
    await page.goto(`${SEOTOOLKIT_SECTION_URL}/dashboard/welcome`);

    const dashboard = page.locator('welcome-dashboard');
    await expect(dashboard).toBeVisible();
    await expect(dashboard.locator('h1')).toHaveText('Welcome!');

    // The module cards are populated from GET /umbraco/seoToolkit/modules. If the
    // request fails the heading still renders, so assert on the cards themselves.
    await expect(dashboard.locator('.module').first()).toBeVisible();
  });

  test('every installed module reports as Installed @smoke', async ({ umbracoApi }) => {
    const response = await umbracoApi.get(ApiRoute.modules);
    expect(response.status()).toBe(200);

    const modules = await response.json();
    expect(Array.isArray(modules), 'modules endpoint should return an array').toBe(true);
    expect(modules.length, 'no SeoToolkit modules were registered').toBeGreaterThan(0);

    // SeoToolkit.Umbraco references all eight feature packages, so on the test site
    // none of them should report as missing.
    const notInstalled = modules
      .filter((module: { status: string }) => module.status === 'NotInstalled')
      .map((module: { alias: string }) => module.alias);
    expect(notInstalled, 'modules unexpectedly reporting as NotInstalled').toEqual([]);
  });
});
