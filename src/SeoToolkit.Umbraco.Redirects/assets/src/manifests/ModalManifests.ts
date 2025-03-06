import { ManifestModal } from '@umbraco-cms/backoffice/modal';


const CreateRedirectModal: ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.redirect.create',
    name: 'SeoToolkit Redirect Create Modal',
    js: () => import('../modals/CreateRedirectModal.element'),
}

const CreateRedirectLinkModal: ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.redirect.link',
    name: 'SeoToolkit Redirect Create Link Modal',
    js: () => import('../modals/CreateRedirectLinkModal.element'),
}

export const ModalManifest = [CreateRedirectModal, CreateRedirectLinkModal];