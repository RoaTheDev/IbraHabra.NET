export const localStorageUtils = {
  get: <T>(key: string): T | null => {
    const data = localStorage.getItem(key)
    if (!data) return null
    try {
      return JSON.parse(data) as T
    } catch {
      return null
    }
  },

  set: <T>(key: string, data: T): void => {
    localStorage.setItem(key, JSON.stringify(data))
  },

  remove: (key: string): void => {
    localStorage.removeItem(key)
  },

  clearAll: (): void => {
    localStorage.clear()
  },
}
