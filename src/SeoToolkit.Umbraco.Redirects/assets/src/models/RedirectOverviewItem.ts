export interface RedirectOverviewItem {
    unique: string;
    entityType: string;

    id: string;
    isEnabled: boolean;
    domain?: (string) | null;
    oldUrl?: (string) | null;
    newUrl?: (string) | null;
    statusCode: number;
    lastUpdated?: (string) | null;
}