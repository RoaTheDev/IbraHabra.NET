import axios from 'axios'
import { adminAuthStore } from '@/stores/adminAuthStore.ts'

export const api = axios.create({
  baseURL: 'https://your-api-url.com',
})

api.interceptors.request.use((config) => {
  const token = adminAuthStore.state.token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
