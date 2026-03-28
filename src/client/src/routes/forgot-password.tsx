import { createFileRoute, Link } from '@tanstack/react-router'
import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { Turnstile } from '@marsidev/react-turnstile'
import { authApi } from '../lib/api'

export const Route = createFileRoute('/forgot-password')({
  component: ForgotPasswordPage,
})

function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [submitted, setSubmitted] = useState(false)
  const [turnstileToken, setTurnstileToken] = useState<string | null>(null)

  const mutation = useMutation({
    mutationFn: authApi.forgotPassword,
    onSuccess: () => setSubmitted(true),
    onError: () => setSubmitted(true), // always show confirmation — no enumeration
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (email && turnstileToken) mutation.mutate({ email, turnstileToken })
  }

  if (submitted) {
    return (
      <div className="px-4 pt-16 pb-16">
        <div className="w-full max-w-md mx-auto text-center">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Check Your Inbox</p>
          <h1 className="font-script text-6xl text-brown mb-6">Email Sent</h1>
          <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
            <p className="text-brown-mid text-sm leading-relaxed mb-4">
              If <strong className="text-brown">{email}</strong> is registered, a password reset link is on its way.
            </p>
            <p className="text-brown-mid text-sm leading-relaxed">
              Click the link in that email to choose a new password.
            </p>
            <p className="text-xs text-brown-light mt-6">Didn't receive it? Check your spam folder.</p>
          </div>
          <Link to="/login" className="inline-block mt-6 text-sm text-brown-mid hover:text-brown transition-colors underline underline-offset-2">
            Back to sign in
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="px-4 pt-16 pb-16">
      <div className="w-full max-w-md mx-auto">
        <div className="text-center mb-10">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Account Recovery</p>
          <h1 className="font-script text-6xl text-brown">Forgot Password</h1>
          <p className="text-brown-mid mt-4 text-sm max-w-xs mx-auto leading-relaxed">
            Enter your email address and we'll send you a link to reset your password.
          </p>
          <div className="flex items-center justify-center gap-3 mt-5">
            <div className="h-px w-12 bg-gold/40" />
            <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
            <div className="h-px w-12 bg-gold/40" />
          </div>
        </div>

        <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
          <form onSubmit={handleSubmit}>
            <div className="mb-8">
              <label htmlFor="email" className="block text-xs tracking-[0.15em] uppercase text-brown-mid font-semibold mb-2">
                Email Address
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="w-full border border-brown/20 bg-white px-4 py-3 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20 transition-colors"
                placeholder="your@email.com"
              />
            </div>

            <div className="mb-6">
              <Turnstile
                siteKey={import.meta.env.VITE_TURNSTILE_SITE_KEY_MANAGED}
                onSuccess={(token) => setTurnstileToken(token)}
                onExpire={() => setTurnstileToken(null)}
              />
            </div>

            <button
              type="submit"
              disabled={mutation.isPending || !turnstileToken}
              className="w-full bg-brown hover:bg-brown-mid text-cream text-xs tracking-[0.2em] uppercase font-semibold py-3.5 transition-colors disabled:opacity-50"
            >
              {mutation.isPending ? 'Sending…' : 'Send Reset Link'}
            </button>
          </form>

          <div className="mt-6 pt-6 border-t border-gold/20 text-center">
            <Link to="/login" className="text-sm text-brown-mid hover:text-brown transition-colors underline underline-offset-2">
              Back to sign in
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
