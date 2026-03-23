import { authStore } from './authStore'
import type { CreateOrderRequest, OrderDetail } from './types'

const ORDERS_API_BASE_URL = import.meta.env.VITE_ORDERS_API_URL || 'http://localhost:5054'

export const ordersApi = {
    createOrder: async (dto: CreateOrderRequest) => {
        const response = await fetch(`${ORDERS_API_BASE_URL}/api/orders`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto),
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Request failed with status ${response.status}`)
        }
        return response.json() as Promise<OrderDetail>
    },

    getOrder: async (orderNumber: string, accessToken?: string): Promise<OrderDetail> => {
        const jwt = authStore.getAccessToken()
        const url = accessToken
            ? `${ORDERS_API_BASE_URL}/api/orders/${orderNumber}?token=${accessToken}`
            : `${ORDERS_API_BASE_URL}/api/orders/${orderNumber}`
        const response = await fetch(url, {
            headers: jwt ? { Authorization: `Bearer ${jwt}` } : {},
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(`${response.status}: ${message || response.statusText}`)
        }
        return response.json()
    },

    getMyOrders: async (): Promise<OrderDetail[]> => {
        const token = authStore.getAccessToken()
        const response = await fetch(`${ORDERS_API_BASE_URL}/api/orders/mine`, {
            headers: { Authorization: `Bearer ${token}` },
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Request failed with status ${response.status}`)
        }
        return response.json()
    },

    claimOrders: async (): Promise<void> => {
        const token = authStore.getAccessToken()
        await fetch(`${ORDERS_API_BASE_URL}/api/orders/claim`, {
            method: 'PATCH',
            headers: { Authorization: `Bearer ${token}` },
        })
    },
}
