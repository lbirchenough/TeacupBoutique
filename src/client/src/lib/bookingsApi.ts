import { authStore } from './authStore'
import type { BookingDetail, BookingListItem, ReturnCondition } from './types'

const INVENTORY_API_BASE_URL = import.meta.env.VITE_INVENTORY_API_URL || 'http://localhost:5054'

export interface CompleteBookingRequest {
    depositAmountKept: number | null
    completionNotes: string | null
    returnPhotoUrls: string[]
}

export interface BookingItemReturnDto {
    bookingItemId: string
    returnCondition: ReturnCondition
    returnNotes?: string | null
}

function authHeaders(): HeadersInit {
    const token = authStore.getAccessToken()
    return token ? { Authorization: `Bearer ${token}` } : {}
}

export const bookingsApi = {
    getBookings: async (): Promise<BookingListItem[]> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings`, {
            headers: { ...authHeaders() },
        })
        if (!response.ok) throw new Error(`Failed to fetch bookings (${response.status})`)
        return response.json()
    },

    getBooking: async (id: string): Promise<BookingDetail> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}`, {
            headers: { ...authHeaders() },
        })
        if (!response.ok) throw new Error(`Failed to fetch booking (${response.status})`)
        return response.json()
    },

    checkOut: async (id: string): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/checkout`, {
            method: 'PUT',
            headers: { ...authHeaders() },
        })
        if (!response.ok) throw new Error(`Failed to check out booking (${response.status})`)
    },

    markReturned: async (id: string, items: BookingItemReturnDto[]): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/return`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', ...authHeaders() },
            body: JSON.stringify({ items }),
        })
        if (!response.ok) throw new Error(`Failed to mark booking as returned (${response.status})`)
    },

    complete: async (id: string, request: CompleteBookingRequest): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/complete`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', ...authHeaders() },
            body: JSON.stringify(request),
        })
        if (!response.ok) throw new Error(`Failed to complete booking (${response.status})`)
    },

    uploadPhoto: async (bookingId: string, file: File): Promise<{ url: string }> => {
        const formData = new FormData()
        formData.append('file', file)
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${bookingId}/photos`, {
            method: 'POST',
            headers: { ...authHeaders() },
            body: formData,
        })
        if (!response.ok) throw new Error(`Failed to upload photo (${response.status})`)
        return response.json()
    },

    cancelBooking: async (id: string): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/cancel`, {
            method: 'PUT',
            headers: { ...authHeaders() },
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Failed to cancel booking (${response.status})`)
        }
    },
}
