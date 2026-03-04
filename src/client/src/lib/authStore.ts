let accessToken: string | null = null

export const authStore = {
  setAccessToken(token: string | null) {
    accessToken = token
  },
  getAccessToken() {
    return accessToken
  },
  clearAccessToken() {
    accessToken = null
  },
}


