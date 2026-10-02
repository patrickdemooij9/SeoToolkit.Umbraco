import { ConstantHelper } from '@umbraco-cms/acceptance-test-helpers';
import { test, expect } from '../../helpers/test';
import { ApiRoute, SeoTab, SeoTabElement } from '../../helpers/seoToolkitConstants';

const templateName = 'SeoToolkitSaveOnlyTemplate';
const documentTypeName = 'SeoToolkitSaveOnlyPage';
const documentName = 'SeoToolkitSaveOnlyContent';

/**
 * Regression test for https://github.com/patrickdemooij9/SeoToolkit.Umbraco/issues/586.
 *
 * Meta field values are not part of the document, so the backoffice persists them after
 * the document has been saved. When the editor changes ONLY SEO fields and clicks Save on
 * a published page, the server sees no change to the document and leaves its update
 * dates alone (DocumentRepository.PersistUpdatedItem returns early). Detecting the save
 * through a changed update date therefore missed it, and the SEO edit was silently lost.
 *
 * The page is saved twice: the first save after publishing can still move the update
 * date, which is what made the bug look intermittent. The second save is the one that
 * used to drop the change.
 */
test.beforeEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);

  // A default template is what switches SEO on for the type (see contentSeoTab.spec.ts).
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
});

test.afterEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('Save persists meta fields when nothing else on the page changed @smoke', async ({
  umbracoApi,
  umbracoUi,
  page,
}) => {
  const documentId = (await umbracoApi.document.getByName(documentName)).id as string;

  const storedTitle = async () => {
    const response = await umbracoApi.get(ApiRoute.metaFields, {
      nodeGuid: documentId,
      culture: 'invariant',
    });
    const model = await response.json();
    return model.fields?.find((field: { alias: string }) => field.alias === 'title')?.userValue;
  };
  const updateDate = async () =>
    (await umbracoApi.document.get(documentId)).variants[0].updateDate as string;

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
  await page.locator(SeoTab.seo).click();
  await page.locator(SeoTab.metaFields).click();

  // Matched on the field's description, which the server supplies; the label is a
  // property on umb-property-layout and is not reflected to an attribute.
  const titleInput = page
    .locator(SeoTabElement.metaFieldsContent)
    .locator('st-metafield-contentfield', { hasText: 'Title for the page' })
    .locator('input');

  await titleInput.fill('First SEO title');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await expect.poll(storedTitle, { message: 'the first SEO-only save was not persisted' }).toBe(
    'First SEO title',
  );
  const updateDateAfterFirstSave = await updateDate();

  await titleInput.fill('Second SEO title');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  await expect
    .poll(storedTitle, { message: 'the second SEO-only save was not persisted (issue #586)' })
    .toBe('Second SEO title');

  // Guards the premise of the test: if the server ever starts moving the update date on
  // an unchanged document, this test no longer covers the case it was written for.
  expect(
    await updateDate(),
    'the document update date changed, so this save no longer exercises the unchanged-document path',
  ).toBe(updateDateAfterFirstSave);
});
