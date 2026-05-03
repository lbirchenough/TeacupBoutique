import { createRootRoute, Link, Outlet } from '@tanstack/react-router'
import { Navbar } from '../components/Navbar'
import { Footer } from '../components/Footer'
import { WakeBanner } from '../components/WakeBanner'

export const Route = createRootRoute({
  component: () => (
    <div className="min-h-screen bg-cream flex flex-col">
      <WakeBanner />
      <Navbar />
      <main className="flex-1 bg-checker bg-cream">
        <Outlet />
      </main>
      <Footer />
    </div>
  ),
  notFoundComponent: () => (
    <div className="flex flex-col items-center justify-center py-32 px-6 text-center">
      <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-4">404</p>
      <h1 className="font-serif text-4xl text-brown mb-4">Page Not Found</h1>
      <p className="text-brown-light text-sm max-w-sm mb-10">
        The page you're looking for doesn't exist. It may have been moved or the link may be incorrect.
      </p>
      <Link
        to="/"
        className="text-xs tracking-widest uppercase bg-brown text-cream px-8 py-3 hover:bg-brown-mid transition-colors"
      >
        Back to Home
      </Link>
    </div>
  ),
})
