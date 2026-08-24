import { ConstantHelper } from '@umbraco-cms/acceptance-test-helpers';
import { test, expect } from '../../helpers/test';
import { SeoTab, SeoTabElement } from '../../helpers/seoToolkitConstants';

const templateName = 'SeoToolkitAcceptanceTemplate';
const documentTypeName = 'SeoToolkitAcceptancePage';
const documentName = 'SeoToolkitAcceptanceContent';

/**
 * The SEO tab on a content item is gated by the SeoToolkit.SeoEnabled condition, which
 * asks the server whether SEO is on for the document's type. With the default
 * SeoToolkit:Global:EnableSeoSettingsByDefaultForTemplated, that is true exactly when
 * the document type has a DEFAULT TEMPLATE (see SeoSettingsRepository.IsEnabled), which
 * createDocumentTypeWithAllowedTemplate sets. Without one the tab never appears.
 */
test.beforeEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);

  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(
    documentTypeName,
    templateId,
    true,
  );
  await umbracoApi.document.createDocumentWithTemplate(documentName, documentTypeId, templateId);
});

test.afterEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test.describe('SEO tab on content', () => {
  test.beforeEach(async ({ umbracoUi, page, discardConsoleErrors }) => {
    // goToContentWithName walks the Content tree, so that section must be open first.
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
    await umbracoUi.content.goToContentWithName(documentName);
    // Drop the transient Umbraco core console errors from walking the Content tree
    // before touching any SeoToolkit UI - the SEO tab render below stays guarded.
    discardConsoleErrors();
    await page.locator(SeoTab.seo).click();
    await expect(page.locator(SeoTabElement.contentView)).toBeVisible();
  });

  test('the SEO tab offers every module sub-view @smoke', async ({ page }) => {
    // Each sub-tab is contributed by a different bundle, so their presence proves
    // MetaFields, Sitemap and SiteAudit all registered against the Common host view.
    await expect(page.locator(SeoTab.metaFields)).toBeVisible();
    await expect(page.locator(SeoTab.sitemap)).toBeVisible();
    await expect(page.locator(SeoTab.pageChecks)).toBeVisible();
  });

  // Which sub-view renders by default is NOT stable: seoToolkitContentView.element.ts
  // builds its routes in whatever order UmbExtensionsManifestInitializer yields (the
  // manifest `weight` is not applied) and aliases route[0] to the empty path, so the
  // landing sub-view varies with bundle load order. Always click explicitly.
  test('the Meta Fields sub-view opens @smoke', async ({ page }) => {
    await page.locator(SeoTab.metaFields).click();
    await expect(page.locator(SeoTabElement.metaFieldsContent)).toBeVisible();
  });

  test('the Sitemap sub-view opens @smoke', async ({ page }) => {
    await page.locator(SeoTab.sitemap).click();
    await expect(page.locator(SeoTabElement.sitemapContent)).toBeVisible();
  });

  test('the Page checks sub-view opens @smoke', async ({ page }) => {
    await page.locator(SeoTab.pageChecks).click();
    await expect(page.locator(SeoTabElement.siteAuditContent)).toBeVisible();
  });
});
