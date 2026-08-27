import { test, expect } from '../../helpers/test';
import {
  ALWAYS_PRESENT_TREE_NODES,
  DOMAIN_ROOT_NODE,
  SEOTOOLKIT_SECTION_URL,
  ApiRoute,
} from '../../helpers/seoToolkitConstants';

const testDomainName = 'SeoToolkit Acceptance Domain';

async function treeRootItems(umbracoApi: any) {
  const response = await umbracoApi.get(`${ApiRoute.tree}/root`);
  expect(response.status()).toBe(200);
  const { items } = await response.json();
  return items as Array<{ id: string; name: string }>;
}

function hasDomainRoot(items: Array<{ id: string }>) {
  return items.some((item) => item.id.toLowerCase() === DOMAIN_ROOT_NODE.id);
}

test.describe('SeoToolkit tree', () => {
  test('the tree root returns every registered section @smoke', async ({ umbracoApi }) => {
    const items = await treeRootItems(umbracoApi);

    const names = items.map((item) => item.name);
    for (const node of ALWAYS_PRESENT_TREE_NODES) {
      expect(names, `"${node.name}" missing from the tree root`).toContain(node.name);
    }

    // Ids are what the frontend maps to entity types (seoToolkitTreeSource.ts), so a
    // changed casing or a renamed GUID silently breaks navigation. Assert them too.
    const ids = items.map((item) => item.id.toLowerCase());
    for (const node of ALWAYS_PRESENT_TREE_NODES) {
      expect(ids, `id for "${node.name}" missing from the tree root`).toContain(node.id);
    }
  });

  test('the Domains node appears and disappears with the domains @smoke', async ({ umbracoApi }) => {
    // SeoToolkitTreeController.GetRoot appends the Domains node only when at least one
    // SeoToolkit domain (or unmapped Umbraco domain) exists, so it is absent on a fresh
    // install. Drive both directions rather than asserting whatever happens to be true.
    const before = await treeRootItems(umbracoApi);
    expect(
      hasDomainRoot(before),
      'expected no Domains node before creating one - is a domain left over from a previous run?',
    ).toBe(false);

    const saveResponse = await umbracoApi.post(ApiRoute.domainSave, {
      name: testDomainName,
      domainIds: [],
      settings: {},
    });
    expect(saveResponse.status(), 'could not create a SeoToolkit domain').toBe(200);
    const createdId = (await saveResponse.text()).replaceAll('"', '');

    try {
      const during = await treeRootItems(umbracoApi);
      expect(hasDomainRoot(during), 'the Domains node did not appear after creating a domain').toBe(true);
    } finally {
      const deleteResponse = await umbracoApi.delete(`${ApiRoute.domainDelete}?domainId=${createdId}`);
      expect(deleteResponse.status(), 'could not delete the test domain').toBe(200);
    }

    const after = await treeRootItems(umbracoApi);
    expect(hasDomainRoot(after), 'the Domains node lingered after the domain was deleted').toBe(false);
  });

  test('every tree node renders in the sidebar @smoke', async ({ umbracoUi, page }) => {
    await umbracoUi.goToBackOffice();
    await page.goto(SEOTOOLKIT_SECTION_URL);

    const sidebar = page.locator('umb-section-sidebar');
    await expect(sidebar).toBeVisible();

    for (const node of ALWAYS_PRESENT_TREE_NODES) {
      await expect(
        sidebar.getByRole('link', { name: node.name, exact: true }),
        `"${node.name}" is not rendered in the sidebar tree`,
      ).toBeVisible();
    }
  });
});
