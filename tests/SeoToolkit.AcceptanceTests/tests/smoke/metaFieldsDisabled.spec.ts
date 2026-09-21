import { test, expect } from '../../helpers/test';
import { ApiRoute } from '../../helpers/seoToolkitConstants';

const documentTypeName = 'SeoToolkitAcceptanceNoTemplateType';
const documentName = 'SeoToolkitAcceptanceNoTemplateContent';

let documentId = '';

/**
 * Regression cover for "s is not iterable".
 *
 * A document type with NO default template has SEO disabled
 * (SeoSettingsRepository.IsEnabled), so DefaultMetaTagsProvider.Get returns null and
 * MetaFieldsController.Get answers with SeoEnabled = false. That response used to leave
 * Fields/Groups/Previewers unset, serialising them as null, and the Meta Fields content
 * view iterated them directly - which threw an uncaught "is not iterable" out of the
 * backoffice whenever the view and the tab's visibility condition disagreed.
 *
 * This surfaced only on CI, where the slower runner widened the window between creating
 * a document type and its default template becoming visible to the settings lookup.
 */
test.beforeEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(
    documentTypeName,
  );
  documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
});

test.afterEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.describe('Meta Fields when SEO is disabled', () => {
  test('the API returns empty arrays rather than nulls @smoke', async ({ umbracoApi }) => {
    const response = await umbracoApi.get(ApiRoute.metaFields, {
      nodeGuid: documentId,
      culture: 'invariant',
    });
    expect(response.status()).toBe(200);

    const model = await response.json();
    expect(model.seoEnabled, 'a template-less document type should report SEO as disabled').toBe(
      false,
    );

    // The backoffice iterates all three directly. Nulls here are what caused the crash.
    expect(Array.isArray(model.groups), 'groups must be an array, never null').toBe(true);
    expect(Array.isArray(model.fields), 'fields must be an array, never null').toBe(true);
    expect(Array.isArray(model.previewers), 'previewers must be an array, never null').toBe(true);
  });
});
