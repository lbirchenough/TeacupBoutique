import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { ordersApi } from '../lib/ordersApi'
import type { OrderDetail } from '../lib/types'

export const Route = createFileRoute('/my-orders')({
  component: MyOrdersPage,
})

const statusLabel: Record<string, string> = {
  Pending: 'Pending',
  AwaitingPayment: 'Awaiting Payment',
  Confirmed: 'Confirmed',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  OutOfStock: 'Out of Stock',
}

const statusColour: Record<string, string> = {
  Pending: 'text-gold',
  AwaitingPayment: 'text-gold',
  Confirmed: 'text-green-700',
  Completed: 'text-brown-light',
  Cancelled: 'text-red-500',
  OutOfStock: 'text-red-500',
}

function OrderRow({ order }: { order: OrderDetail }) {
  return (
    <Link
      to="/orders/$orderId"
      params={{ orderId: order.id }}
      className="block border border-gold/20 bg-white hover:border-gold/50 transition-colors p-5"
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs tracking-[0.15em] uppercase text-brown-light mb-1">{order.orderNumber}</p>
          <p className="font-serif text-lg text-brown">
            {order.orderItems && order.orderItems.length > 0
              ? order.orderItems[0].name
              : 'Order'}
            {order.orderItems && order.orderItems.length > 1 && (
              <span className="text-brown-light text-sm font-sans"> +{order.orderItems.length - 1} more</span>
            )}
          </p>
          <p className="text-sm text-brown-light mt-1">
            {new Date(order.reservationDate).toLocaleDateString('en-AU', { day: 'numeric', month: 'long', year: 'numeric' })}
          </p>
        </div>
        <div className="text-right shrink-0">
          <p className={`text-xs tracking-widest uppercase font-medium ${statusColour[order.status] ?? 'text-brown-light'}`}>
            {statusLabel[order.status] ?? order.status}
          </p>
          <p className="font-serif text-lg text-brown mt-1">${order.total.toFixed(2)}</p>
        </div>
      </div>
    </Link>
  )
}

function MyOrdersPage() {
  const { data: orders, isLoading, isError } = useQuery({
    queryKey: ['my-orders'],
    queryFn: ordersApi.getMyOrders,
  })

  return (
    <div className="min-h-screen bg-cream">
      <div className="max-w-3xl mx-auto px-6 py-16">

        <div className="mb-10">
          <p className="text-xs tracking-[0.2em] uppercase text-brown-light mb-2">Account</p>
          <h1 className="font-serif text-4xl text-brown">My Orders</h1>
          <div className="mt-3 h-px bg-gold/30" />
        </div>

        {isLoading && (
          <p className="text-sm text-brown-light text-center py-20">Loading your orders...</p>
        )}

        {isError && (
          <p className="text-sm text-red-500 text-center py-20">Failed to load orders. Please try again.</p>
        )}

        {orders && orders.length === 0 && (
          <div className="text-center py-20">
            <svg xmlns="http://www.w3.org/2000/svg" className="h-12 w-12 mx-auto text-gold/40 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z" />
            </svg>
            <p className="font-serif text-xl text-brown mb-2">No orders yet</p>
            <p className="text-sm text-brown-light mb-8">Your bookings will appear here once you've placed an order.</p>
            <Link
              to="/availability"
              className="text-xs tracking-widest uppercase bg-brown text-cream px-8 py-3 hover:bg-brown-mid transition-colors"
            >
              Book Now
            </Link>
          </div>
        )}

        {orders && orders.length > 0 && (
          <div className="flex flex-col gap-3">
            {orders.map(order => (
              <OrderRow key={order.id} order={order} />
            ))}
          </div>
        )}

        <div className="mt-8 pt-6 border-t border-gold/20">
          <Link
            to="/profile"
            className="inline-flex items-center gap-2 text-sm text-brown-light hover:text-brown transition-colors"
          >
            ← Back to Profile
          </Link>
        </div>

      </div>
    </div>
  )
}
