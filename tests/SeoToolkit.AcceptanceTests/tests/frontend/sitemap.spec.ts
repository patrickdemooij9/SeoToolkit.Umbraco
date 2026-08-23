import { test, expect } from '../../helpers/test';

const templateName = 'SeoToolkitSitemapTemplate';
const documentTypeName = 'SeoToolkitSitemapPage';
const documentName = 'SeoToolkitSitemapContent';

/** Pulls every <loc> out of a sitemap document. */
function locations(xml: string): string[] {
  return [...xml.matchAll(/<loc>([^<]+)<\/loc>/g)].map((match) => match[1].trim());
}

/**
 * SitemapGenerator.GetSelfAndChildren only emits a node when `content.TemplateId > 0`
 * ("so we don't index data objects and such"), and it walks the published cache. So the
 * fixture needs a document type WITH a template, and the document must be published -
 * an unpublished or template-less node will never appear no matter how long you wait.
 */
test.describe('sitemap.xml', () => {
  test.afterEach(async ({ umbracoApi }) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('new published content appears in the sitemap @smoke', async ({ umbracoApi, request }) => {
    // Arrange - baseline the sitemap before touching anything.
    const before = await request.get('/sitemap.xml');
    expect(before.status(), '/sitemap.xml should be served by the sitemap middleware').toBe(200);
    expect(before.headers()['content-type']).toContain('xml');

    const beforeLocations = locations(await before.text());
    expect(beforeLocations.length, 'expected the starter kit content in the sitemap').toBeGreaterThan(0);

    // Act - create a templated, published page.
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);

    const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(
      documentTypeName,
      templateId,
      true,
    );
    const documentId = await umbracoApi.document.createDocumentWithTemplate(
      documentName,
      documentTypeId,
      templateId,
    );
    expect(await umbracoApi.document.publish(documentId)).toBe(200);

    // Ask Umbraco what URL it assigned rather than guessing the routing rules.
    const documentUrl = await umbracoApi.document.getDocumentUrl(documentId);
    expect(documentUrl, 'Umbraco did not assign the new document a URL').toBeTruthy();

    // Assert - poll, because the published cache refreshes asynchronously.
    await expect
      .poll(
        async () => {
          const response = await request.get('/sitemap.xml');
          return locations(await response.text());
        },
        { message: `"${documentUrl}" never appeared in the sitemap` },
      )
      .toEqual(expect.arrayContaining([expect.stringContaining(documentUrl)]));

    const after = locations(await (await request.get('/sitemap.xml')).text());
    expect(after.length, 'the sitemap should have grown by exactly one entry').toBe(
      beforeLocations.length + 1,
    );
  });

  test('unpublished content stays out of the sitemap @smoke', async ({ umbracoApi, request }) => {
    const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(
      documentTypeName,
      templateId,
      true,
    );
    await umbracoApi.document.createDocumentWithTemplate(documentName, documentTypeId, templateId);
    // Deliberately not published.

    const response = await request.get('/sitemap.xml');

    // Establish the sitemap is actually working BEFORE asserting something is absent
    // from it. Without this the test passes just as happily when the middleware is
    // disabled and /sitemap.xml 404s - an absence proves nothing if nothing is present.
    expect(response.status(), '/sitemap.xml should be served by the sitemap middleware').toBe(200);
    const found = locations(await response.text());
    expect(found.length, 'expected the starter kit content in the sitemap').toBeGreaterThan(0);

    expect(
      found.some((location) => location.includes(documentName.toLowerCase())),
      'a draft should never be advertised in the sitemap',
    ).toBe(false);
  });
});
