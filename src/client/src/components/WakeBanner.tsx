import { useEffect, useState } from 'react'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5054'

type WakeState = 'waking' | 'warm' | 'failed'

export function WakeBanner() {
  const [state, setState] = useState<WakeState>('waking')

  useEffect(() => {
    let cancelled = false
    const wake = async () => {
      try {
        const res = await fetch(`${API_BASE_URL}/api/wake`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
        })
        if (cancelled) return
        setState(res.ok ? 'warm' : 'failed')
      } catch {
        if (cancelled) return
        setState('failed')
      }
    }
    wake()
    return () => {
      cancelled = true
    }
  }, [])

  if (state === 'warm') return null

  const message =
    state === 'waking'
      ? 'Waking service containers — this can take up to 30 seconds on a cold start.'
      : 'Service containers did not respond. They may still be warming up; try again shortly.'

  return (
    <div className="bg-brown text-cream text-xs tracking-widest uppercase px-6 py-3 text-center">
      {message}
    </div>
  )
}
