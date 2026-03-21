import { createFileRoute, Link } from '@tanstack/react-router'

export const Route = createFileRoute('/my-orders')({
  component: MyOrdersPage,
})

function MyOrdersPage() {
  // TODO: fetch user's orders from backend when endpoint is available

  return (
    <div className="min-h-screen bg-cream">
      <div className="max-w-3xl mx-auto px-6 py-16">

        <div className="mb-10">
          <p className="text-xs tracking-[0.2em] uppercase text-brown-light mb-2">Account</p>
          <h1 className="font-serif text-4xl text-brown">My Orders</h1>
          <div className="mt-3 h-px bg-gold/30" />
        </div>

        {/* Empty state */}
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
