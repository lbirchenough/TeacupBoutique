import { useSyncExternalStore } from 'react'
import { cartStore } from './cartStore'

export function useCart() {
    const items = useSyncExternalStore(cartStore.subscribe, cartStore.getItems)
    const reservationDate = useSyncExternalStore(cartStore.subscribe, cartStore.getReservationDate)

    return {
        items,
        count: items.reduce((sum, i) => sum + i.quantity, 0),
        total: items.reduce((sum, i) => sum + i.pricePerDay * i.quantity, 0),
        reservationDate,
        addItem: cartStore.addItem,
        removeItem: cartStore.removeItem,
        updateQuantity: cartStore.updateQuantity,
        clear: cartStore.clear,
        setReservationDate: cartStore.setReservationDate,
    }
}
