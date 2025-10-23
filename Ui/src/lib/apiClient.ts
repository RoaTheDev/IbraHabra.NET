import type {AxiosInstance, AxiosRequestConfig} from 'axios'
import axios from 'axios'

interface ApiConfig {
    baseURL: string
    timeout?: number
}

class ApiClient {
    private client: AxiosInstance
    private authToken: string | null = null

    constructor(config: ApiConfig) {
        this.client = axios.create({
            baseURL: config.baseURL,
            timeout: config.timeout ?? 15000,
            headers: {
                'Content-Type': 'application/json',
            },
        })

        this.setupInterceptors()
    }

    // --- async/await methods ---
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
            if (this.authToken && config.headers) {
                config.headers.Authorization = `Bearer ${this.authToken}`
            }
            return config
        })
    }
}

export const apiClient = new ApiClient({
    baseURL: `${import.meta.env.VITE_API_URL}/api/${import.meta.env.VITE_API_VERSION}`,
})
