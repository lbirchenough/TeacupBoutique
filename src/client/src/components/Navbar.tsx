import { Link, useNavigate } from '@tanstack/react-router'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import { authApi } from '../lib/api'
import { useAuth } from '../lib/useAuth'
import { useCart } from '../lib/useCart'

export function Navbar() {
  const navigate = useNavigate()
  const { isLoggedIn, isAdmin, email, setToken } = useAuth()
  const { count } = useCart()
  const queryClient = useQueryClient()
  const [profileOpen, setProfileOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setProfileOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const logoutMutation = useMutation({
    mutationFn: authApi.logout,
    onSuccess: () => {
      setToken(null)
      queryClient.clear()
      setProfileOpen(false)
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
            {isAdmin && (
              <Link
                to="/maintenance"
                className="text-sm text-brown-mid hover:text-brown transition-colors"
              >
                Maintenance
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
                  search={{ email: undefined }}
                  className="text-xs tracking-widest uppercase border border-brown/30 text-brown px-4 py-2 hover:bg-cream-dark transition-colors"
                >
                  Register
                </Link>
              </>
            )}

            {/* Profile dropdown */}
            {isLoggedIn && (
              <div className="relative" ref={dropdownRef}>
                <button
                  type="button"
                  onClick={() => setProfileOpen(o => !o)}
                  className="flex items-center gap-2 text-brown-mid hover:text-brown transition-colors"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M17.982 18.725A7.488 7.488 0 0012 15.75a7.488 7.488 0 00-5.982 2.975m11.963 0a9 9 0 10-11.963 0m11.963 0A8.966 8.966 0 0112 21a8.966 8.966 0 01-5.982-2.275M15 9.75a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  <svg xmlns="http://www.w3.org/2000/svg" className={`h-3 w-3 transition-transform ${profileOpen ? 'rotate-180' : ''}`} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
                  </svg>
                </button>

                {profileOpen && (
                  <div className="absolute right-0 mt-3 w-52 bg-cream border border-gold/20 shadow-lg">
                    {email && (
                      <div className="px-4 py-3 border-b border-gold/20">
                        <p className="text-xs text-brown-light truncate">{email}</p>
                      </div>
                    )}
                    <Link
                      to="/profile"
                      onClick={() => setProfileOpen(false)}
                      className="flex items-center gap-2 px-4 py-3 text-sm text-brown-mid hover:text-brown hover:bg-cream-dark transition-colors"
                    >
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                        <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
                      </svg>
                      My Profile
                    </Link>
                    <Link
                      to="/my-orders"
                      onClick={() => setProfileOpen(false)}
                      className="flex items-center gap-2 px-4 py-3 text-sm text-brown-mid hover:text-brown hover:bg-cream-dark transition-colors"
                    >
                      <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                        <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z" />
                      </svg>
                      My Orders
                    </Link>
                    <div className="border-t border-gold/20">
                      <button
                        type="button"
                        onClick={() => logoutMutation.mutate()}
                        disabled={logoutMutation.isPending}
                        className="flex items-center gap-2 w-full px-4 py-3 text-sm text-brown-mid hover:text-brown hover:bg-cream-dark transition-colors disabled:opacity-50"
                      >
                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                          <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15m3 0l3-3m0 0l-3-3m3 3H9" />
                        </svg>
                        {logoutMutation.isPending ? 'Logging out…' : 'Logout'}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  )
}
