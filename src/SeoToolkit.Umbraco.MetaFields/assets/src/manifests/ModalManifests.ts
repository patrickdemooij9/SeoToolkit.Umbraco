import { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { ST_AI_SUGGESTIONS_MODAL } from '../actions/MetaFieldsAIGenerateAction';

const ItemGroupModalManifest : ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.itemGroupPicker',
    name: 'SeoToolkit ItemGroupPicker',
    js: () => import('../popups/ItemGroupPicker.element'),
}

const AIGenerateSuggestionsModalManifest: ManifestModal = {
    type: 'modal',
    alias: ST_AI_SUGGESTIONS_MODAL,
    name: 'SeoToolkit AI Suggestions',
    js: () => import('../popups/MetaFieldsAISuggestionsModal.element'),
};

export const ModalManifests = [ ItemGroupModalManifest, AIGenerateSuggestionsModalManifest ];