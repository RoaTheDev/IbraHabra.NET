import {
  Activity,
  ActivityIcon,
  LayoutDashboardIcon,
  ProjectorIcon,
  Settings,
  Shield,
  Users,
  X,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { ComponentType, SVGProps } from 'react'
import { FileRouteTypes } from '@/routeTree.gen.ts'
import { Link, useRouterState } from '@tanstack/react-router'

interface SecuritySidebarProps {
  isOpen: boolean
  onClose: () => void
}

type MenuItems = {
  icon: ComponentType<SVGProps<SVGSVGElement>>
  label: string
  route: FileRouteTypes['to']
}
const menuItems: MenuItems[] = [
  { icon: LayoutDashboardIcon, label: 'Main UI', route: '/' },
  { icon: Users, label: 'User Management', route: '/user' },
  { icon: Shield, label: 'Role Control', route: '/role' },
  { icon: ProjectorIcon, label: 'Security Logs', route: '/project' },
  { icon: ActivityIcon, label: 'Client Management', route: '/client' },
  { icon: Settings, label: 'System Config', route: '/sys-config' },
  // { icon: Account, label: 'System Config' ,route: '/account'},
]

export const SecuritySidebar = ({ isOpen, onClose }: SecuritySidebarProps) => {
  const router = useRouterState()
  const currentPath = router.location.pathname

  const isActive = (route: string) => {
    if (route === '/') {
      return currentPath === '/'
    }
    return currentPath.startsWith(route)
  }
  return (
    <>
      {/* Mobile overlay */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-background/80 backdrop-blur-sm z-40 lg:hidden"
          onClick={onClose}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed top-0 left-0 h-screen w-64 bg-sidebar border-r border-sidebar-border z-50 transition-transform duration-300',
          'flex flex-col',
          !isOpen && '-translate-x-full lg:translate-x-0',
        )}
      >
        {/* Header (non-scrolling) */}
        <div className="flex items-center justify-between p-4 border-b border-sidebar-border lg:hidden">
          <span className="font-bold text-sidebar-foreground uppercase text-sm tracking-wide">
            Navigation
          </span>
          <Button
            variant="ghost"
            size="icon"
            onClick={onClose}
            className="hover:bg-sidebar-accent h-8 w-8"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>

        {/* Scrollable nav */}
        <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
          {' '}
          {/* ← Only this scrolls */}
          {menuItems.map((item) => (
            <Link
              key={item.label}
              to={item.route}
              className={cn(
                'w-full flex items-center gap-3 px-3 py-2.5 rounded transition-all duration-200',
                'text-sidebar-foreground font-medium text-sm uppercase tracking-wide',
                isActive(item.route)
                  ? 'bg-primary/20 border-l-2 border-primary text-primary'
                  : 'hover:bg-sidebar-accent/50 hover:translate-x-0.5 border-l-2 border-transparent',
              )}
              onClick={onClose}
            >
              <item.icon className="h-4 w-4 flex-shrink-0" />
              <span className="truncate">{item.label}</span>
              {isActive(item.route) && (
                <div className="ml-auto h-1.5 w-1.5 bg-primary rounded-full animate-pulse" />
              )}
            </Link>
          ))}
        </nav>

        {/* Footer (non-scrolling) */}
        <div className="p-3 border-t border-sidebar-border shrink-0">
          {' '}
          {/* ← Prevent this from scrolling */}
          <div className="bg-card border border-border rounded p-3 space-y-2">
            <div className="flex items-center gap-2 mb-2">
              <Activity className="h-3 w-3 text-security-info" />
              <p className="text-[10px] text-muted-foreground uppercase tracking-wider font-mono">
                System Status
              </p>
            </div>
            <div className="space-y-1.5">
              <div className="flex justify-between text-xs font-mono">
                <span className="text-muted-foreground">Active</span>
                <span className="font-bold text-security-success">142</span>
              </div>
              <div className="flex justify-between text-xs font-mono">
                <span className="text-muted-foreground">Pending</span>
                <span className="font-bold text-security-warning">8</span>
              </div>
              <div className="flex justify-between text-xs font-mono">
                <span className="text-muted-foreground">Threats</span>
                <span className="font-bold text-security-critical">0</span>
              </div>
            </div>
          </div>
        </div>
      </aside>
    </>
  )
}
