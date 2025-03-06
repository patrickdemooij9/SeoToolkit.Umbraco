import { ManifestModal } from '@umbraco-cms/backoffice/modal';

const ItemGroupModalManifest : ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.itemGroupPicker',
    name: 'SeoToolkit ItemGroupPicker',
    js: () => import('../popups/ItemGroupPicker.element'),
}

export const ModalManifests = [ ItemGroupModalManifest] ;