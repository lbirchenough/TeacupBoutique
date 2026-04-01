export interface ProductListDto {
    id: string;
    name: string;
    description: string;
    colour?: string | null;
    price: number;
    depositAmount: number;
    servings: number;
    featuredPhotoUrl?: string | null;
    contents?: string | null;
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

export type ItemStatus = 'Available' | 'Maintenance' | 'Retired';

export interface ProductSet {
    id: string;
    name: string;
    status: ItemStatus;
}

export interface SetItemDetail {
    id: string;
    name: string;
    quantity: number;
    depositValuePerUnit: number;
    spareStock: number;
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

// Orders (response from API)
export type OrderStatus = 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled' | 'AwaitingPayment' | 'OutOfStock' | 'PendingMissingItems'
export type PaymentStatus = 'Pending' | 'Paid'
export type RefundStatus = 'None' | 'DepositRefunded' | 'DepositPartiallyRefunded' | 'FullyRefunded'

export interface OrderDetail {
    id: string;
    orderNumber: string;
    accessToken: string;
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
    refundStatus?: RefundStatus | null;
    amountRefunded?: number | null;
    depositTotal: number;
    cancelledAt?: string | null;
    cancellationReason?: string | null;
    depositAmountKept?: number | null;
    completionNotes?: string | null;
    returnPhotoUrls?: string[];
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
    depositAmount: number;
}

// Bookings
export type BookingStatus = 'Reserved' | 'Confirmed' | 'CheckedOut' | 'Returned' | 'Completed' | 'Cancelled'

export interface ReturnAssessmentDetail {
    id: string
    setItemId: string
    setItemName?: string | null
    quantityGood: number
    quantityDamaged: number
    quantityMissing: number
    replacedAt?: string | null
    quantityCustomerReturned: number
}

export interface BookingItemDetail {
    id: string
    reservationDate: string
    returnNotes?: string | null
    returnedAt?: string | null
    completedAt?: string | null
    productId: string
    productName: string
    productColour?: string | null
    productSetId: string
    productSetName?: string | null
    returnAssessments?: ReturnAssessmentDetail[]
}

export interface BookingDetail {
    id: string
    orderId: string
    reservationDate: string
    status: BookingStatus
    reservedAt: string
    confirmedAt?: string | null
    checkedOutAt?: string | null
    returnedAt?: string | null
    completedAt?: string | null
    notes?: string | null
    completionNotes?: string | null
    depositAmountKept?: number | null
    createdAt: string
    setItemsByProduct?: Record<string, SetItemDetail[]>
    bookingItems: BookingItemDetail[]
}

export interface BookingListItem {
    id: string
    orderId: string
    reservationDate: string
    status: BookingStatus
    reservedAt: string
    confirmedAt?: string | null
    checkedOutAt?: string | null
    returnedAt?: string | null
    completedAt?: string | null
    notes?: string | null
    createdAt: string
    itemCount: number
}

export interface MaintenanceComponentDetail {
    setItemId: string
    setItemName?: string | null
    totalDamaged: number
    totalMissing: number
    spareStockAvailable: number
    isReplaced: boolean
}

export interface MaintenanceQueueItem {
    productSetId: string
    productSetName?: string | null
    productId: string
    productName?: string | null
    bookingId: string
    returnedAt?: string | null
    isCleaned: boolean
    components: MaintenanceComponentDetail[]
    upcomingBookings: { reservationDate: string; daysUntil: number }[]
    nextBookingDays?: number | null
}

export interface SpareStockItem {
    id: string
    setItemId: string
    setItemName: string
    productId: string
    productName: string
    quantityAvailable: number
}

export interface SetItemCreateDto {
    name: string
    quantity: number
    depositValuePerUnit: number
}

// Availability
export interface ProductAvailabilityDto {
    productId: string
    name: string
    featuredPhotoUrl?: string | null
    pricePerDay: number
    depositAmount: number
    servings: number
    available: number
    total: number
}

// Cart
export interface CartItem {
    productId: string;
    name: string;
    pricePerDay: number;
    depositAmount: number;
    quantity: number;
    servings: number;
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
    items: CreateOrderItemRequest[];
    turnstileToken: string;
}

export interface CreateOrderItemRequest {
    productId: string;
    quantity: number;
    productName: string;
    productThemeColor?: string;
    productImageUrl?: string;
    rentalDate: string;
}
