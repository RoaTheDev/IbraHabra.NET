import type { AxiosInstance, AxiosRequestConfig } from 'axios'
import axios from 'axios'
import { ApiResponse } from '@/types/ApiResponse.ts'
import { RefreshTokenResponse } from '@/features/admin/auth/adminAuthTypes.ts'

interface ApiConfig {
  baseURL: string
  timeout?: number
}

class ApiClient {
  private readonly client: AxiosInstance
  private isRefreshing = false
  private refreshPromise: Promise<void> | null = null
  private failedQueue: Array<{
    resolve: (value?: any) => void
    reject: (reason?: any) => void
  }> = []

  constructor(config: ApiConfig) {
    this.client = axios.create({
      baseURL: config.baseURL,
      timeout: config.timeout ?? 15000,
      withCredentials: true,
      headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest',
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
    this.client.interceptors.request.use(
      async (config) => {
        if (
          config.url?.includes('/login') ||
          config.url?.includes('/verify-2fa') ||
          config.url?.includes('/2fa/recovery') ||
          config.url?.includes('/refresh')
        ) {
          return config
        }

        return config
      },
      (error) => Promise.reject(error),
    )

    this.client.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config

        if (
          originalRequest.url?.includes('/refresh') ||
          originalRequest._retry
        ) {
          if (error.response?.status === 401) {
            window.location.href = '/auth/login'
          }
          return Promise.reject(error)
        }

        if (error.response?.status !== 401) {
          return Promise.reject(error)
        }

        if (this.isRefreshing) {
          return new Promise((resolve, reject) => {
            this.failedQueue.push({ resolve, reject })
            this.processQueuedRequest({ resolve, reject }, 10000).catch(reject)
          })
            .then(() => {
              return this.client(originalRequest)
            })
            .catch((err) => {
              console.error('Queued request failed:', err)
              return Promise.reject(err)
            })
        }

        originalRequest._retry = true
        this.isRefreshing = true

        this.refreshPromise = (async () => {
          try {
            await this.client.post<ApiResponse<RefreshTokenResponse>>(
              '/admin/auth/refresh',
              {},
            )

            this.processQueue(null)
          } catch (refreshError) {
            this.processQueue(refreshError)
            window.location.href = '/auth/login'
            throw refreshError
          } finally {
            this.isRefreshing = false
            this.refreshPromise = null
          }
        })()

        try {
          await this.refreshPromise
          return this.client(originalRequest)
        } catch (refreshError) {
          return Promise.reject(refreshError)
        }
      },
    )
  }

  private processQueue(error: any, token: string | null = null) {
    this.failedQueue.forEach((prom) => {
      if (error) {
        prom.reject(error)
      } else {
        prom.resolve(token)
      }
    })
    this.failedQueue = []
  }

  private async processQueuedRequest(
    prom: { resolve: (value?: any) => void; reject: (reason?: any) => void },
    timeout = 5000,
  ) {
    const timeoutPromise = new Promise((_, reject) =>
      setTimeout(() => reject(new Error('Token refresh timeout')), timeout),
    )

    try {
      if (this.refreshPromise) {
        await Promise.race([this.refreshPromise, timeoutPromise])
        prom.resolve()
      } else {
        prom.reject(new Error('No refresh in progress'))
      }
    } catch (error) {
      prom.reject(error)
    }
  }
}

export const apiClient = new ApiClient({
  baseURL: `${import.meta.env.VITE_API_URL}/${import.meta.env.VITE_API_VERSION}/api`,
})
export const adminAuthEndpoint = '/admin/auth'
export const adminRoleEndpoint = '/admin/roles'
