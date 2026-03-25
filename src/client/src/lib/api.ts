import { authStore } from './authStore'
import type { ProfileResponse, UpdateProfileRequest } from './authTypes'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5054'

function authHeaders(): HeadersInit {
  const token = authStore.getAccessToken()
  return token ? { Authorization: `Bearer ${token}` } : {}
}

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

export interface RegisterResponse {
  requiresVerification: boolean
  accessToken?: string
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

  register: async (data: RegisterRequest): Promise<RegisterResponse> => {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify(data),
    })

    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'An error occurred while registering')
    }

    return await response.json()
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

  getProfile: async (): Promise<ProfileResponse> => {
    const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
      credentials: 'include',
      headers: { ...authHeaders() },
    })
    if (!response.ok) throw new Error('Failed to load profile')
    return response.json()
  },

  updateProfile: async (data: UpdateProfileRequest): Promise<{ accessToken?: string }> => {
    const response = await fetch(`${API_BASE_URL}/api/auth/profile`, {
      method: 'PUT',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json', ...authHeaders() },
      body: JSON.stringify(data),
    })
    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'Failed to update profile')
    }
    if (response.status === 204) return {}
    return response.json()
  },

  verifyEmail: async (params: { email: string; token: string }): Promise<AuthResponse> => {
    const url = new URL(`${API_BASE_URL}/api/auth/verify-email`)
    url.searchParams.set('email', params.email)
    url.searchParams.set('token', params.token)
    const response = await fetch(url.toString(), { credentials: 'include' })
    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'Invalid or expired link')
    }
    return response.json()
  },

  verifyEmailChange: async (params: { newEmail: string; token: string }): Promise<AuthResponse> => {
    const url = new URL(`${API_BASE_URL}/api/auth/verify-email-change`)
    url.searchParams.set('newEmail', params.newEmail)
    url.searchParams.set('token', params.token)
    const response = await fetch(url.toString(), { credentials: 'include' })
    if (!response.ok) {
      const message = await response.text()
      throw new Error(message || 'Invalid or expired link')
    }
    return response.json()
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

