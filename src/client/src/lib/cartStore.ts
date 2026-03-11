import type { CartItem } from './types'

type Listener = () => void

let items: CartItem[] = []
let reservationDate: string | null = null
const listeners = new Set<Listener>()

function notify() {
    listeners.forEach(l => l())
}

export const cartStore = {
    subscribe(listener: Listener) {
        listeners.add(listener)
        return () => listeners.delete(listener)
    },

    getItems() {
        return items
    },

    addItem(item: CartItem) {
        const existing = items.find(i => i.productId === item.productId)
        if (existing) {
            items = items.map(i =>
                i.productId === item.productId ? { ...i, quantity: i.quantity + item.quantity } : i
            )
        } else {
            items = [...items, item]
        }
        notify()
    },

    removeItem(productId: string) {
        items = items.filter(i => i.productId !== productId)
        notify()
    },

    updateQuantity(productId: string, quantity: number) {
        if (quantity <= 0) {
            items = items.filter(i => i.productId !== productId)
        } else {
            items = items.map(i => i.productId === productId ? { ...i, quantity } : i)
        }
        notify()
    },

    clear() {
        items = []
        reservationDate = null
        notify()
    },

    getReservationDate() {
        return reservationDate
    },

    setReservationDate(date: string | null) {
        reservationDate = date
        notify()
    },

    getTotal() {
        return items.reduce((sum, i) => sum + i.pricePerDay * i.quantity, 0)
    },

    getCount() {
        return items.reduce((sum, i) => sum + i.quantity, 0)
    },
}
