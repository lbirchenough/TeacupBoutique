import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '../lib/ordersApi'
import { StripePaymentForm } from '../components/StripePaymentForm'
import type { OrderDetail, OrderStatus } from '../lib/types'

export const Route = createFileRoute('/orders/$orderId')({
    component: OrderDetailPage,
})

const statusConfig: Record<OrderStatus, { label: string; bg: string; text: string; description: string }> = {
    Pending:        { label: 'Checking availability…', bg: 'bg-amber-50',  text: 'text-amber-800',  description: 'We are confirming your items are available.' },
    AwaitingPayment:{ label: 'Awaiting Payment',       bg: 'bg-blue-50',   text: 'text-blue-800',   description: 'Your items are reserved. Complete payment to confirm your booking.' },
    Confirmed:      { label: 'Confirmed',              bg: 'bg-green-50',  text: 'text-green-800',  description: 'Your booking is confirmed. We look forward to making your event special.' },
    Completed:      { label: 'Completed',              bg: 'bg-cream-dark',text: 'text-brown-mid',  description: 'This order has been completed. Thank you for choosing Teacup Boutique.' },
    Cancelled:      { label: 'Cancelled',              bg: 'bg-red-50',    text: 'text-red-800',    description: 'This order was cancelled.' },
    OutOfStock:     { label: 'Out of Stock',           bg: 'bg-orange-50', text: 'text-orange-800', description: 'Sorry, one or more items became unavailable. Please try again with different dates or items.' },
}

const terminalStatuses: OrderStatus[] = ['Confirmed', 'Completed', 'Cancelled', 'OutOfStock']

function OrderDetailPage() {
    const { orderId } = Route.useParams()

    const { data: order, isPending, isError, error } = useQuery<OrderDetail>({
        queryKey: ['order', orderId],
        queryFn: () => ordersApi.getOrder(orderId),
        refetchInterval: (query) => {
            const status = query.state.data?.status
            if (!status || !terminalStatuses.includes(status)) return 2000
            return false
        },
    })

    if (isPending) return (
        <div className="max-w-3xl mx-auto px-6 py-20 text-center">
            <p className="font-serif text-lg text-brown-light">Loading your order…</p>
        </div>
    )
    if (isError) return (
        <div className="max-w-3xl mx-auto px-6 py-20 text-center">
            <p className="text-red-600">Error: {error.message}</p>
        </div>
    )

    const config = statusConfig[order.status] ?? statusConfig.Pending

    return (
        <div>
            {/* Page header */}
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">Order</p>
                <h1 className="font-serif text-4xl text-brown">{order.orderNumber}</h1>
                <p className="text-brown-light text-sm mt-2">
                    Placed {new Date(order.createdAt).toLocaleString('en-AU', { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' })}
                </p>
            </div>

            <div className="max-w-3xl mx-auto px-6 py-12 space-y-6">
                <Link to="/products" className="text-xs text-brown-light hover:text-brown transition-colors tracking-widest uppercase">
                    ← Continue Shopping
                </Link>

                {/* Status banner */}
                <div className={`border border-gold/20 px-6 py-5 ${config.bg}`}>
                    <div className="flex items-start gap-4">
                        {order.status === 'Pending' && (
                            <svg className="animate-spin h-5 w-5 shrink-0 mt-0.5 text-amber-600" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
                            </svg>
                        )}
                        <div>
                            <p className={`font-serif text-lg ${config.text}`}>{config.label}</p>
                            <p className={`text-sm mt-1 opacity-80 ${config.text}`}>{config.description}</p>
                            {order.status === 'Cancelled' && order.cancellationReason && (
                                <p className={`text-sm mt-2 font-medium ${config.text}`}>Reason: {order.cancellationReason}</p>
                            )}
                        </div>
                    </div>
                </div>

                {/* Customer details */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Booking Details</h2>
                    <div className="grid grid-cols-2 gap-x-8 gap-y-4 text-sm">
                        <Detail label="Name" value={order.customerName} />
                        <Detail label="Email" value={order.customerEmail} />
                        <Detail label="Phone" value={order.customerPhone} />
                        <Detail label="Event Date" value={new Date(order.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', { day: 'numeric', month: 'long', year: 'numeric' })} />
                    </div>
                </div>

                {/* Order items */}
                <div className="bg-white border border-gold/20 p-6">
                    <h2 className="font-serif text-lg text-brown mb-5">Items</h2>
                    <div className="space-y-4">
                        {order.orderItems?.map(item => (
                            <div key={item.id} className="flex gap-4 items-center">
                                <div className="w-14 h-14 bg-cream-dark overflow-hidden shrink-0 flex items-center justify-center">
                                    {item.imageUrl ? (
                                        <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                                    ) : (
                                        <span className="font-script text-sm text-brown-light">—</span>
                                    )}
                                </div>
                                <div className="flex-1 min-w-0">
                                    <p className="font-serif text-brown truncate">{item.name}</p>
                                    {item.colour && <p className="text-xs text-brown-light tracking-wide">{item.colour}</p>}
                                    <p className="text-xs text-brown-mid mt-0.5">Qty {item.quantity} × ${item.unitPrice.toFixed(2)}</p>
                                </div>
                                <p className="font-semibold text-brown shrink-0">${item.total.toFixed(2)}</p>
                            </div>
                        ))}
                    </div>

                    {/* Totals */}
                    <div className="border-t border-gold/20 mt-5 pt-5 space-y-2 text-sm">
                        <div className="flex justify-between text-brown-mid">
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

                {/* Payment */}
                {order.status === 'AwaitingPayment' && (
                    <div className="bg-white border border-gold p-6">
                        <h2 className="font-serif text-lg text-brown mb-1">Payment</h2>
                        <p className="text-sm text-brown-mid mb-5">
                            Total due: <span className="font-semibold text-brown text-base">${order.total.toFixed(2)}</span>
                        </p>
                        <StripePaymentForm
                            orderId={orderId}
                            amount={order.total}
                            onSuccess={() => {/* polling will update the status automatically */}}
                        />
                    </div>
                )}
            </div>
        </div>
    )
}

function Detail({ label, value }: { label: string; value: string }) {
    return (
        <div>
            <p className="text-xs font-semibold text-brown-light uppercase tracking-widest mb-1">{label}</p>
            <p className="text-brown font-medium">{value}</p>
        </div>
    )
}
