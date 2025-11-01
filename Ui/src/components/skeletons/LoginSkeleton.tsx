import { Shield } from 'lucide-react'
import { Skeleton } from '../ui/skeleton.tsx'

export function LoginSkeleton() {
  return (
    <div className="w-full max-w-md relative z-10">
      {/* Header Skeleton */}
      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-lg bg-primary/20 border-glow-info mb-4 animate-pulse">
          <Shield className="w-8 h-8 text-primary/50" />
        </div>
        <Skeleton className="h-8 w-64 mx-auto mb-2" />
        <Skeleton className="h-4 w-48 mx-auto" />
      </div>

      {/* Login Form Card Skeleton */}
      <div className="bg-card border border-border rounded-lg p-8 shadow-2xl backdrop-blur-sm border-glow-cyan animate-pulse">
        <Skeleton className="h-6 w-20 mb-2" />
        <Skeleton className="h-4 w-40 mb-6" />

        {/* API Error Placeholder (hidden but reserved) */}
        <div className="h-6 mb-4" />

        {/* Email Field Skeleton */}
        <div className="mb-4">
          <Skeleton className="h-4 w-32 mb-2" />
          <Skeleton className="h-10 w-full rounded-md" />
        </div>

        {/* Password Field Skeleton */}
        <div className="mb-6">
          <Skeleton className="h-4 w-24 mb-2" />
          <Skeleton className="h-10 w-full rounded-md" />
        </div>

        {/* Submit Button Skeleton */}
        <Skeleton className="h-12 w-full rounded-md" />
      </div>

      {/* Footer Skeleton */}
      <div className="text-center mt-6">
        <Skeleton className="h-4 w-64 mx-auto" />
      </div>
    </div>
  )
}
