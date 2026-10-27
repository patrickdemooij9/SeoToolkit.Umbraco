import { UmbContextToken, type UmbContextMinimal } from "@umbraco-cms/backoffice/context-api";
import type { Observable } from "@umbraco-cms/backoffice/external/rxjs";

/**
 * An entry in the side navigation of the document type SEO screen.
 * Feature packages register these for their own `seoToolkitDocumentView`.
 */
export interface SeoDocumentNavItem {
  key: string;
  label: string;
  icon?: string;
  badge?: string;
}

export interface SeoDocumentNavigation extends UmbContextMinimal {
  /**
   * Sets the navigation items shown under a document view, keyed by the view's `meta.pathname`.
   * A view without items is shown as a single entry.
   */
  setNavItems(viewPathname: string, items: SeoDocumentNavItem[]): void;

  /**
   * The key of the selected item of a document view, or undefined when none is selected
   * (the view should then show its first item).
   */
  activeNavItem(viewPathname: string): Observable<string | undefined>;
}

// Matched by alias, so feature packages declare an identical token instead of importing this file.
export const ST_SEO_DOCUMENT_NAVIGATION_CONTEXT =
  new UmbContextToken<SeoDocumentNavigation>("ST-SeoDocumentNavigation-Context");
