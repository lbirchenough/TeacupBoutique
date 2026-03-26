import { createFileRoute, Link } from '@tanstack/react-router'
import { useState } from 'react'
import { useMutation } from '@tanstack/react-query'
import { authApi } from '../lib/api'

export const Route = createFileRoute('/reset-password')({
  validateSearch: (search: Record<string, unknown>) => ({
    token: typeof search.token === 'string' ? search.token : '',
    email: typeof search.email === 'string' ? search.email : '',
  }),
  component: ResetPasswordPage,
})

function ResetPasswordPage() {
  const { token, email } = Route.useSearch()
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [matchError, setMatchError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const mutation = useMutation({
    mutationFn: authApi.resetPassword,
    onSuccess: () => setSuccess(true),
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (newPassword !== confirmPassword) {
      setMatchError('Passwords do not match')
      return
    }
    setMatchError(null)
    mutation.mutate({ email, token, newPassword })
  }

  if (success) {
    return (
      <div className="px-4 pt-16 pb-16">
        <div className="w-full max-w-md mx-auto text-center">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">All Done</p>
          <h1 className="font-script text-6xl text-brown mb-6">Password Updated</h1>
          <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
            <p className="text-brown-mid text-sm leading-relaxed mb-6">
              Your password has been changed successfully.
            </p>
            <Link
              to="/login"
              className="inline-block bg-brown hover:bg-brown-mid text-cream text-xs tracking-[0.2em] uppercase font-semibold px-8 py-3.5 transition-colors"
            >
              Sign In
            </Link>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="px-4 pt-16 pb-16">
      <div className="w-full max-w-md mx-auto">
        <div className="text-center mb-10">
          <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Account Recovery</p>
          <h1 className="font-script text-6xl text-brown">New Password</h1>
          <div className="flex items-center justify-center gap-3 mt-5">
            <div className="h-px w-12 bg-gold/40" />
            <div className="w-1.5 h-1.5 rounded-full bg-gold/60" />
            <div className="h-px w-12 bg-gold/40" />
          </div>
        </div>

        <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
          {(!token || !email) ? (
            <p className="text-sm text-red-600 text-center">Invalid reset link. Please request a new one.</p>
          ) : (
            <form onSubmit={handleSubmit}>
              <div className="mb-5">
                <label htmlFor="newPassword" className="block text-xs tracking-[0.15em] uppercase text-brown-mid font-semibold mb-2">
                  New Password
                </label>
                <input
                  id="newPassword"
                  type="password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  required
                  minLength={6}
                  className="w-full border border-brown/20 bg-white px-4 py-3 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20 transition-colors"
                  placeholder="At least 6 characters"
                />
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
                  required
                  className="w-full border border-brown/20 bg-white px-4 py-3 text-sm text-brown focus:outline-none focus:border-gold focus:ring-1 focus:ring-gold/20 transition-colors"
                  placeholder="Re-enter your password"
                />
                {matchError && <p className="text-red-500 text-xs mt-1.5">{matchError}</p>}
              </div>

              {mutation.isError && (
                <p className="text-sm text-red-600 mb-4">{mutation.error.message}</p>
              )}

              <button
                type="submit"
                disabled={mutation.isPending}
                className="w-full bg-brown hover:bg-brown-mid text-cream text-xs tracking-[0.2em] uppercase font-semibold py-3.5 transition-colors disabled:opacity-50"
              >
                {mutation.isPending ? 'Updating…' : 'Set New Password'}
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}
