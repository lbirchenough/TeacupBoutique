const INVENTORY_API_BASE_URL = import.meta.env.VITE_INVENTORY_API_URL || 'http://localhost:5035'

export const inventoryApi = {
  getProducts: async () => {
    console.log(`Getting products from inventory API: ${INVENTORY_API_BASE_URL}/api/products`)
    const response = await fetch(`${INVENTORY_API_BASE_URL}/api/products`);

    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'An error occurred while getting products')
    }
    return await response.json();
  },
}