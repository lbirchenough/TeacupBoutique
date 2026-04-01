import { authStore } from './authStore'
import type { BookingDetail, BookingListItem, MaintenanceQueueItem, SpareStockItem } from './types'

const INVENTORY_API_BASE_URL = import.meta.env.VITE_INVENTORY_API_URL || 'http://localhost:5054'

export interface SetItemAssessmentDto {
    setItemId: string
    quantityGood: number
    quantityDamaged: number
    quantityMissing: number
}

export interface BookingItemAssessmentDto {
    bookingItemId: string
    components: SetItemAssessmentDto[]
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

    markReturned: async (id: string, items: BookingItemAssessmentDto[]): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/return`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', ...authHeaders() },
            body: JSON.stringify({ items }),
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Failed to mark booking as returned (${response.status})`)
        }
    },

    markMissingItemReturned: async (bookingId: string, assessmentId: string): Promise<void> => {
        const response = await fetch(
            `${INVENTORY_API_BASE_URL}/api/bookings/${bookingId}/missing-items/${assessmentId}/returned`,
            { method: 'PUT', headers: { ...authHeaders() } }
        )
        if (!response.ok) throw new Error(`Failed to mark missing item as returned (${response.status})`)
    },


    markComponentReplaced: async (productSetId: string, setItemId: string): Promise<void> => {
        const response = await fetch(
            `${INVENTORY_API_BASE_URL}/api/maintenance/${productSetId}/components/${setItemId}/replaced`,
            { method: 'POST', headers: { ...authHeaders() } }
        )
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Failed to mark component replaced (${response.status})`)
        }
    },

    getMaintenanceQueue: async (): Promise<MaintenanceQueueItem[]> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/maintenance`, {
            headers: { ...authHeaders() },
        })
        if (!response.ok) throw new Error(`Failed to fetch maintenance queue (${response.status})`)
        return response.json()
    },

    markCleaned: async (bookingId: string): Promise<void> => {
        const response = await fetch(
            `${INVENTORY_API_BASE_URL}/api/bookings/${bookingId}/maintenance/mark-cleaned`,
            { method: 'POST', headers: { ...authHeaders() } }
        )
        if (!response.ok) throw new Error(`Failed to mark as cleaned (${response.status})`)
    },

    getAllSpareStock: async (): Promise<SpareStockItem[]> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/spare-stock`, {
            headers: { ...authHeaders() },
        })
        if (!response.ok) throw new Error(`Failed to fetch spare stock (${response.status})`)
        return response.json()
    },

    removeSpareStock: async (setItemId: string, quantity: number): Promise<void> => {
        const response = await fetch(
            `${INVENTORY_API_BASE_URL}/api/bookings/spare-stock/${setItemId}/remove`,
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', ...authHeaders() },
                body: JSON.stringify({ quantity }),
            }
        )
        if (!response.ok) throw new Error(`Failed to remove spare stock (${response.status})`)
    },


    addSpareStock: async (setItemId: string, quantity: number): Promise<void> => {
        const response = await fetch(
            `${INVENTORY_API_BASE_URL}/api/bookings/spare-stock/${setItemId}/add`,
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', ...authHeaders() },
                body: JSON.stringify({ quantity }),
            }
        )
        if (!response.ok) throw new Error(`Failed to add spare stock (${response.status})`)
    },

    complete: async (id: string, request: { completionNotes?: string; returnPhotoUrls?: string[] }): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${id}/complete`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', ...authHeaders() },
            body: JSON.stringify(request),
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Failed to complete booking (${response.status})`)
        }
    },

    releaseSet: async (productSetId: string): Promise<void> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/maintenance/${productSetId}/release`, {
            method: 'POST',
            headers: { ...authHeaders() },
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Failed to release set (${response.status})`)
        }
    },

    uploadPhoto: async (bookingId: string, file: File): Promise<string> => {
        const formData = new FormData()
        formData.append('file', file)
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/bookings/${bookingId}/photos`, {
            method: 'POST',
            headers: { ...authHeaders() },
            body: formData,
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Failed to upload photo (${response.status})`)
        }
        const data = await response.json()
        return data.url as string
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
