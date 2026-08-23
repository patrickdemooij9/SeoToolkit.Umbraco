import { ConstantHelper } from '@umbraco-cms/acceptance-test-helpers';
import { test, expect } from '../../helpers/test';
import { SeoTab, SeoTabElement } from '../../helpers/seoToolkitConstants';

const templateName = 'SeoToolkitAcceptanceDocTypeTemplate';
const documentTypeName = 'SeoToolkitAcceptanceDocType';
const renamedDocumentTypeName = 'SeoToolkitAcceptanceDocTypeRenamed';

/**
 * The SEO tab itself is unconditional on a document type (it only matches the
 * Umb.Workspace.DocumentType alias), but its Meta Fields and Sitemap sub-views only
 * appear once SEO is enabled for the type - which, by default, means the type needs a
 * default template. So this fixture is templated, same as the content one.
 */
test.beforeEach(async ({ umbracoApi }) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);

  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(
    documentTypeName,
    templateId,
    true,
  );
});

test.afterEach(async ({ umbracoApi }) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(renamedDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test.describe('SEO tab on document types', () => {
  test.beforeEach(async ({ umbracoUi }) => {
    // goToDocumentType walks the Settings tree, so that section must be open first.
    await umbracoUi.goToBackOffice();
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings, false);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
  });

  test('the SEO tab renders its module sub-views @smoke', async ({ page }) => {
    await page.locator(SeoTab.seo).click();

    const seoView = page.locator(SeoTabElement.documentTypeView);
    await expect(seoView, 'the SEO tab should render <st-document-view>').toBeVisible();

    await expect(page.locator(SeoTab.metaFields)).toBeVisible();
    await expect(page.locator(SeoTab.sitemap)).toBeVisible();

    // The landing sub-view is not stable (see the note in contentSeoTab.spec.ts), so
    // click Meta Fields explicitly rather than assuming it is the default.
    await page.locator(SeoTab.metaFields).click();
    await expect(page.locator(SeoTabElement.metaFieldsDocumentType)).toBeVisible();
  });

  test('the overridden Save action persists changes @smoke', async ({ umbracoUi, umbracoApi }) => {
    // SeoToolkit overwrites Umb.WorkspaceAction.DocumentType.Save
    // (seoToolkitDocumentManifests.ts) with its own action that delegates to
    // UmbSubmitWorkspaceAction and then fires an extra event. A regression there breaks
    // saving ANY document type in the backoffice, not just SeoToolkit's own screens.
    //
    // Umbraco 17.0 shows no toast and sets no button state on a document type save, so
    // this asserts the only thing that actually matters: the change reached the server.
    await umbracoUi.documentType.enterDocumentTypeName(renamedDocumentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    await expect
      // doesNameExist resolves to the document type object (or false), not a boolean.
      .poll(async () => Boolean(await umbracoApi.documentType.doesNameExist(renamedDocumentTypeName)), {
        message: 'the renamed document type was never persisted - the Save override is broken',
      })
      .toBe(true);
  });
});
