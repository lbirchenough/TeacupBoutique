import { Link, useNavigate } from '@tanstack/react-router'
import { useMutation } from '@tanstack/react-query'
import { authApi } from '../lib/api'
import { useAuth } from '../lib/useAuth'

export function Navbar() {
  const navigate = useNavigate()
  const { isLoggedIn, setToken } = useAuth()

  const logoutMutation = useMutation({
    mutationFn: authApi.logout,
    onSuccess: () => {
      // Clear auth state and send user to login page
      setToken(null)
      navigate({ to: '/login' })
    },
    onError: (error) => {
      // For now just log; you can surface this in UI later if you want
      console.error('Logout error:', error)
    },
  })

  const handleLogout = () => {
    logoutMutation.mutate()
  }

  return (
    <nav className="bg-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex items-center">
            <Link to="/" className="text-xl font-bold text-gray-800">
              High Tea Rentals
            </Link>
          </div>
          <div className="flex items-center space-x-4">
            <Link
              to="/"
              className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
            >
              Home
            </Link>
            {!isLoggedIn && (
              <>
                <Link
                  to="/login"
                  className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium"
                >
                  Login
                </Link>
                <Link
                  to="/register"
                  className="bg-blue-600 text-white hover:bg-blue-700 px-4 py-2 rounded-md text-sm font-medium"
                >
                  Register
                </Link>
              </>
            )}
            {isLoggedIn && (
              <button
                type="button"
                onClick={handleLogout}
                disabled={logoutMutation.isPending}
                className="text-gray-600 hover:text-gray-900 px-3 py-2 rounded-md text-sm font-medium disabled:opacity-50"
              >
                {logoutMutation.isPending ? 'Logging out...' : 'Logout'}
              </button>
            )}
          </div>
        </div>
      </div>
    </nav>
  )
}

