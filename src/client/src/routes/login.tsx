import { createFileRoute, useNavigate, Link } from '@tanstack/react-router'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { authApi } from '../lib/api'
import { useAuth } from '../lib/useAuth'
import { ordersApi } from '../lib/ordersApi'

export const Route = createFileRoute('/login')({
  component: LoginPage,
})

function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({})
  const [serverError, setServerError] = useState<string | null>(null)
  const { setToken } = useAuth()

  const loginMutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: async (data) => {
      console.log('Login response:', data)
      setToken(data.accessToken)
      await ordersApi.claimOrders()
      navigate({ to: '/' })
    },
    onError: (error) => {
      setServerError(error.message)
    },
  })

  const validate = () => {
    const newErrors: { email?: string; password?: string } = {}

    if (!email) {
      newErrors.email = 'Email is required'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'Please enter a valid email address'
    }

    if (!password) {
      newErrors.password = 'Password is required'
    } else if (password.length < 8) {
      newErrors.password = 'Password must be at least 8 characters'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) {
      loginMutation.mutate({ email, password })
    }
  }

  return (
    <div className="px-4 pt-16 pb-16">
      <div className="w-full max-w-md mx-auto">
        {/* Header */}
        <div className="text-center mb-10">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Welcome Back</p>
          <h1 className="font-script text-6xl text-brown">Sign In</h1>
          <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
            Your orders, bookings and details — all in one place.
          </p>
          <div className="flex items-center justify-center gap-3 mt-5">
            <div className="h-px w-12 bg-gold/40" />
            <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
            <div className="h-px w-12 bg-gold/40" />
          </div>
        </div>

        {/* Card */}
        <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
          <form onSubmit={handleSubmit}>
            {serverError && (
              <p className="text-sm text-center text-brown-mid bg-gold-pale border border-gold/30 px-4 py-2.5 mb-6">
                {serverError}
              </p>
            )}

            <div className="mb-5">
              <label htmlFor="email" className="block text-xs tracking-[0.15em] uppercase text-brown-mid font-semibold mb-2">
                Email Address
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={`w-full border bg-white px-4 py-3 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20 transition-colors ${
                  errors.email ? 'border-red-400' : 'border-brown/20'
                }`}
                placeholder="your@email.com"
              />
              {errors.email && (
                <p className="text-red-500 text-xs mt-1.5">{errors.email}</p>
              )}
            </div>

            <div className="mb-8">
              <label htmlFor="password" className="block text-xs tracking-[0.15em] uppercase text-brown-mid font-semibold mb-2">
                Password
              </label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className={`w-full border bg-white px-4 py-3 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20 transition-colors ${
                  errors.password ? 'border-red-400' : 'border-brown/20'
                }`}
                placeholder="Enter your password"
              />
              {errors.password && (
                <p className="text-red-500 text-xs mt-1.5">{errors.password}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={loginMutation.isPending}
              className="w-full bg-brown hover:bg-brown-mid text-cream text-xs tracking-[0.2em] uppercase font-semibold py-3.5 transition-colors disabled:opacity-50"
            >
              {loginMutation.isPending ? 'Signing in...' : 'Sign In'}
            </button>
          </form>

          <div className="mt-6 pt-6 border-t border-gold/20 text-center">
            <p className="text-sm text-brown-mid">
              Don't have an account?{' '}
              <Link to="/register" className="text-brown font-semibold hover:text-gold transition-colors underline underline-offset-2">
                Create one
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

