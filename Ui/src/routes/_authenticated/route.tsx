import { createFileRoute, Outlet, redirect } from '@tanstack/react-router'
import { SecurityNavbar } from '@/components/SecurityNavbar.tsx'
import { SecuritySidebar } from '@/components/SecuritySidebar.tsx'
import { SecurityFooter } from '@/components/SecurityFooter.tsx'
import { createContext, use, useState } from 'react'
import { adminAuthStore } from '@/stores/adminAuthStore.ts'
import { authApi } from '@/features/admin/auth/adminAuthApi.ts'

interface SidebarContextType {
  sidebarOpen: boolean
  setSidebarOpen: (open: boolean) => void
}

const SidebarContext = createContext<SidebarContextType | undefined>(undefined)

export const useSidebar = () => {
  const context = use(SidebarContext)
  if (!context) {
    throw new Error('useSidebar must be used within SidebarProvider')
  }
  return context
}

export const Route = createFileRoute('/_authenticated')({
  component: AuthenticatedLayout,
  beforeLoad: async () => {
    const { token, sessionCode2Fa } = adminAuthStore.state

    if (!token) {
      throw redirect({ to: '/auth/login' })
    }

    if (sessionCode2Fa) {
      throw redirect({ to: '/auth/2fa' })
    }

    try {
      await authApi.verify()
    } catch {
      throw redirect({ to: '/auth/login' })
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
      <main className="lg:ml-64 min-h-[calc(100vh-3.5rem-4rem)] pt-14">
        <Outlet />
      </main>
      <SecurityFooter />
    </SidebarContext.Provider>
  )
}
