import { createFileRoute, useNavigate, Link } from '@tanstack/react-router'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { authApi } from '../lib/api'
import { useAuth } from '../lib/useAuth'
import { ordersApi } from '../lib/ordersApi'

export const Route = createFileRoute('/register')({
  validateSearch: (search: Record<string, unknown>) => ({
    email: typeof search.email === 'string' ? search.email : undefined,
  }),
  component: RegisterPage,
})

function RegisterPage() {
  const navigate = useNavigate()
  const { setToken } = useAuth()
  const { email: prefillEmail } = Route.useSearch()
  const [email, setEmail] = useState(prefillEmail ?? '')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [errors, setErrors] = useState<{
    email?: string
    password?: string
    confirmPassword?: string
  }>({})
  const [serverError, setServerError] = useState<string | null>(null)
  const [verificationSent, setVerificationSent] = useState(false)
  const [registeredEmail, setRegisteredEmail] = useState('')

  const registerMutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: async (data) => {
      if (data.requiresVerification) {
        setRegisteredEmail(email)
        setVerificationSent(true)
        return
      }
      if (data.accessToken) {
        setToken(data.accessToken)
        await ordersApi.claimOrders()
        navigate({ to: '/my-orders' })
      }
    },
    onError: (error) => {
      setServerError(error.message)
    },
  })

  const validate = () => {
    const newErrors: {
      email?: string
      password?: string
      confirmPassword?: string
    } = {}

    if (!email) {
      newErrors.email = 'Email is required'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = 'Please enter a valid email address'
    }

    if (!password) {
      newErrors.password = 'Password is required'
    } else if (password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters'
    }

    if (!confirmPassword) {
      newErrors.confirmPassword = 'Please confirm your password'
    } else if (password !== confirmPassword) {
      newErrors.confirmPassword = 'Passwords do not match'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (validate()) {
      registerMutation.mutate({ email, password })
    }
  }

  if (verificationSent) {
    return (
      <div className="px-4 pt-16 pb-16">
        <div className="w-full max-w-md mx-auto text-center">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Almost There</p>
          <h1 className="font-script text-6xl text-brown mb-6">Check Your Inbox</h1>
          <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
            <p className="text-brown-mid text-sm leading-relaxed mb-4">
              We've sent a verification link to <strong className="text-brown">{registeredEmail}</strong>.
            </p>
            <p className="text-brown-mid text-sm leading-relaxed">
              Click the link in that email to activate your account and log in.
            </p>
            <p className="text-xs text-brown-light mt-6">Didn't receive it? Check your spam folder or try registering again.</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="px-4 pt-16 pb-16">
      <div className="w-full max-w-md mx-auto">
        {/* Header */}
        <div className="text-center mb-10">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Join Us</p>
          <h1 className="font-script text-6xl text-brown">Create Account</h1>
          <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
            Streamline your orders, track your hires and manage everything from one place.
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

            <div className="mb-5">
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
                placeholder="At least 6 characters"
              />
              {errors.password && (
                <p className="text-red-500 text-xs mt-1.5">{errors.password}</p>
              )}
            </div>

            <div className="mb-8">
              <label htmlFor="confirmPassword" className="block text-xs tracking-[0.15em] uppercase text-brown-mid font-semibold mb-2">
                Confirm Password
              </label>
              <input
                id="confirmPassword"
                type="password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className={`w-full border bg-white px-4 py-3 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20 transition-colors ${
                  errors.confirmPassword ? 'border-red-400' : 'border-brown/20'
                }`}
                placeholder="Re-enter your password"
              />
              {errors.confirmPassword && (
                <p className="text-red-500 text-xs mt-1.5">{errors.confirmPassword}</p>
              )}
            </div>

            {serverError && (
              <p className="text-sm text-center text-brown-mid bg-gold-pale border border-gold/30 px-4 py-2.5 mb-6">
                {serverError}
              </p>
            )}

            <button
              type="submit"
              disabled={registerMutation.isPending}
              className="w-full bg-brown hover:bg-brown-mid text-cream text-xs tracking-[0.2em] uppercase font-semibold py-3.5 transition-colors disabled:opacity-50"
            >
              {registerMutation.isPending ? 'Creating Account...' : 'Create Account'}
            </button>
          </form>

          <div className="mt-6 pt-6 border-t border-gold/20 text-center">
            <p className="text-sm text-brown-mid">
              Already have an account?{' '}
              <Link to="/login" className="text-brown font-semibold hover:text-gold transition-colors underline underline-offset-2">
                Sign in
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
