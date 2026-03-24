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
    const { items, total, count, removeItem, updateQuantity, clear, reservationDate: cartDate } = useCart()
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
            navigate({ to: '/orders/$orderNumber', params: { orderNumber: order.orderNumber }, search: { token: order.accessToken } })
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
            <div>
                {/* Header */}
                <div className="text-center pt-16 pb-10 px-6">
                    <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Your Cart</p>
                    <h1 className="font-script text-6xl text-brown">Nothing here yet</h1>
                    <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
                        Your cart is empty — browse the collection or pick a date to get started.
                    </p>
                    <div className="flex items-center justify-center gap-3 mt-5">
                        <div className="h-px w-12 bg-gold/40" />
                        <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
                        <div className="h-px w-12 bg-gold/40" />
                    </div>
                </div>
                <div className="max-w-md mx-auto px-6 pb-20 text-center">
                    <div className="flex flex-col sm:flex-row gap-4 justify-center">
                        <Link
                            to="/availability"
                            className="inline-block bg-brown text-cream px-8 py-3.5 text-xs font-semibold tracking-[0.15em] uppercase hover:bg-brown-mid transition-colors"
                        >
                            Book Now
                        </Link>
                        <Link
                            to="/products"
                            className="inline-block border border-brown/30 text-brown px-8 py-3.5 text-xs font-semibold tracking-[0.15em] uppercase hover:bg-cream-dark transition-colors"
                        >
                            Our Collection
                        </Link>
                    </div>
                </div>
            </div>
        )
    }

    return (
        <div>
            {/* Header */}
            <div className="text-center pt-16 pb-10 px-6">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Review</p>
                <h1 className="font-script text-6xl text-brown">Your Cart</h1>
                <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
                    Review your selections and complete your booking below.
                </p>
                <div className="flex items-center justify-center gap-3 mt-5">
                    <div className="h-px w-12 bg-gold/40" />
                    <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
                    <div className="h-px w-12 bg-gold/40" />
                </div>
            </div>

            <div className="max-w-5xl mx-auto px-6 pt-6 pb-12">
                <div className="grid grid-cols-1 gap-10 lg:grid-cols-5">
                    {/* Cart items — left col */}
                    <div className="lg:col-span-3 space-y-4">
                        {items.map(item => (
                            <div key={item.productId} className="flex gap-4 bg-white border border-gold/20 p-4">
                                <div className="w-16 h-16 bg-cream-dark overflow-hidden shrink-0 flex items-center justify-center">
                                    {item.imageUrl ? (
                                        <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                                    ) : (
                                        <span className="font-script text-sm text-brown-light">—</span>
                                    )}
                                </div>
                                <div className="flex-1 min-w-0">
                                    <p className="font-serif text-brown truncate">{item.name}</p>
                                    {item.colour && <p className="text-xs text-brown-light mt-0.5 tracking-wide">{item.colour}</p>}
                                    <p className="text-sm text-brown-mid mt-1">${item.pricePerDay.toFixed(2)}</p>
                                </div>
                                <div className="flex flex-col items-end gap-2">
                                    <p className="font-semibold text-brown">
                                        ${(item.pricePerDay * item.quantity).toFixed(2)}
                                    </p>
                                    <div className="flex items-center gap-2">
                                        <button
                                            onClick={() => updateQuantity(item.productId, item.quantity - 1)}
                                            className="w-7 h-7 border border-brown/20 text-brown-mid hover:bg-cream-dark text-sm transition-colors"
                                        >
                                            −
                                        </button>
                                        <span className="text-sm text-brown w-4 text-center">{item.quantity}</span>
                                        <button
                                            onClick={() => updateQuantity(item.productId, item.quantity + 1)}
                                            className="w-7 h-7 border border-brown/20 text-brown-mid hover:bg-cream-dark text-sm transition-colors"
                                        >
                                            +
                                        </button>
                                    </div>
                                    <button
                                        onClick={() => removeItem(item.productId)}
                                        className="text-xs text-brown-light hover:text-red-600 transition-colors"
                                    >
                                        Remove
                                    </button>
                                </div>
                            </div>
                        ))}

                        {/* Add more */}
                        <div className="text-right">
                            <Link
                                to="/availability"
                                className="text-sm text-gold hover:text-gold/70 font-medium transition-colors"
                            >
                                + Add more items
                            </Link>
                        </div>

                        {/* Totals */}
                        <div className="bg-white border border-gold/20 p-5 space-y-3 text-sm">
                            {totalServings > 0 && (
                                <div className="flex justify-between text-brown-mid pb-3 border-b border-gold/20">
                                    <span>Serves</span>
                                    <span className="font-medium text-brown">{totalServings} people</span>
                                </div>
                            )}
                            <div className="flex justify-between font-semibold text-brown text-base">
                                <span>Total</span>
                                <span>${grandTotal.toFixed(2)}</span>
                            </div>
                            <div className="flex justify-between text-brown-light text-xs">
                                <span>GST included (10%)</span>
                                <span>${gst.toFixed(2)}</span>
                            </div>
                            {totalDeposit > 0 && (
                                <div className="flex justify-between text-brown-mid pt-3 border-t border-gold/20">
                                    <span>Security deposit</span>
                                    <span className="font-medium">${totalDeposit.toFixed(2)}</span>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Order form — right col */}
                    <div className="lg:col-span-2">
                        <form onSubmit={handleSubmit} className="bg-white border border-gold/20 p-6 space-y-5">
                            <h2 className="font-serif text-xl text-brown">Your Details</h2>

                            {orderMutation.isError && (
                                <p className="text-sm text-red-600">{orderMutation.error.message}</p>
                            )}

                            <Field label="Full Name *">
                                <input
                                    required
                                    type="text"
                                    value={form.customerName}
                                    onChange={e => setForm(f => ({ ...f, customerName: e.target.value }))}
                                    className="w-full border border-brown/20 bg-cream px-3 py-2.5 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20"
                                />
                            </Field>
                            <Field label="Email *">
                                <input
                                    required
                                    type="email"
                                    value={form.customerEmail}
                                    onChange={e => setForm(f => ({ ...f, customerEmail: e.target.value }))}
                                    className="w-full border border-brown/20 bg-cream px-3 py-2.5 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20"
                                />
                            </Field>
                            <Field label="Phone *">
                                <input
                                    required
                                    type="tel"
                                    value={form.customerPhone}
                                    onChange={e => setForm(f => ({ ...f, customerPhone: e.target.value }))}
                                    className="w-full border border-brown/20 bg-cream px-3 py-2.5 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20"
                                />
                            </Field>
                            <Field label="Reservation Date *">
                                {cartDate && !showDateChangeConfirm ? (
                                    <div className="flex items-center gap-2">
                                        <span className="flex-1 border border-gold/40 bg-gold-pale px-3 py-2.5 text-sm text-brown font-medium">
                                            {new Date(cartDate + 'T00:00:00').toLocaleDateString('en-AU', { day: 'numeric', month: 'long', year: 'numeric' })}
                                        </span>
                                        <button
                                            type="button"
                                            onClick={() => setShowDateChangeConfirm(true)}
                                            className="text-xs text-brown-light hover:text-brown transition-colors"
                                        >
                                            Change
                                        </button>
                                    </div>
                                ) : cartDate && showDateChangeConfirm ? (
                                    <div className="border border-amber-200 bg-amber-50 p-3 space-y-2">
                                        <p className="text-xs text-amber-800">This will clear your cart and take you back to the availability page.</p>
                                        <div className="flex gap-2">
                                            <button
                                                type="button"
                                                onClick={() => setShowDateChangeConfirm(false)}
                                                className="flex-1 text-xs border border-brown/20 text-brown py-1.5 hover:bg-cream-dark transition-colors"
                                            >
                                                Cancel
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => { clear(); navigate({ to: '/availability' }) }}
                                                className="flex-1 text-xs bg-amber-600 hover:bg-amber-700 text-white py-1.5 transition-colors"
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
                                        className="w-full border border-brown/20 bg-cream px-3 py-2.5 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20"
                                    />
                                )}
                            </Field>

                            <button
                                type="submit"
                                disabled={orderMutation.isPending}
                                className="w-full bg-brown hover:bg-brown-mid text-cream py-3.5 text-xs font-semibold tracking-[0.15em] uppercase disabled:opacity-50 transition-colors mt-2"
                            >
                                {orderMutation.isPending ? 'Placing Order…' : 'Place Order'}
                            </button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
    return (
        <div>
            <label className="block text-xs font-semibold text-brown-light uppercase tracking-wider mb-1.5">{label}</label>
            {children}
        </div>
    )
}
