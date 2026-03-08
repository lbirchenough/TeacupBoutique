import type { InventoryItemUpdateDto, ProductCreateDto, ProductUpdateDto } from './types'

const INVENTORY_API_BASE_URL = import.meta.env.VITE_INVENTORY_API_URL || 'http://localhost:5035'

async function handleResponse(response: Response) {
    if (!response.ok) {
        const message = await response.text()
        throw new Error(message || `Request failed with status ${response.status}`)
    }
    if (response.status === 204 || response.headers.get('content-length') === '0') {
        return null
    }
    return response.json()
}

export const inventoryApi = {
    // Products
    getProducts: async () => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products`)
        return handleResponse(response)
    },

    getProduct: async (productId: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}`)
        return handleResponse(response)
    },

    createProduct: async (dto: ProductCreateDto) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto),
        })
        return handleResponse(response)
    },

    updateProduct: async (productId: string, dto: ProductUpdateDto) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto),
        })
        return handleResponse(response)
    },

    // Inventory items
    getProductItems: async (productId: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/items`)
        return handleResponse(response)
    },

    createInventoryItem: async (productId: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/items`, {
            method: 'POST',
        })
        return handleResponse(response)
    },

    updateInventoryItem: async (productId: string, itemId: string, dto: InventoryItemUpdateDto) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/items/${itemId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto),
        })
        return handleResponse(response)
    },

    deleteInventoryItem: async (productId: string, itemId: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/items/${itemId}`, {
            method: 'DELETE',
        })
        return handleResponse(response)
    },
}
