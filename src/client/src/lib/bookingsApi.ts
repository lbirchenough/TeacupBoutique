import type { BookingDetail, BookingListItem, ReturnCondition } from './types'

const INVENTORY_API_BASE_URL = import.meta.env.VITE_INVENTORY_API_URL || 'http://localhost:5035'

export interface BookingItemReturnDto {
    bookingItemId: string
    returnCondition: ReturnCondition
    returnNotes?: string | null
}

export const bookingsApi = {
    getBookings: async (): Promise<BookingListItem[]> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings`)
        if (!response.ok) throw new Error(`Failed to fetch bookings (${response.status})`)
        return response.json()
    },

    getBooking: async (id: string): Promise<BookingDetail> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}`)
        if (!response.ok) throw new Error(`Failed to fetch booking (${response.status})`)
        return response.json()
    },

    checkOut: async (id: string): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/checkout`, { method: 'PUT' })
        if (!response.ok) throw new Error(`Failed to check out booking (${response.status})`)
    },

    markReturned: async (id: string, items: BookingItemReturnDto[]): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/return`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ items }),
        })
        if (!response.ok) throw new Error(`Failed to mark booking as returned (${response.status})`)
    },

    complete: async (id: string): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/complete`, { method: 'PUT' })
        if (!response.ok) throw new Error(`Failed to complete booking (${response.status})`)
    },
}
