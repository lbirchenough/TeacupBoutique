import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { bookingsApi, type BookingItemReturnDto } from '../lib/bookingsApi'
import { ordersApi } from '../lib/ordersApi'
import type { BookingDetail, BookingStatus, OrderDetail, ReturnCondition } from '../lib/types'

export const Route = createFileRoute('/bookings/$bookingId')({
    component: BookingDetailPage,
})

const statusConfig: Record<BookingStatus, { label: string; bg: string; text: string }> = {
    Reserved:   { label: 'Reserved',    bg: 'bg-amber-50',  text: 'text-amber-800' },
    Confirmed:  { label: 'Confirmed',   bg: 'bg-blue-50',   text: 'text-blue-800' },
    CheckedOut: { label: 'Checked Out', bg: 'bg-purple-50', text: 'text-purple-800' },
    Returned:   { label: 'Returned',    bg: 'bg-green-50',  text: 'text-green-800' },
    Completed:  { label: 'Completed',   bg: 'bg-cream-dark',text: 'text-brown-mid' },
    Cancelled:  { label: 'Cancelled',   bg: 'bg-red-50',    text: 'text-red-800' },
}

const conditionOptions: ReturnCondition[] = ['Good', 'Damaged', 'MissingItems']
const conditionLabel: Record<ReturnCondition, string> = {
    Good: 'Good',
    Damaged: 'Damaged',
    MissingItems: 'Missing Items',
}

