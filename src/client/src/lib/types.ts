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

// Orders (response from API)
export type OrderStatus = 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled' | 'AwaitingPayment' | 'OutOfStock'
export type PaymentStatus = 'Pending' | 'Paid' | 'Refunded'

export interface OrderDetail {
    id: string;
    orderNumber: string;
    userId?: string | null;
    customerName: string;
    customerEmail: string;
    customerPhone: string;
    status: OrderStatus;
    subtotal: number;
    tax: number;
    total: number;
    pickupDate: string;
    returnDate: string;
    reservationDate: string;
    paymentStatus?: PaymentStatus | null;
    paymentId?: string | null;
    cancelledAt?: string | null;
    cancellationReason?: string | null;
    createdAt: string;
    updatedAt: string;
    confirmedAt?: string | null;
    completedAt?: string | null;
    orderItems?: OrderItemDetail[];
}

export interface OrderItemDetail {
    id: string;
    productId: string;
    quantity: number;
    reservationDate: string;
    name: string;
    colour?: string | null;
    imageUrl?: string | null;
    unitPrice: number;
    total: number;
}

// Cart
export interface CartItem {
    productId: string;
    name: string;
    pricePerDay: number;
    quantity: number;
    colour?: string | null;
    imageUrl?: string | null;
}

// Orders
export interface CreateOrderRequest {
    customerName: string;
    customerEmail: string;
    customerPhone: string;
    reservationDate: string;
    pickupDate: string;
    returnDate: string;
    subtotal: number;
    tax: number;
    total: number;
    items: CreateOrderItemRequest[];
}

export interface CreateOrderItemRequest {
    productId: string;
    quantity: number;
    productName: string;
    productThemeColor?: string;
    productImageUrl?: string;
    pricePerDay: number;
    subtotal: number;
    rentalDate: string;
}
