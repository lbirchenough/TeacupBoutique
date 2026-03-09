import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '../lib/ordersApi'
import type { OrderDetail, OrderStatus } from '../lib/types'

export const Route = createFileRoute('/orders/$orderId')({
    component: OrderDetailPage,
})

const statusConfig: Record<OrderStatus, { label: string; colour: string; description: string }> = {
    Pending:        { label: 'Checking availability…', colour: 'bg-yellow-100 text-yellow-700', description: 'We are confirming your items are available.' },
    AwaitingPayment:{ label: 'Awaiting Payment',       colour: 'bg-blue-100 text-blue-700',    description: 'Your items are reserved. Complete payment to confirm your booking.' },
    Confirmed:      { label: 'Confirmed',              colour: 'bg-green-100 text-green-700',  description: 'Your booking is confirmed.' },
    Completed:      { label: 'Completed',              colour: 'bg-gray-100 text-gray-600',    description: 'This order has been completed.' },
    Cancelled:      { label: 'Cancelled',              colour: 'bg-red-100 text-red-700',      description: 'This order was cancelled.' },
}

const terminalStatuses: OrderStatus[] = ['AwaitingPayment', 'Confirmed', 'Completed', 'Cancelled']

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

    if (isPending) return <div className="max-w-3xl mx-auto px-4 py-16 text-gray-500">Loading…</div>
    if (isError) return <div className="max-w-3xl mx-auto px-4 py-16 text-red-600">Error: {error.message}</div>

    const config = statusConfig[order.status] ?? statusConfig.Pending

    return (
        <div className="max-w-3xl mx-auto px-4 py-8">
            <Link to="/products" className="text-sm text-indigo-600 hover:text-indigo-800 mb-6 inline-block">
                ← Continue shopping
            </Link>

            {/* Status banner */}
            <div className={`rounded-xl px-5 py-4 mb-6 ${config.colour}`}>
                <div className="flex items-center gap-3">
                    {order.status === 'Pending' && (
                        <svg className="animate-spin h-5 w-5 shrink-0" fill="none" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" />
                        </svg>
                    )}
                    <div>
                        <p className="font-semibold">{config.label}</p>
                        <p className="text-sm mt-0.5 opacity-80">{config.description}</p>
                        {order.status === 'Cancelled' && order.cancellationReason && (
                            <p className="text-sm mt-1 font-medium">Reason: {order.cancellationReason}</p>
                        )}
                    </div>
                </div>
            </div>

            {/* Order header */}
            <div className="bg-white border border-gray-200 rounded-xl p-6 mb-5 shadow-sm">
                <div className="flex items-start justify-between mb-4">
                    <div>
                        <h1 className="text-xl font-bold text-gray-900">{order.orderNumber}</h1>
                        <p className="text-sm text-gray-500 mt-0.5">
                            Placed {new Date(order.createdAt).toLocaleString()}
                        </p>
                    </div>
                </div>

                <div className="grid grid-cols-2 gap-x-8 gap-y-2 text-sm">
                    <Detail label="Name" value={order.customerName} />
                    <Detail label="Email" value={order.customerEmail} />
                    <Detail label="Phone" value={order.customerPhone} />
                    <Detail label="Reservation Date" value={order.reservationDate} />
                </div>
            </div>

            {/* Order items */}
            <div className="bg-white border border-gray-200 rounded-xl p-6 mb-5 shadow-sm">
                <h2 className="font-semibold text-gray-800 mb-4">Items</h2>
                <div className="space-y-3">
                    {order.orderItems?.map(item => (
                        <div key={item.id} className="flex gap-4 items-center">
                            <div className="w-12 h-12 bg-gray-100 rounded-lg overflow-hidden shrink-0 flex items-center justify-center">
                                {item.imageUrl ? (
                                    <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                                ) : (
                                    <span className="text-gray-400 text-xs">—</span>
                                )}
                            </div>
                            <div className="flex-1 min-w-0">
                                <p className="font-medium text-gray-900 truncate">{item.name}</p>
                                {item.colour && <p className="text-xs text-gray-500">{item.colour}</p>}
                                <p className="text-xs text-gray-500">Qty: {item.quantity} × ${item.unitPrice.toFixed(2)}/day</p>
                            </div>
                            <p className="font-semibold text-gray-900 shrink-0">${item.total.toFixed(2)}</p>
                        </div>
                    ))}
                </div>

                {/* Totals */}
                <div className="border-t border-gray-100 mt-4 pt-4 space-y-1.5 text-sm">
                    <div className="flex justify-between text-gray-600">
                        <span>Subtotal</span><span>${order.subtotal.toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between text-gray-600">
                        <span>Tax</span><span>${order.tax.toFixed(2)}</span>
                    </div>
                    <div className="flex justify-between font-semibold text-gray-900 text-base pt-1 border-t border-gray-100">
                        <span>Total</span><span>${order.total.toFixed(2)}</span>
                    </div>
                </div>
            </div>

            {/* Payment placeholder */}
            {order.status === 'AwaitingPayment' && (
                <div className="bg-white border border-indigo-200 rounded-xl p-6 shadow-sm">
                    <h2 className="font-semibold text-gray-800 mb-2">Payment</h2>
                    <p className="text-sm text-gray-500">Payment integration coming soon.</p>
                </div>
            )}
        </div>
    )
}

function Detail({ label, value }: { label: string; value: string }) {
    return (
        <div>
            <span className="text-gray-500">{label}: </span>
            <span className="text-gray-900 font-medium">{value}</span>
        </div>
    )
}
