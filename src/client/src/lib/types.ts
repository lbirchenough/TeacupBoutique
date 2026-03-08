export interface ProductListDto {
    id: string;
    name: string;
    description: string;
    colour?: string | null;
    price: number;
    servings: number;
    featuredPhotoUrl?: string | null;
}

export interface ProductDetail {
    id: string;
    name: string;
    description: string;
    contents?: string | null;
    colour?: string | null;
    price: number;
    depositAmount: number;
    minRentalDays: number;
    maxRentalDays: number;
    bufferDays: number;
    isActive: boolean;
    servings: number;
    createdAt: string;
    photos?: Photo[];
}

export interface Photo {
    id: string;
    url: string;
    isFeatured: boolean;
}

export type Condition = 'New' | 'Good' | 'Fair' | 'Poor';
export type ItemStatus = 'Available' | 'Maintenance' | 'Retired';

export interface InventoryItem {
    id: string;
    condition: Condition;
    status: ItemStatus;
    conditionNotes?: string | null;
    maintenanceHistory?: string | null;
}

export interface ProductCreateDto {
    name: string;
    description: string;
    contents?: string;
    colour?: string;
    price: number;
    depositAmount: number;
    minRentalDays: number;
    maxRentalDays: number;
    bufferDays: number;
    isActive: boolean;
    servings: number;
}

export interface ProductUpdateDto extends ProductCreateDto {}

export interface InventoryItemUpdateDto {
    id: string;
    condition: Condition;
    status: ItemStatus;
    conditionNotes?: string;
    maintenanceHistory?: string;
}
