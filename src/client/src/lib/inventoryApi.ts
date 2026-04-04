import type { ProductAvailabilityDto, ProductCreateDto, ProductUpdateDto, SetItemCreateDto } from './types'

const INVENTORY_API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5054'

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

    createProduct: async (dto: ProductCreateDto, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify(dto),
        })
        return handleResponse(response)
    },

    updateProduct: async (productId: string, dto: ProductUpdateDto, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify(dto),
        })
        return handleResponse(response)
    },

    // Product sets (physical instances)
    getProductSets: async (productId: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/sets`)
        return handleResponse(response)
    },

    createProductSet: async (productId: string, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/sets`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
        })
        return handleResponse(response)
    },

    // Set items (component definitions — write-once)
    getSetItems: async (productId: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/set-items`)
        return handleResponse(response)
    },

    createSetItem: async (productId: string, dto: SetItemCreateDto, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/set-items`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify(dto),
        })
        return handleResponse(response)
    },

    deactivateSetItem: async (productId: string, setItemId: string, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/set-items/${setItemId}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` },
        })
        return handleResponse(response)
    },

    activateSetItem: async (productId: string, setItemId: string, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/set-items/${setItemId}/activate`, {
            method: 'PUT',
            headers: { 'Authorization': `Bearer ${token}` },
        })
        return handleResponse(response)
    },

    getAvailability: async (date: string): Promise<ProductAvailabilityDto[]> => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/availability?date=${date}`)
        return handleResponse(response)
    },

    // Product photos
    uploadProductPhoto: async (productId: string, file: File, token: string) => {
        const formData = new FormData()
        formData.append('file', file)
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/photos`, {
            method: 'POST',
            headers: { 'Authorization': `Bearer ${token}` },
            body: formData,
        })
        return handleResponse(response)
    },

    deleteProductPhoto: async (productId: string, photoId: string, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/photos/${photoId}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` },
        })
        return handleResponse(response)
    },

    setFeaturedPhoto: async (productId: string, photoId: string, token: string) => {
        const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products/${productId}/photos/${photoId}/featured`, {
            method: 'PUT',
            headers: { 'Authorization': `Bearer ${token}` },
        })
        return handleResponse(response)
    },
}
