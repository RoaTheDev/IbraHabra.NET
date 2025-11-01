import { Shield } from 'lucide-react'
import { Skeleton } from '@/components/ui/skeleton.tsx'

export function TwoFactorSkeleton() {
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

      {/* 2FA Form Card Skeleton */}
      <div className="bg-card border border-border rounded-lg p-8 shadow-2xl backdrop-blur-sm border-glow-cyan animate-pulse">
        {/* Title Skeleton */}
        <div className="text-center mb-6">
          <Skeleton className="h-6 w-48 mx-auto mb-2" />
          <Skeleton className="h-4 w-64 mx-auto" />
        </div>

        {/* API Error Placeholder (hidden but reserved) */}
        <div className="h-6 mb-6" />

        {/* OTP Input Skeleton */}
        <div className="flex justify-center mb-6">
          <div className="flex gap-2">
            {[0, 1, 2, 3, 4, 5].map((i) => (
              <Skeleton key={i} className="w-10 h-12 rounded-md" />
            ))}
          </div>
        </div>

        {/* Submit Button Skeleton */}
        <Skeleton className="h-12 w-full rounded-md mb-4" />

        {/* Back Link Skeleton */}
        <Skeleton className="h-4 w-32 mx-auto" />
      </div>

      {/* Footer Skeleton */}
      <div className="text-center mt-6">
        <Skeleton className="h-4 w-64 mx-auto" />
      </div>
    </div>
  )
}
