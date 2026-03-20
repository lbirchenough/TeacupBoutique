import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { authApi } from './api'
import { authStore } from './authStore'
import type { AuthContextValue } from './authTypes'
import { AuthContext } from './AuthContextBase'

function parseIsAdmin(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    return role === 'Admin'
  } catch {
    return false
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessToken] = useState<string | null>(authStore.getAccessToken())
  const [initialised, setInitialised] = useState(false)

  const setToken = (token: string | null) => {
    setAccessToken(token)
    if (token) {
      authStore.setAccessToken(token)
    } else {
      authStore.clearAccessToken()
    }
  }

  useEffect(() => {
    let cancelled = false

    const runRefresh = async () => {
      try {
        const data = await authApi.refresh()
        if (!cancelled) {
          setToken(data.accessToken)
        }
      } catch {
        if (!cancelled) {
          setToken(null)
        }
      } finally {
        if (!cancelled) {
          setInitialised(true)
        }
      }
    }

    runRefresh()

    return () => {
      cancelled = true
    }
  }, [])

  const value: AuthContextValue = {
    isLoggedIn: !!accessToken,
    isAdmin: accessToken ? parseIsAdmin(accessToken) : false,
    accessToken,
    setToken,
  }

  if (!initialised) {
    // Optionally render a loading state while we check for a refresh token
    return null
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// No hooks or extra exports here to keep fast-refresh happy
