import { createFileRoute, Outlet, redirect } from '@tanstack/react-router'
import { authStore } from '../lib/authStore'

export const Route = createFileRoute('/bookings')({
    beforeLoad: () => {
        if (!authStore.getAccessToken()) {
            throw redirect({ to: '/login' })
        }
    },
    component: () => <Outlet />,
})