function BookingDetailPage() {
    const { bookingId } = Route.useParams()
    const queryClient = useQueryClient()

    const [showReturnForm, setShowReturnForm] = useState(false)
    const [returnItems, setReturnItems] = useState<Record<string, { condition: ReturnCondition; notes: string }>>({})

    const { data: booking, isPending, isError } = useQuery<BookingDetail>({
        queryKey: ['booking', bookingId],
        queryFn: () => bookingsApi.getBooking(bookingId),
    })

    const { data: order } = useQuery<OrderDetail>({
        queryKey: ['order', booking?.orderId],
        queryFn: () => ordersApi.getOrder(booking!.orderId),
        enabled: !!booking?.orderId,
    })

    const invalidate = () => queryClient.invalidateQueries({ queryKey: ['booking', bookingId] })

    const checkOutMutation = useMutation({
        mutationFn: () => bookingsApi.checkOut(bookingId),
        onSuccess: invalidate,
    })

    const returnMutation = useMutation({
        mutationFn: () => {
            const items: BookingItemReturnDto[] = booking!.bookingItems.map(item => ({
                bookingItemId: item.id,
                returnCondition: returnItems[item.id]?.condition ?? 'Good',
                returnNotes: returnItems[item.id]?.notes || null,
            }))
            return bookingsApi.markReturned(bookingId, items)
        },
        onSuccess: () => {
            setShowReturnForm(false)
            invalidate()
        },
    })

    const completeMutation = useMutation({
        mutationFn: () => bookingsApi.complete(bookingId),
        onSuccess: invalidate,
    })

    if (isPending) return (
        <div className="max-w-3xl mx-auto px-6 py-20 text-center">
            <p className="font-serif text-lg text-brown-light">Loading booking…</p>
        </div>
    )

    if (isError || !booking) return (
        <div className="max-w-3xl mx-auto px-6 py-20 text-center">
            <p className="text-red-600">Booking not found.</p>
        </div>
    )

    const config = statusConfig[booking.status]

    const initReturnForm = () => {
        const initial: Record<string, { condition: ReturnCondition; notes: string }> = {}
        booking.bookingItems.forEach(item => {
            initial[item.id] = { condition: 'Good', notes: '' }
        })
        setReturnItems(initial)
        setShowReturnForm(true)
    }

    return (
        <div>
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">Booking</p>
                <h1 className="font-serif text-3xl text-brown">{booking.id.slice(0, 8).toUpperCase()}</h1>
                <p className="text-brown-light text-sm mt-2">
                    Created {new Date(booking.createdAt).toLocaleDateString('en-AU', {
                        day: 'numeric', month: 'long', year: 'numeric'
                    })}
                </p>
            </div>

            <div className="max-w-3xl mx-auto px-6 py-12 space-y-6">
                <Link to="/bookings" className="text-xs text-brown-light hover:text-brown transition-colors tracking-widest uppercase">
                    ← All Bookings
                </Link>

                {/* Status banner + action buttons */}
                <div className={`border border-gold/20 px-6 py-5 ${config.bg} flex items-center justify-between`}>
                    <p className={`font-serif text-lg ${config.text}`}>{config.label}</p>
                    <div className="flex gap-3">
                        {booking.status === 'Confirmed' && (
                            <ActionButton
                                label="Mark as Checked Out"
                                onClick={() => checkOutMutation.mutate()}
                                isPending={checkOutMutation.isPending}
                            />
                        )}
                        {booking.status === 'CheckedOut' && !showReturnForm && (
                            <ActionButton
                                label="Mark as Returned"
                                onClick={initReturnForm}
                                isPending={false}
                            />
                        )}
                        {booking.status === 'Returned' && (
                            <ActionButton
                                label="Complete"
                                onClick={() => completeMutation.mutate()}
                                isPending={completeMutation.isPending}
                            />
                        )}
                    </div>
                </div>

                {/* Booking details */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Details</h2>
                    <div className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm">
                        <Detail label="Reservation Date" value={
                            new Date(booking.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', {
                                day: 'numeric', month: 'long', year: 'numeric'
                            })
                        } />
                        <Detail label="Order ID" value={booking.orderId} mono />
                        {booking.confirmedAt && (
                            <Detail label="Confirmed" value={new Date(booking.confirmedAt).toLocaleString('en-AU')} />
                        )}
                        {booking.checkedOutAt && (
                            <Detail label="Checked Out" value={new Date(booking.checkedOutAt).toLocaleString('en-AU')} />
                        )}
                        {booking.returnedAt && (
                            <Detail label="Returned" value={new Date(booking.returnedAt).toLocaleString('en-AU')} />
                        )}
                        {booking.completedAt && (
                            <Detail label="Completed" value={new Date(booking.completedAt).toLocaleString('en-AU')} />
                        )}
                        {booking.notes && (
                            <div className="col-span-2">
                                <Detail label="Notes" value={booking.notes} />
                            </div>
                        )}
                    </div>
                </div>

                {/* Items — inline return form or read-only */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Items</h2>

                    {showReturnForm ? (
                        <div className="space-y-4">
                            {booking.bookingItems.map(item => (
                                <div key={item.id} className="border border-gold/20 p-4 space-y-3">
                                    <div>
                                        <p className="font-serif text-brown">{item.productName}</p>
                                        {item.productColour && (
                                            <p className="text-xs text-brown-light">{item.productColour}</p>
                                        )}
                                    </div>
                                    <div className="flex gap-3">
                                        {conditionOptions.map(c => (
                                            <button
                                                key={c}
                                                type="button"
                                                onClick={() => setReturnItems(prev => ({
                                                    ...prev,
                                                    [item.id]: { ...prev[item.id], condition: c }
                                                }))}
                                                className={`text-xs px-3 py-1.5 border transition-colors ${
                                                    returnItems[item.id]?.condition === c
                                                        ? c === 'Good' ? 'bg-green-100 border-green-400 text-green-800'
                                                          : c === 'Damaged' ? 'bg-red-100 border-red-400 text-red-800'
                                                          : 'bg-amber-100 border-amber-400 text-amber-800'
                                                        : 'border-gold/20 text-brown-light hover:border-gold/50'
                                                }`}
                                            >
                                                {conditionLabel[c]}
                                            </button>
                                        ))}
                                    </div>
                                    {returnItems[item.id]?.condition !== 'Good' && (
                                        <input
                                            type="text"
                                            placeholder="Notes (optional)"
                                            value={returnItems[item.id]?.notes ?? ''}
                                            onChange={e => setReturnItems(prev => ({
                                                ...prev,
                                                [item.id]: { ...prev[item.id], notes: e.target.value }
                                            }))}
                                            className="w-full border border-gold/20 px-3 py-2 text-sm text-brown placeholder-brown-light/50 focus:outline-none focus:border-gold/50"
                                        />
                                    )}
                                </div>
                            ))}
                            <div className="flex gap-3 pt-2">
                                <button
                                    onClick={() => returnMutation.mutate()}
                                    disabled={returnMutation.isPending}
                                    className="bg-brown text-cream text-sm px-5 py-2 hover:bg-brown-mid transition-colors disabled:opacity-50"
                                >
                                    {returnMutation.isPending ? 'Saving…' : 'Confirm Return'}
                                </button>
                                <button
                                    onClick={() => setShowReturnForm(false)}
                                    className="text-sm text-brown-light hover:text-brown transition-colors px-3"
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {booking.bookingItems.map(item => (
                                <div key={item.id} className="flex items-center justify-between py-3 border-b border-gold/10 last:border-0">
                                    <div>
                                        <p className="font-serif text-brown">{item.productName}</p>
                                        {item.productColour && (
                                            <p className="text-xs text-brown-light mt-0.5">{item.productColour}</p>
                                        )}
                                        <p className="text-xs text-brown-light font-mono mt-0.5">Item {item.inventoryItemId.slice(0, 8)}</p>
                                        {item.returnNotes && (
                                            <p className="text-xs text-brown-mid mt-1 italic">"{item.returnNotes}"</p>
                                        )}
                                    </div>
                                    {item.returnCondition && (
                                        <span className={`text-xs px-2 py-1 font-medium ${
                                            item.returnCondition === 'Good' ? 'bg-green-50 text-green-700' :
                                            item.returnCondition === 'Damaged' ? 'bg-red-50 text-red-700' :
                                            'bg-amber-50 text-amber-700'
                                        }`}>
                                            {conditionLabel[item.returnCondition]}
                                        </span>
                                    )}
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Order details */}
                {order && (
                    <div className="bg-white border border-gold/20 p-6">
                        <h2 className="font-serif text-lg text-brown mb-5">Order</h2>
                        <div className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm mb-6">
                            <Detail label="Order Number" value={order.orderNumber} />
                            <Detail label="Status" value={order.status} />
                            <Detail label="Customer" value={order.customerName} />
                            <Detail label="Email" value={order.customerEmail} />
                            <Detail label="Phone" value={order.customerPhone} />
                            <Detail label="Event Date" value={
                                new Date(order.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', {
                                    day: 'numeric', month: 'long', year: 'numeric'
                                })
                            } />
                        </div>
                        <div className="border-t border-gold/20 pt-4 space-y-2 text-sm">
                            {order.orderItems?.map(item => (
                                <div key={item.id} className="flex justify-between text-brown-mid">
                                    <span>{item.name} × {item.quantity}</span>
                                    <span>${item.total.toFixed(2)}</span>
                                </div>
                            ))}
                            <div className="flex justify-between text-brown-light text-xs pt-2 border-t border-gold/10">
                                <span>Subtotal (ex GST)</span><span>${order.subtotal.toFixed(2)}</span>
                            </div>
                            <div className="flex justify-between text-brown-light text-xs">
                                <span>GST (10%)</span><span>${order.tax.toFixed(2)}</span>
                            </div>
                            <div className="flex justify-between font-semibold text-brown text-base pt-2 border-t border-gold/20">
                                <span>Total</span><span>${order.total.toFixed(2)}</span>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}

function ActionButton({ label, onClick, isPending }: { label: string; onClick: () => void; isPending: boolean }) {
    return (
        <button
            onClick={onClick}
            disabled={isPending}
            className="text-xs tracking-widest uppercase border border-brown/30 text-brown px-4 py-2 hover:bg-cream-dark transition-colors disabled:opacity-50"
        >
            {isPending ? '…' : label}
        </button>
    )
}

function Detail({ label, value, mono = false }: { label: string; value: string; mono?: boolean }) {
    return (
        <div>
            <p className="text-xs font-semibold text-brown-light uppercase tracking-widest mb-1">{label}</p>
            <p className={`text-brown font-medium ${mono ? 'font-mono text-xs break-all' : ''}`}>{value}</p>
        </div>
    )
}
