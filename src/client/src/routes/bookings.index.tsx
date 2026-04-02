import { createFileRoute, Link } from '@tanstack/react-router'
import { useQuery } from '@tanstack/react-query'
import { bookingsApi } from '../lib/bookingsApi'
import type { BookingListItem, BookingStatus } from '../lib/types'

export const Route = createFileRoute('/bookings/')({
    component: BookingsPage,
})

const statusConfig: Record<BookingStatus, { label: string; bg: string; text: string; dot: string }> = {
    Reserved:   { label: 'Reserved',    bg: 'bg-amber-50',  text: 'text-amber-800',  dot: 'bg-amber-400' },
    Confirmed:  { label: 'Confirmed',   bg: 'bg-blue-50',   text: 'text-blue-800',   dot: 'bg-blue-400' },
    CheckedOut: { label: 'Checked Out', bg: 'bg-purple-50', text: 'text-purple-800', dot: 'bg-purple-400' },
    Returned:   { label: 'Returned',    bg: 'bg-green-50',  text: 'text-green-800',  dot: 'bg-green-400' },
    Completed:  { label: 'Completed',   bg: 'bg-cream-dark',text: 'text-brown-mid',  dot: 'bg-brown-light' },
    Cancelled:  { label: 'Cancelled',   bg: 'bg-red-50',    text: 'text-red-800',    dot: 'bg-red-400' },
}

const statusOrder: BookingStatus[] = ['CheckedOut', 'Confirmed', 'Reserved', 'Returned', 'Completed', 'Cancelled']

function BookingsPage() {
    const { data: bookings, isPending, isError } = useQuery<BookingListItem[]>({
        queryKey: ['bookings'],
        queryFn: bookingsApi.getBookings,
        refetchOnMount: 'always',
    })

    if (isPending) return (
        <div className="max-w-4xl mx-auto px-6 py-20 text-center">
            <p className="font-serif text-lg text-brown-light">Loading bookings…</p>
        </div>
    )

    if (isError) return (
        <div className="max-w-4xl mx-auto px-6 py-20 text-center">
            <p className="text-red-600">Failed to load bookings.</p>
        </div>
    )

    const grouped = statusOrder.reduce<Record<BookingStatus, BookingListItem[]>>((acc, status) => {
        let items = bookings?.filter(b => b.status === status) ?? []
        if (status === 'Confirmed') {
            items = [...items].sort((a, b) =>
                new Date(a.reservationDate).getTime() - new Date(b.reservationDate).getTime()
            )
        }
        acc[status] = items
        return acc
    }, {} as Record<BookingStatus, BookingListItem[]>)

    const hasAny = statusOrder.some(s => grouped[s].length > 0)

    return (
        <div>
            <div className="bg-cream-dark border-b border-gold/20 py-16 text-center">
                <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">Management</p>
                <h1 className="font-serif text-4xl text-brown">Bookings</h1>
            </div>

            <div className="max-w-4xl mx-auto px-6 py-12 space-y-10">
                {!hasAny && (
                    <p className="text-center text-brown-light font-serif text-lg py-10">No active bookings.</p>
                )}

                {statusOrder.map(status => {
                    const items = grouped[status]
                    if (items.length === 0) return null
                    const config = statusConfig[status]

                    return (
                        <section key={status}>
                            <div className="flex items-center gap-3 mb-4">
                                <span className={`w-2 h-2 rounded-full ${config.dot}`} />
                                <h2 className="font-serif text-xl text-brown">{config.label}</h2>
                                <span className="text-sm text-brown-light">({items.length})</span>
                            </div>

                            <div className="space-y-2">
                                {items.map(booking => (
                                    <Link
                                        key={booking.id}
                                        to="/bookings/$bookingId"
                                        params={{ bookingId: booking.id }}
                                        className="flex items-center justify-between bg-white border border-gold/20 px-5 py-4 hover:border-gold/50 transition-colors group"
                                    >
                                        <div className="flex items-center gap-6">
                                            <div>
                                                <p className="text-xs text-brown-light uppercase tracking-widest mb-0.5">Reservation Date</p>
                                                <p className="font-serif text-brown">
                                                    {new Date(booking.reservationDate + 'T00:00:00').toLocaleDateString('en-AU', {
                                                        day: 'numeric', month: 'long', year: 'numeric'
                                                    })}
                                                </p>
                                            </div>
                                            <div>
                                                <p className="text-xs text-brown-light uppercase tracking-widest mb-0.5">Items</p>
                                                <p className="text-brown-mid text-sm">{booking.itemCount}</p>
                                            </div>
                                            <div>
                                                <p className="text-xs text-brown-light uppercase tracking-widest mb-0.5">Order</p>
                                                <p className="text-brown-mid text-sm font-mono text-xs">{booking.orderId.slice(0, 8)}…</p>
                                            </div>
                                        </div>
                                        <svg className="h-4 w-4 text-brown-light group-hover:text-brown transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                                            <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
                                        </svg>
                                    </Link>
                                ))}
                            </div>
                        </section>
                    )
                })}
            </div>
        </div>
    )
}
