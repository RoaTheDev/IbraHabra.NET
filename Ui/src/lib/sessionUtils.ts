export const sessionUtils = {
  get: <T>(key: string): T | null => {
    const data = sessionStorage.getItem(key)
    if (!data) return null
    try {
      return JSON.parse(data) as T
    } catch {
      return null
    }
  },

  set: <T>(key: string, data: T): void => {
    sessionStorage.setItem(key, JSON.stringify(data))
  },

  remove: (key: string): void => {
    sessionStorage.removeItem(key)
  },

  clearAll: (): void => {
    sessionStorage.clear()
  },
}
