export interface AuthContextValue {
  isLoggedIn: boolean
  isAdmin: boolean
  email: string | null
  accessToken: string | null
  setToken: (token: string | null) => void
}


