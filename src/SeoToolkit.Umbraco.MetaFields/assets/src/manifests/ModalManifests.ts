import { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { ST_AI_SUGGESTIONS_MODAL } from '../popups/MetaFieldsAISuggestionsModal.element';

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

const AIGenerateSuggestionsModalManifest: ManifestModal = {
    type: 'modal',
    alias: ST_AI_SUGGESTIONS_MODAL,
    name: 'SeoToolkit AI Suggestions',
    js: () => import('../popups/MetaFieldsAISuggestionsModal.element'),
};

export const ModalManifests = [ ItemGroupModalManifest, AIGenerateSuggestionsModalManifest, SchemaPickerModalManifest, SchemaPropertyModalManifest, SchemaSourceModalManifest ];
