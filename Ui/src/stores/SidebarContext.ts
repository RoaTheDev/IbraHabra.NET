import { createContext, use } from 'react'

interface SidebarContextType {
  sidebarOpen: boolean
  setSidebarOpen: (open: boolean) => void
}

export const SidebarContext = createContext<SidebarContextType | undefined>(undefined)

export const useSidebar = () => {
  const context = use(SidebarContext)
  if (!context) {
    throw new Error('useSidebar must be used within SidebarProvider')
  }
  return context
}