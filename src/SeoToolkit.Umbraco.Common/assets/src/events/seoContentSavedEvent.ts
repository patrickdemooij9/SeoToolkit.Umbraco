export const SEO_CONTENT_SAVED_EVENT_TYPE = "seo-content-saved";

export interface SeoContentSavedEventDetail {
    unique: string;
    cultures: string[];
}

export class SeoContentSavedEvent extends CustomEvent<SeoContentSavedEventDetail> {
    constructor(detail: SeoContentSavedEventDetail) {
        super(SEO_CONTENT_SAVED_EVENT_TYPE, { detail });
    }
}
