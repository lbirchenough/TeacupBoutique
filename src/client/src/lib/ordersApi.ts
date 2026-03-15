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

    getOrder: async (orderId: string): Promise<OrderDetail> => {
        const response = await fetch(`${ORDERS_API_BASE_URL}/api/orders/${orderId}`)
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Request failed with status ${response.status}`)
        }
        return response.json()
    },
}
