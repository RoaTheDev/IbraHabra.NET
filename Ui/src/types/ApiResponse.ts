export type ApiResponse<T> = {
  data: T
  error: ApiError[]
  meta: ResponseMeta
}
export type ResponseMeta = {
  requestId: string
  timeStamp: Date
  version?: string
}

export type ApiError = {
  code: string
  message: string
  type: number
  field?: string
}
