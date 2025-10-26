import { FC } from 'react'
import { ApiError } from '@/types/ApiResponse.ts'

type ApiErrorsMessageType = {
  apiErrors: ApiError[] | undefined
}

export const ApiErrorsMessage: FC<ApiErrorsMessageType> = ({ apiErrors }) => {
  return (
    <>
      {apiErrors && apiErrors.length > 0 && (
        <div className="rounded-md bg-red-50 p-4 border border-red-200">
          <div className="flex">
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">
                {apiErrors.length === 1 ? 'Error' : 'Errors'}
              </h3>
              <div className="mt-2 text-sm text-red-700">
                <ul className="list-disc space-y-1 pl-5">
                  {apiErrors.map((err, index) => (
                    <li key={index}>
                      {err.field ? `${err.field}: ` : ''}
                      {err.message}
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  )
}
