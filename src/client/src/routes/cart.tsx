import { createFileRoute, Link, useNavigate } from '@tanstack/react-router'
import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { useCart } from '../lib/useCart'
import { ordersApi } from '../lib/ordersApi'
import type { CreateOrderRequest } from '../lib/types'

export const Route = createFileRoute('/cart')({
    component: CartPage,
})

const TAX_RATE = 0.1

function CartPage() {
    const { items, total, count, removeItem, updateQuantity, clear, reservationDate: cartDate, setReservationDate } = useCart()
    const navigate = useNavigate()

    const grandTotal = total  // prices are GST-inclusive
    const gst = grandTotal * TAX_RATE
    const exGst = grandTotal - gst
    const totalDeposit = items.reduce((sum, i) => sum + (i.depositAmount ?? 0) * i.quantity, 0)
    const totalServings = items.reduce((sum, i) => sum + (i.servings ?? 0) * i.quantity, 0)

    const [form, setForm] = useState({
        customerName: '',
        customerEmail: '',
        customerPhone: '',
        reservationDate: cartDate ?? '',
    })
    const [showDateChangeConfirm, setShowDateChangeConfirm] = useState(false)

    const orderMutation = useMutation({
        mutationFn: (dto: CreateOrderRequest) => ordersApi.createOrder(dto),
        onSuccess: (order) => {
            clear()
            navigate({ to: '/orders/$orderId', params: { orderId: order.id } })
        },
    })

    function handleSubmit(e: React.FormEvent) {
        e.preventDefault()
        if (items.length === 0) return

        const dto: CreateOrderRequest = {
            customerName: form.customerName,
            customerEmail: form.customerEmail,
            customerPhone: form.customerPhone,
            reservationDate: form.reservationDate,
            pickupDate: form.reservationDate,
            returnDate: form.reservationDate,
            subtotal: exGst,
            tax: gst,
            total: grandTotal,
            items: items.map(i => ({
                productId: i.productId,
                quantity: i.quantity,
                productName: i.name,
                productThemeColor: i.colour ?? undefined,
                productImageUrl: i.imageUrl ?? undefined,
                pricePerDay: i.pricePerDay,
                subtotal: i.pricePerDay * i.quantity,
                rentalDate: form.reservationDate,
            })),
        }

        orderMutation.mutate(dto)
    }

    if (count === 0) {
        return (
            <div className="max-w-3xl mx-auto px-4 py-16 text-center">
                <p className="text-gray-500 text-lg mb-4">Your cart is empty.</p>
                <Link to="/products" className="text-indigo-600 hover:text-indigo-800 font-medium">
                    Browse products →
                </Link>
            </div>
        )
    }

    return (
        <div className="max-w-4xl mx-auto px-4 py-8">
            <h1 className="text-2xl font-bold text-gray-900 mb-8">Your Cart</h1>

            <div className="grid grid-cols-1 gap-8 lg:grid-cols-5">
                {/* Cart items — left col */}
                <div className="lg:col-span-3 space-y-3">
                    {items.map(item => (
                        <div key={item.productId} className="flex gap-4 bg-white border border-gray-200 rounded-xl p-4">
                            <div className="w-16 h-16 bg-gray-100 rounded-lg overflow-hidden shrink-0 flex items-center justify-center">
                                {item.imageUrl ? (
                                    <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                                ) : (
                                    <span className="text-gray-400 text-xs">No photo</span>
                                )}
                            </div>
                            <div className="flex-1 min-w-0">
                                <p className="font-medium text-gray-900 truncate">{item.name}</p>
                                {item.colour && <p className="text-xs text-gray-500">{item.colour}</p>}
                                <p className="text-sm text-gray-600 mt-0.5">${item.pricePerDay.toFixed(2)}</p>
                            </div>
                            <div className="flex flex-col items-end gap-2">
                                <p className="font-semibold text-gray-900">
                                    ${(item.pricePerDay * item.quantity).toFixed(2)}
                                </p>
                                <div className="flex items-center gap-2">
                                    <button
                                        onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                                        className="w-6 h-6 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 text-sm"
                                    >
                                        −
                                    </button>
                                    <span className="text-sm w-4 text-center">{item.quantity}</span>
                                    <button
                                        onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                                        className="w-6 h-6 rounded border border-gray-300 text-gray-600 hover:bg-gray-50 text-sm"
                                    >
                                        +
                                    </button>
                                </div>
                                <button
                                    onClick={() => removeItem(item.productId)}
                                    className="text-xs text-red-500 hover:text-red-700"
                                >
                                    Remove
                                </button>
                            </div>
                        </div>
                    ))}

                    {/* Add more items */}
                    <div className="text-right">
                        <Link
                            to="/availability"
                            className="text-sm text-indigo-600 hover:text-indigo-800 font-medium"
                        >
                            + Add more items
                        </Link>
                    </div>

                    {/* Totals */}
                    <div className="bg-gray-50 border border-gray-200 rounded-xl p-4 space-y-2 text-sm">
                        {totalServings > 0 && (
                            <div className="flex justify-between text-indigo-700 font-medium pb-2 border-b border-gray-200">
                                <span>Serves</span>
                                <span>{totalServings} people</span>
                            </div>
                        )}
                        <div className="flex justify-between font-semibold text-gray-900 text-base">
                            <span>Total</span>
                            <span>${grandTotal.toFixed(2)}</span>
                        </div>
                        <div className="flex justify-between text-gray-500 text-xs">
                            <span>GST included (10%)</span>
                            <span>${gst.toFixed(2)}</span>
                        </div>
                        {totalDeposit > 0 && (
                            <div className="flex justify-between text-gray-700 font-medium pt-2 border-t border-gray-200">
                                <span>Security deposit</span>
                                <span>${totalDeposit.toFixed(2)}</span>
                            </div>
                        )}
                    </div>
                </div>

                {/* Order form — right col */}
                <div className="lg:col-span-2">
                    <form onSubmit={handleSubmit} className="bg-white border border-gray-200 rounded-xl p-5 space-y-4">
                        <h2 className="font-semibold text-gray-800">Your Details</h2>

                        {orderMutation.isError && (
                            <p className="text-sm text-red-600">{orderMutation.error.message}</p>
                        )}

                        <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">Full Name *</label>
                            <input
                                required
                                type="text"
                                value={form.customerName}
                                onChange={e => setForm(f => ({ ...f, customerName: e.target.value }))}
                                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">Email *</label>
                            <input
                                required
                                type="email"
                                value={form.customerEmail}
                                onChange={e => setForm(f => ({ ...f, customerEmail: e.target.value }))}
                                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">Phone *</label>
                            <input
                                required
                                type="tel"
                                value={form.customerPhone}
                                onChange={e => setForm(f => ({ ...f, customerPhone: e.target.value }))}
                                className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-medium text-gray-600 mb-1">Reservation Date *</label>
                            {cartDate && !showDateChangeConfirm ? (
                                <div className="flex items-center gap-2">
                                    <span className="flex-1 rounded-md border border-indigo-300 bg-indigo-50 px-3 py-2 text-sm text-indigo-800 font-medium">
                                        {new Date(cartDate + 'T00:00:00').toLocaleDateString('en-AU', { day: 'numeric', month: 'long', year: 'numeric' })}
                                    </span>
                                    <button
                                        type="button"
                                        onClick={() => setShowDateChangeConfirm(true)}
                                        className="text-xs text-gray-400 hover:text-gray-600"
                                    >
                                        Change
                                    </button>
                                </div>
                            ) : cartDate && showDateChangeConfirm ? (
                                <div className="rounded-md border border-amber-200 bg-amber-50 p-3 space-y-2">
                                    <p className="text-xs text-amber-800">This will clear your cart and take you back to the availability page.</p>
                                    <div className="flex gap-2">
                                        <button
                                            type="button"
                                            onClick={() => setShowDateChangeConfirm(false)}
                                            className="flex-1 text-xs border border-gray-300 text-gray-700 py-1.5 rounded-md hover:bg-gray-50"
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => { clear(); navigate({ to: '/availability' }) }}
                                            className="flex-1 text-xs bg-amber-600 hover:bg-amber-700 text-white py-1.5 rounded-md"
                                        >
                                            Yes, clear cart
                                        </button>
                                    </div>
                                </div>
                            ) : (
                                <input
                                    required
                                    type="date"
                                    value={form.reservationDate}
                                    onChange={e => setForm(f => ({ ...f, reservationDate: e.target.value }))}
                                    className="w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                />
                            )}
                        </div>

                        <button
                            type="submit"
                            disabled={orderMutation.isPending}
                            className="w-full rounded-md bg-indigo-600 px-4 py-2.5 text-sm font-medium text-white hover:bg-indigo-700 disabled:opacity-50"
                        >
                            {orderMutation.isPending ? 'Placing Order…' : 'Place Order'}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    )
}
