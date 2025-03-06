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

export const PropertyEditorManifests = [ fieldsEditorPropertyEditor, betterCheckboxListPropertyEditor ];