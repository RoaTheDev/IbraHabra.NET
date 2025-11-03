import { createFileRoute, Outlet, redirect } from '@tanstack/react-router'
import { SecurityNavbar } from '@/components/SecurityNavbar.tsx'
import { SecuritySidebar } from '@/components/SecuritySidebar.tsx'
import { SecurityFooter } from '@/components/SecurityFooter.tsx'
import { useState } from 'react'
import {
  adminAuthStore,
  adminAuthStoreAction,
} from '@/stores/adminAuthStore.ts'
import { authApi } from '@/features/admin/auth/adminAuthApi.ts'
import { Skeleton } from '@/components/ui/skeleton'
import { Activity, ShieldCheck } from 'lucide-react'
import { SidebarContext } from '@/stores/SidebarContext'
import { clientCacheKeys } from '@/constants/clientCacheKeys.ts'
import { sessionUtils } from '@/lib/sessionUtils.ts'
import { AxiosError } from 'axios'

let lastVerifyAttempt = 0
let consecutiveFailures = 0
const BASE_COOLDOWN = 10_000
const MAX_COOLDOWN = 5 * 60 * 1000
const VERIFY_INTERVAL = 1000 * 60 * 15

const getBackoffDelay = () => {
  if (consecutiveFailures === 0) return BASE_COOLDOWN
  return Math.min(
    BASE_COOLDOWN * Math.pow(2, consecutiveFailures),
    MAX_COOLDOWN,
  )
}

export const Route = createFileRoute('/_authenticated')({
  component: AuthenticatedLayout,
  staleTime: Infinity,
  ssr: false,
  pendingComponent: AuthLoadingSkeleton,

  beforeLoad: async () => {
    const now = Date.now()
    const backoffDelay = getBackoffDelay()

    if (now - lastVerifyAttempt < backoffDelay) {
      return
    }

    lastVerifyAttempt = now

    adminAuthStoreAction.rehydrate()
    const { token, sessionCode2Fa } = adminAuthStore.state

    if (!token) {
      throw redirect({ to: '/auth/login' })
    }

    if (sessionCode2Fa) {
      throw redirect({ to: '/auth/2fa' })
    }

    if (typeof window !== 'undefined') {
      const lastVerified = sessionUtils.get<string>(
        clientCacheKeys.last_verified_session,
      )
      const shouldVerify =
        !lastVerified || now - parseInt(lastVerified) > VERIFY_INTERVAL

      if (shouldVerify) {
        try {
          await authApi.verify()
          sessionUtils.set(
            clientCacheKeys.last_verified_session,
            now.toString(),
          )
          consecutiveFailures = 0
        } catch (error: unknown) {
          if (error instanceof AxiosError) {
            const isAuthError = error.response?.status === 401

            if (isAuthError) {
              console.error('Authentication failed - token invalid')
              sessionUtils.remove(clientCacheKeys.last_verified_session)
              adminAuthStoreAction.reset()
              consecutiveFailures = 0
              throw redirect({ to: '/auth/login' })
            } else {
              consecutiveFailures++
              console.warn(
                `Token verification failed (attempt ${consecutiveFailures}), backing off for ${backoffDelay}ms:`,
                error.message,
              )
            }
          } else {
            consecutiveFailures++
            console.warn('Unexpected error during  verification:', error)
          }
        }
      }
    }
  },
})

function AuthenticatedLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)

  return (
    <SidebarContext.Provider value={{ sidebarOpen, setSidebarOpen }}>
      <SecurityNavbar onMenuClick={() => setSidebarOpen(true)} />
      <SecuritySidebar
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      />
      <main className="lg:ml-64 min-h-[calc(100vh-3.5rem-4rem)] pt-14 pb-16">
        <Outlet />
      </main>
      <SecurityFooter />
    </SidebarContext.Provider>
  )
}

function AuthLoadingSkeleton() {
  return (
    <div className="min-h-screen bg-background">
      {/* Navbar Skeleton */}
      <nav className="h-14 bg-card/95 backdrop-blur-md border-b border-primary/30 flex items-center px-6 sticky top-0 z-50 lg:ml-64">
        <div className="flex items-center gap-3">
          <div className="relative">
            <ShieldCheck className="h-7 w-7 text-primary/50 animate-pulse" />
            <div className="absolute inset-0 bg-primary/20 blur-lg rounded-full"></div>
          </div>
          <div>
            <Skeleton className="h-5 w-32 mb-1" />
            <Skeleton className="h-3 w-40" />
          </div>
        </div>

        <div className="ml-auto flex items-center gap-4">
          <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-card border border-primary/40 rounded">
            <Activity className="h-3 w-3 text-primary/50 animate-pulse" />
            <Skeleton className="h-3 w-12" />
          </div>
          <Skeleton className="h-4 w-16" />
        </div>
      </nav>

      {/* Sidebar Skeleton */}
      <aside className="fixed top-0 left-0 h-screen w-64 bg-sidebar border-r border-sidebar-border z-40 hidden lg:flex flex-col">
        <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
          {[1, 2, 3, 4, 5].map((i) => (
            <div
              key={i}
              className="w-full flex items-center gap-3 px-3 py-2.5 rounded border-l-2 border-transparent"
            >
              <Skeleton className="h-4 w-4 rounded" />
              <Skeleton className="h-4 flex-1" />
            </div>
          ))}
        </nav>

        <div className="p-3 border-t border-sidebar-border shrink-0">
          <div className="bg-card border border-border rounded p-3 space-y-2">
            <div className="flex items-center gap-2 mb-2">
              <Activity className="h-3 w-3 text-primary/50 animate-pulse" />
              <Skeleton className="h-3 w-24" />
            </div>
            <div className="space-y-1.5">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex justify-between">
                  <Skeleton className="h-3 w-16" />
                  <Skeleton className="h-3 w-8" />
                </div>
              ))}
            </div>
          </div>
        </div>
      </aside>

      {/* Empty main area - clean and minimal */}
      <main className="lg:ml-64 min-h-[calc(100vh-3.5rem-4rem)] pt-14 bg-background" />

      {/* Footer Skeleton */}
      <footer className="bg-card/95 backdrop-blur-md border-t border-border py-2.5 px-6 lg:ml-64">
        <div className="flex items-center justify-between flex-wrap gap-4">
          <div className="flex items-center gap-4">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex items-center gap-2">
                <Skeleton className="h-3.5 w-3.5 rounded" />
                <div className="flex items-baseline gap-1.5">
                  <Skeleton className="h-3 w-10" />
                  <Skeleton className="h-2 w-16" />
                </div>
              </div>
            ))}
          </div>

          <div className="flex items-center gap-2">
            <div className="h-1.5 w-1.5 bg-primary/50 rounded-full animate-pulse" />
            <Skeleton className="h-1 w-32" />
          </div>
        </div>
      </footer>
    </div>
  )
}
