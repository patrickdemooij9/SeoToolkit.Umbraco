import { ManifestPropertyEditorUi } from "@umbraco-cms/backoffice/property-editor";

const fieldsEditorPropertyEditor: ManifestPropertyEditorUi = {
  type: "propertyEditorUi",
  alias: "SeoToolkit.FieldsEditor",
  name: "SeoToolkit FieldsEditor",
  element: () =>
    import("../propertyEditors/FieldsEditorPropertyEditor.element"),
  meta: {
    label: "Fields Editor",
    icon: 'icon-code',
    group: 'common',
    propertyEditorSchemaAlias: "Umbraco.Plain.String",
  },
};

const betterCheckboxListPropertyEditor: ManifestPropertyEditorUi = {
  type: "propertyEditorUi",
  alias: "SeoToolkit.SelectCheckboxList",
  name: "SeoToolkit SelectCheckboxList",
  element: () =>
    import("../propertyEditors/SelectCheckboxList.element"),
  meta: {
    label: "SelectCheckboxList",
    icon: 'icon-code',
    group: 'common',
    propertyEditorSchemaAlias: "Umbraco.CheckBoxList",
  },
};

const schemaEditorPropertyEditor: ManifestPropertyEditorUi = {
  type: "propertyEditorUi",
  alias: "SeoToolkit.SchemaEditor",
  name: "SeoToolkit Schema Editor",
  element: () =>
    import("../propertyEditors/SchemaEditorPropertyEditor.element"),
  meta: {
    label: "Schema Editor",
    icon: 'icon-sitemap',
    group: 'common',
    propertyEditorSchemaAlias: "Umbraco.Plain.String",
  },
};

export const PropertyEditorManifests = [ fieldsEditorPropertyEditor, betterCheckboxListPropertyEditor, schemaEditorPropertyEditor ];