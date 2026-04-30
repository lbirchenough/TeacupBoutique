export interface AuthContextValue {
  authStatus: 'checking' | 'anonymous' | 'authenticated'
  isLoggedIn: boolean
  isAdmin: boolean
  email: string | null
  accessToken: string | null
  setToken: (token: string | null) => void
  updateToken: (token: string) => void
}

export interface UpdateProfileRequest {
  fullName: string
  email: string
  phoneNumber: string
}

export interface ProfileResponse {
  fullName: string
  email: string
  phoneNumber: string
}


