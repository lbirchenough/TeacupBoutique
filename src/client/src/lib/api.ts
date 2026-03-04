import { authStore } from './authStore'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5054'

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
}

export interface AuthResponse {
  accessToken: string
}

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })

    if (!response.ok) {
      const message = await response.text(); // "Invalid credentials"
      throw new Error(message || 'An error occurred while logging in');
    }

    return await response.json()
  },

  register: async (data: RegisterRequest) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })
    return response.json()
  },

  logout: async () => {
    const token = authStore.getAccessToken()

    const response = await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      credentials: 'include',
      headers: token
        ? {
            Authorization: `Bearer ${token}`,
          }
        : undefined,
    })

    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'An error occurred while logging out')
    }

    authStore.clearAccessToken()
  },

  refresh: async (): Promise<AuthResponse> => {
    const response = await fetch(`${API_BASE_URL}/api/auth/refresh-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      
    })

    if (!response.ok) {
      const message = await response.text(); // "Some server side error about refresh token"
      throw new Error(message || 'An error attempting to refresh token');
    }

    return await response.json()
  },
}

