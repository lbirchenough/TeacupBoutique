import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import { authApi } from '../lib/api'
import { useAuth } from '../lib/useAuth'

export const Route = createFileRoute('/verify-email')({
  validateSearch: (search: Record<string, unknown>) => ({
    token: typeof search.token === 'string' ? search.token : '',
    email: typeof search.email === 'string' ? search.email : '',
  }),
  component: VerifyEmailPage,
})

function VerifyEmailPage() {
  const { token, email } = Route.useSearch()
  const { setToken } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token || !email) {
      setError('Invalid verification link.')
      return
    }

    authApi.verifyEmail({ token, email })
      .then(async (data) => {
        setToken(data.accessToken)
        navigate({ to: '/' })
      })
      .catch((err) => {
        setError(err.message || 'Invalid or expired link.')
      })
  }, [])

  return (
    <div className="px-4 pt-16 pb-16">
      <div className="w-full max-w-md mx-auto text-center">
        <p className="text-xs tracking-[0.3em] uppercase text-gold font-semibold mb-3">Verifying</p>
        <h1 className="font-script text-6xl text-brown mb-6">Email Verification</h1>
        <div className="bg-cream border border-gold/20 shadow-lg px-8 py-10">
          {error ? (
            <>
              <p className="text-red-600 text-sm mb-4">{error}</p>
              <p className="text-brown-mid text-sm">Please try registering again or contact us if the problem persists.</p>
            </>
          ) : (
            <p className="text-brown-mid text-sm">Verifying your email address…</p>
          )}
        </div>
      </div>
    </div>
  )
}
