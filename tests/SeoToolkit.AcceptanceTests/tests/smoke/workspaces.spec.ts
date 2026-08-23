import { test, expect } from '../../helpers/test';
import { ALWAYS_PRESENT_TREE_NODES, SEOTOOLKIT_SECTION_URL } from '../../helpers/seoToolkitConstants';

/**
 * The highest-value spec in the suite: opening every tree node forces each of the
 * eight backoffice bundles to load its workspace element. A broken vite build, a bad
 * entry path in ManifestLoader.cs, or a renamed Umbraco extension type all fail here.
 *
 * Nodes are opened by clicking the tree rather than by URL so the test also covers
 * tree-item routing, and does not bake in workspace URL formats that differ between
 * routable and non-routable workspace kinds.
 */
test.describe('SeoToolkit workspaces', () => {
  for (const node of ALWAYS_PRESENT_TREE_NODES) {
    test(`the ${node.name} workspace opens from the tree @smoke`, async ({ umbracoUi, page }) => {
      await umbracoUi.goToBackOffice();
      await page.goto(SEOTOOLKIT_SECTION_URL);

      const sidebar = page.locator('umb-section-sidebar');
      await expect(sidebar).toBeVisible();

      await sidebar.getByRole('link', { name: node.name, exact: true }).click();

      await expect(
        page.locator(node.element),
        `${node.name} should render <${node.element}>`,
      ).toBeVisible();

      // The workspace chrome loading without its content would still pass the check
      // above in some layouts, so confirm the node id actually reached the router.
      await expect(page).toHaveURL(new RegExp(node.id, 'i'));
    });
  }
});
