import { createFileRoute, Link } from '@tanstack/react-router'
import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../lib/useAuth'
import { authApi } from '../lib/api'

export const Route = createFileRoute('/profile')({
  component: ProfilePage,
})

function ProfilePage() {
  const { updateToken } = useAuth()
  const queryClient = useQueryClient()

  const [fullName, setFullName] = useState('')
  const [profileEmail, setProfileEmail] = useState('')
  const [phone, setPhone] = useState('')

  const { data: profile, isPending } = useQuery({
    queryKey: ['profile'],
    queryFn: () => authApi.getProfile(),
  })

  useEffect(() => {
    if (profile) {
      setFullName(profile.fullName)
      setProfileEmail(profile.email)
      setPhone(profile.phoneNumber)
    }
  }, [profile])

  const updateMutation = useMutation({
    mutationFn: () => authApi.updateProfile({ fullName, email: profileEmail, phoneNumber: phone }),
    onSuccess: (data) => {
      if (data.accessToken) {
        updateToken(data.accessToken)
      }
      queryClient.invalidateQueries({ queryKey: ['profile'] })
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    updateMutation.mutate()
  }

  return (
    <div className="min-h-screen bg-cream">
      <div className="max-w-2xl mx-auto px-6 py-16">

        <div className="mb-10">
          <p className="text-xs tracking-[0.2em] uppercase text-brown-light mb-2">Account</p>
          <h1 className="font-serif text-4xl text-brown">My Profile</h1>
          <div className="mt-3 h-px bg-gold/30" />
        </div>

        {isPending ? (
          <p className="font-serif text-brown-light">Loading…</p>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-xs tracking-[0.15em] uppercase text-brown-light mb-2">
                Full Name
              </label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="Your full name"
                className="w-full bg-white border border-gold/30 px-4 py-3 text-sm text-brown placeholder:text-brown-light/50 focus:outline-none focus:border-gold transition-colors"
              />
            </div>

            <div>
              <label className="block text-xs tracking-[0.15em] uppercase text-brown-light mb-2">
                Email Address
              </label>
              <input
                type="email"
                value={profileEmail}
                onChange={(e) => setProfileEmail(e.target.value)}
                placeholder="your@email.com"
                className="w-full bg-white border border-gold/30 px-4 py-3 text-sm text-brown placeholder:text-brown-light/50 focus:outline-none focus:border-gold transition-colors"
              />
              <p className="text-xs text-brown-light mt-1">Changing your email will require re-authentication and send a security notice to both addresses.</p>
            </div>

            <div>
              <label className="block text-xs tracking-[0.15em] uppercase text-brown-light mb-2">
                Phone Number
              </label>
              <input
                type="tel"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                placeholder="+61 400 000 000"
                className="w-full bg-white border border-gold/30 px-4 py-3 text-sm text-brown placeholder:text-brown-light/50 focus:outline-none focus:border-gold transition-colors"
              />
            </div>

            {updateMutation.isError && (
              <p className="text-sm text-red-600">{updateMutation.error.message}</p>
            )}

            <div className="flex items-center gap-4 pt-2">
              <button
                type="submit"
                disabled={updateMutation.isPending}
                className="text-xs tracking-widest uppercase bg-brown text-cream px-8 py-3 hover:bg-brown-mid transition-colors disabled:opacity-50"
              >
                {updateMutation.isPending ? 'Saving…' : 'Save Changes'}
              </button>
              {updateMutation.isSuccess && (
                <span className="text-xs text-gold tracking-wide">Changes saved</span>
              )}
            </div>
          </form>
        )}

        <div className="mt-12 pt-8 border-t border-gold/20">
          <Link
            to="/my-orders"
            className="inline-flex items-center gap-3 text-sm text-brown-mid hover:text-brown transition-colors group"
          >
            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z" />
            </svg>
            View My Orders
            <span className="group-hover:translate-x-0.5 transition-transform">→</span>
          </Link>
        </div>

      </div>
    </div>
  )
}
