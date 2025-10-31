const isSecure =
  typeof window !== 'undefined' && window.location.protocol === 'https:'

export const cookieUtils = {
  set: (
    key: string,
    value: string,
    options?: { days?: number; path?: string },
  ) => {
    if (typeof document === 'undefined') return
    const { days = 7, path = '/' } = options || {}
    const expires = new Date(Date.now() + days * 24 * 60 * 60 * 1000)

    const securePart = isSecure ? 'Secure; ' : ''
    document.cookie = `${encodeURIComponent(key)}=${encodeURIComponent(value)}; expires=${expires.toUTCString()}; path=${path}; ${securePart}SameSite=Lax`
  },

  remove: (key: string, path: string = '/') => {
    if (typeof document === 'undefined') return
    const securePart = isSecure ? 'Secure; ' : ''
    document.cookie = `${encodeURIComponent(key)}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=${path}; ${securePart}SameSite=Lax`
  },

  get: (key: string): string | null => {
    if (typeof document === 'undefined') return null
    const match = document.cookie
      .split('; ')
      .find((row) => row.startsWith(`${encodeURIComponent(key)}=`))
    return match ? decodeURIComponent(match.split('=')[1]) : null
  },
}
