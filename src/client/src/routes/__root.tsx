import { createRootRoute, Outlet } from '@tanstack/react-router'
import { Navbar } from '../components/Navbar'
import { Footer } from '../components/Footer'

export const Route = createRootRoute({
  component: () => (
    <div className="min-h-screen bg-cream flex flex-col">
      <Navbar />
      <main className="flex-1 bg-checker bg-cream">
        <Outlet />
      </main>
      <Footer />
    </div>
  ),
})
