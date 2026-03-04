export interface AuthContextValue {
  isLoggedIn: boolean
  accessToken: string | null
  setToken: (token: string | null) => void
}


