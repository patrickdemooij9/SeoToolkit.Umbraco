import { UmbContextToken, type UmbContextMinimal } from "@umbraco-cms/backoffice/context-api";
import type { Observable } from "@umbraco-cms/backoffice/external/rxjs";
import { SeoFieldSettingsGroupViewModel, SeoFieldViewModel } from "../api";

export type MetaFieldsSettingsGroup = SeoFieldSettingsGroupViewModel & {
  fields: SeoFieldViewModel[];
};

const FALLBACK_GROUP: SeoFieldSettingsGroupViewModel = {
  alias: "others",
  name: "Other",
  icon: "icon-settings",
};

export function groupSettingsFields(
  fields: SeoFieldViewModel[],
  groups: SeoFieldSettingsGroupViewModel[]
): MetaFieldsSettingsGroup[] {
  const result: MetaFieldsSettingsGroup[] = groups.map((group) => ({
    ...group,
    fields: fields.filter((field) => field.settingsGroupAlias === group.alias),
  }));

  // Fields without a known group are still shown, at the end.
  const ungrouped = fields.filter(
    (field) => !groups.some((group) => group.alias === field.settingsGroupAlias)
  );
  if (ungrouped.length) {
    const fallback = result.find((group) => group.alias === FALLBACK_GROUP.alias);
    if (fallback) {
      fallback.fields.push(...ungrouped);
    } else {
      result.push({ ...FALLBACK_GROUP, fields: ungrouped });
    }
  }

  return result.filter((group) => group.fields.length > 0);
}

export function isFieldConfigured(field: SeoFieldViewModel) {
  return field.useInheritedValue || !isEmptyValue(field.value);
}

// Treats nested empty values as empty too, e.g. the schema field's { schemas: [] }.
function isEmptyValue(value: unknown): boolean {
  if (value === null || value === undefined || value === "") return true;
  if (Array.isArray(value)) return value.every((item) => isEmptyValue(item));
  if (typeof value === "object") {
    return Object.values(value as object).every((item) => isEmptyValue(item));
  }
  return false;
}

// Mirrors the navigation contract of the shared SEO document view in SeoToolkit.Umbraco.Common
// (seoDocumentNavigation.ts). Contexts are matched by alias, so this resolves to the same instance.
export interface SeoDocumentNavItem {
  key: string;
  label: string;
  icon?: string;
  badge?: string;
}

export interface SeoDocumentNavigation extends UmbContextMinimal {
  setNavItems(viewPathname: string, items: SeoDocumentNavItem[]): void;
  activeNavItem(viewPathname: string): Observable<string | undefined>;
}

export const ST_SEO_DOCUMENT_NAVIGATION_CONTEXT =
  new UmbContextToken<SeoDocumentNavigation>("ST-SeoDocumentNavigation-Context");

export const METAFIELDS_VIEW_PATHNAME = "metaFields";
