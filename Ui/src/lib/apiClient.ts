import type { AxiosInstance, AxiosRequestConfig } from 'axios'
import axios from 'axios'
import {
  adminAuthStore,
  adminAuthStoreAction,
} from '@/stores/adminAuthStore.ts'
import { ApiResponse } from '@/types/ApiResponse.ts'
import { RefreshTokenResponse } from '@/features/admin/auth/adminAuthTypes.ts'

interface ApiConfig {
  baseURL: string
  timeout?: number
}

class ApiClient {
  private readonly client: AxiosInstance

  constructor(config: ApiConfig) {
    this.client = axios.create({
      baseURL: config.baseURL,
      timeout: config.timeout ?? 15000,
      withCredentials: true,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    this.setupInterceptors()
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, config)
    return response.data
  }

  async post<T, D = unknown>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    const response = await this.client.post<T>(url, data, config)
    return response.data
  }

  async put<T, D = unknown>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    const response = await this.client.put<T>(url, data, config)
    return response.data
  }

  async patch<T, D = unknown>(
    url: string,
    data?: D,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    const response = await this.client.patch<T>(url, data, config)
    return response.data
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.client.delete<T>(url, config)
    return response.data
  }

  private setupInterceptors(): void {
    this.client.interceptors.request.use((config) => {
      const token = adminAuthStore.state.token
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    })

    this.client.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config

        const isRefreshEndpoint = originalRequest.url?.includes(
          '/refresh',
        )


        if (
          error.response?.status === 401 &&
          !originalRequest._retry &&
          !isRefreshEndpoint
        ) {
          originalRequest._retry = true

          try {
            const jwtToken = adminAuthStore.state.token
            if (!jwtToken) throw new Error('Cannot refresh')

            const response = await this.client.post<
              ApiResponse<RefreshTokenResponse>
            >(
              '/admin/auth/refresh',
              {})

            const { token, expiredAt } = response.data.data

            adminAuthStoreAction.setAuth(
              adminAuthStore.state.user!,
              token,
              expiredAt,
            )

            originalRequest.headers['Authorization'] = `Bearer ${token}`
            return this.client(originalRequest)
          } catch (refreshError) {
            console.error('Token refresh failed:', refreshError)
            adminAuthStoreAction.reset()
            window.location.href = '/auth/login'
            return Promise.reject(refreshError)
          }
        }

        if (isRefreshEndpoint && error.response?.status === 401) {
          console.error('Refresh token invalid or expired')
          adminAuthStoreAction.reset()
          window.location.href = '/auth/login'
        }

        return Promise.reject(error)
      },
    )
  }
}

export const apiClient = new ApiClient({
  baseURL: `${import.meta.env.VITE_API_URL}/${import.meta.env.VITE_API_VERSION}/api`,
})
