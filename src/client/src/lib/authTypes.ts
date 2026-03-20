export interface AuthContextValue {
  isLoggedIn: boolean
  isAdmin: boolean
  accessToken: string | null
  setToken: (token: string | null) => void
}


