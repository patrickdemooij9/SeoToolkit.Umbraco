import { test, expect } from '../../helpers/test';
import { ApiRoute } from '../../helpers/seoToolkitConstants';

const testContent = [
  'User-agent: *',
  'Disallow: /umbraco',
  'Disallow: /seotoolkit-acceptance-test',
].join('\n');

let originalContent = '';

/**
 * robots.txt is a single global value, not per-test data, so capture and restore it -
 * otherwise a failed run leaves the site's real robots.txt overwritten.
 *
 * Note RobotsTxtController.Save VALIDATES by default and returns the errors WITHOUT
 * saving when any are found, so the content here has to be valid (or skipValidation set).
 */
test.beforeEach(async ({ umbracoApi }) => {
  const response = await umbracoApi.get(ApiRoute.robotsTxt);
  expect(response.status()).toBe(200);
  originalContent = response.status() === 200 ? await response.text() : '';
});

test.afterEach(async ({ umbracoApi }) => {
  await umbracoApi.post(ApiRoute.robotsTxt, {
    content: originalContent,
    skipValidation: true,
  });
});

test.describe('robots.txt', () => {
  test('saved content is served at /robots.txt @smoke', async ({ umbracoApi, request }) => {
    // Act
    const save = await umbracoApi.post(ApiRoute.robotsTxt, {
      content: testContent,
      skipValidation: false,
    });
    expect(save.status()).toBe(200);

    const saveResult = await save.json();
    expect(
      saveResult.errors ?? [],
      'the test content should pass robots.txt validation',
    ).toEqual([]);

    // Assert - the middleware reads through to the service on every request, no cache.
    const response = await request.get('/robots.txt');
    expect(response.status()).toBe(200);
    expect(response.headers()['content-type']).toContain('text/plain');

    const served = await response.text();
    expect(served).toContain('Disallow: /seotoolkit-acceptance-test');
    expect(served.trim()).toBe(testContent);
  });

  test('the saved content round-trips through the management API @smoke', async ({ umbracoApi }) => {
    await umbracoApi.post(ApiRoute.robotsTxt, { content: testContent, skipValidation: false });

    const response = await umbracoApi.get(ApiRoute.robotsTxt);
    expect(response.status()).toBe(200);
    expect((await response.text()).trim()).toBe(testContent);
  });

  test('content that would block the whole site is rejected @smoke', async ({ umbracoApi, request }) => {
    // RobotsTxtValidator warns on `Disallow: /` inside a wildcard User-agent group -
    // i.e. de-indexing the entire site, the mistake an SEO package should never let
    // through silently. Save returns the warnings WITHOUT persisting.
    const blocksEverything = ['User-agent: *', 'Disallow: /'].join('\n');

    const save = await umbracoApi.post(ApiRoute.robotsTxt, {
      content: blocksEverything,
      skipValidation: false,
    });
    expect(save.status()).toBe(200);

    const saveResult = await save.json();
    expect(
      saveResult.errors?.length ?? 0,
      'the validator should have flagged Disallow: / for User-agent: *',
    ).toBeGreaterThan(0);

    // Nothing was persisted, so the live file must still be the original.
    const response = await request.get('/robots.txt');
    const served = response.status() === 200 ? await response.text() : '';
    expect(served.trim(), 'rejected content must never reach /robots.txt').toBe(
      originalContent.trim(),
    );
  });

  test('the same content saves when validation is skipped @smoke', async ({ umbracoApi, request }) => {
    // The backoffice offers a "save anyway" path, so the override has to actually work.
    const blocksEverything = ['User-agent: *', 'Disallow: /'].join('\n');

    const save = await umbracoApi.post(ApiRoute.robotsTxt, {
      content: blocksEverything,
      skipValidation: true,
    });
    expect(save.status()).toBe(200);

    const response = await request.get('/robots.txt');
    expect(response.status()).toBe(200);
    expect((await response.text()).trim()).toBe(blocksEverything);
  });
});
