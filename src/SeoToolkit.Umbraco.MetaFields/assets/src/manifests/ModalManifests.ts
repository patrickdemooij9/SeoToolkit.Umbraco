import { ManifestModal } from '@umbraco-cms/backoffice/modal';

const ItemGroupModalManifest : ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.itemGroupPicker',
    name: 'SeoToolkit ItemGroupPicker',
    js: () => import('../popups/ItemGroupPicker.element'),
}

const SchemaPickerModalManifest : ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.schemaPicker',
    name: 'SeoToolkit SchemaPicker',
    js: () => import('../popups/SchemaPickerModal.element'),
}

const SchemaPropertyModalManifest : ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.schemaProperty',
    name: 'SeoToolkit SchemaProperty',
    js: () => import('../popups/SchemaPropertyModal.element'),
}

const SchemaSourceModalManifest : ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.schemaSource',
    name: 'SeoToolkit SchemaSource',
    js: () => import('../popups/SchemaSourceModal.element'),
}

export const ModalManifests = [ ItemGroupModalManifest, SchemaPickerModalManifest, SchemaPropertyModalManifest, SchemaSourceModalManifest] ;