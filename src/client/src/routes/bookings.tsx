import { createFileRoute, Outlet, redirect } from '@tanstack/react-router'
import { authStore } from '../lib/authStore'

function getRole(token: string): string | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null
  } catch {
    return null
  }
}

export const Route = createFileRoute('/bookings')({
    beforeLoad: () => {
        const token = authStore.getAccessToken()
        if (!token) throw redirect({ to: '/login' })
        if (getRole(token) !== 'Admin') throw redirect({ to: '/' })
    },
    component: () => <Outlet />,
})
