const PAYMENTS_API_BASE_URL = import.meta.env.VITE_PAYMENTS_API_URL || 'http://localhost:5127'

export const paymentsApi = {
    capture: async (orderId: string): Promise<void> => {
        const response = await fetch(`${PAYMENTS_API_BASE_URL}/payments/${orderId}/capture`, {
            method: 'POST',
        })
        if (!response.ok) {
            const message = await response.text()
            throw new Error(message || `Payment failed with status ${response.status}`)
        }
    },
    fail: async (orderId: string): Promise<void> => {
        const response = await fetch(`${PAYMENTS_API_BASE_URL}/payments/${orderId}/fail`, {
            method: 'POST',
        })
        if (!response.ok) throw new Error(`Payment declined (${response.status})`)
    },
}
