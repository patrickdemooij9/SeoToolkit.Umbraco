export interface Redirect { 
    id: number;
    unique: string;
    entityType: string;
    isEnabled: boolean;
    isRegex: boolean;
    domain?: number | null;
    customDomain?: string | null;
    oldUrl?: string | null;
    newUrl?: string | null;
    newNodeId?: string | null;
    newCultureIso?: string | null;
    redirectCode: number;
    lastUpdated?: (string) | null;
}