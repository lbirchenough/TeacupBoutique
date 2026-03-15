const PAYMENTS_API_BASE_URL = import.meta.env.VITE_PAYMENTS_API_URL || 'http://localhost:5054'

export const paymentsApi = {
    createIntent: async (orderId: string, amount: number): Promise<{ clientSecret: string }> => {
        const response = await fetch(`${PAYMENTS_API_BASE_URL}/payments/create-intent`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ orderId, amount }),
        })
        if (!response.ok) throw new Error(`Failed to create payment intent (${response.status})`)
        return response.json()
    },
}
