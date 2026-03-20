import { Link, useNavigate } from '@tanstack/react-router'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../lib/api'
import { useAuth } from '../lib/useAuth'
import { useCart } from '../lib/useCart'

export function Navbar() {
  const navigate = useNavigate()
  const { isLoggedIn, isAdmin, setToken } = useAuth()
  const { count } = useCart()
  const queryClient = useQueryClient()

  const logoutMutation = useMutation({
    mutationFn: authApi.logout,
    onSuccess: () => {
      setToken(null)
      queryClient.clear()
      navigate({ to: '/login' })
    },
    onError: (error) => {
      console.error('Logout error:', error)
    },
  })

  return (
    <nav className="sticky top-0 z-50 bg-cream border-b border-gold/20">
      <div className="max-w-7xl mx-auto px-6">
        <div className="flex items-center justify-between h-20">
          {/* Logo */}
          <Link to="/" className="flex flex-col leading-none group">
            <span className="font-script text-3xl text-brown group-hover:text-brown-mid transition-colors">Teacup Boutique</span>
            <span className="text-[10px] tracking-[0.2em] uppercase text-brown-light">High Tea Hire</span>
          </Link>

          {/* Nav links */}
          <div className="flex items-center gap-7">
            <Link
              to="/"
              className="text-sm text-brown-mid hover:text-brown transition-colors"
            >
              Home
            </Link>
            <Link
              to="/products"
              className="text-sm text-brown-mid hover:text-brown transition-colors"
            >
              Our Collection
            </Link>
            <Link
              to="/availability"
              className="text-sm font-semibold text-gold hover:text-gold/70 transition-colors tracking-wide"
            >
              Book Now
            </Link>
            {isAdmin && (
              <Link
                to="/bookings"
                className="text-sm text-brown-mid hover:text-brown transition-colors"
              >
                Bookings
              </Link>
            )}

            {/* Cart icon */}
            <Link to="/cart" className="relative text-brown-mid hover:text-brown transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 10.5V6a3.75 3.75 0 10-7.5 0v4.5m11.356-1.993l1.263 12c.07.665-.45 1.243-1.119 1.243H4.25a1.125 1.125 0 01-1.12-1.243l1.264-12A1.125 1.125 0 015.513 7.5h12.974c.576 0 1.059.435 1.119 1.007zM8.625 10.5a.375.375 0 11-.75 0 .375.375 0 01.75 0zm5.625 0a.375.375 0 11-.75 0 .375.375 0 01.75 0z" />
              </svg>
              {count > 0 && (
                <span className="absolute -top-2 -right-2 bg-gold text-white text-[10px] font-bold rounded-full w-4 h-4 flex items-center justify-center">
                  {count}
                </span>
              )}
            </Link>

            {/* Auth */}
            {!isLoggedIn && (
              <>
                <Link
                  to="/login"
                  className="text-sm text-brown-mid hover:text-brown transition-colors"
                >
                  Login
                </Link>
                <Link
                  to="/register"
                  className="text-xs tracking-widest uppercase border border-brown/30 text-brown px-4 py-2 hover:bg-cream-dark transition-colors"
                >
                  Register
                </Link>
              </>
            )}
            {isLoggedIn && (
              <button
                type="button"
                onClick={() => logoutMutation.mutate()}
                disabled={logoutMutation.isPending}
                className="text-sm text-brown-mid hover:text-brown transition-colors disabled:opacity-50"
              >
                {logoutMutation.isPending ? 'Logging out…' : 'Logout'}
              </button>
            )}
          </div>
        </div>
      </div>
    </nav>
  )
}
