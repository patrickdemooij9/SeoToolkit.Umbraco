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

const ImportRedirectLinkModal: ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.redirect.import',
    name: 'SeoToolkit Redirect Import Modal',
    js: () => import('../modals/ImportRedirectsModal.element'),
}

const RedirectStatusCodeModal: ManifestModal = {
    type: 'modal',
    alias: 'seoToolkit.modal.redirect.statusCode',
    name: 'SeoToolkit Redirect Status Code Modal',
    js: () => import('../modals/RedirectStatusCodeModal.element'),
}

export const ModalManifest = [CreateRedirectModal, CreateRedirectLinkModal, ImportRedirectLinkModal, RedirectStatusCodeModal];