import { Eye, Lock, Shield, Zap } from 'lucide-react'
import { cn } from '@/lib/utils'

interface RoleAccessPanelProps {
  name: string
  permissions: number
  type: 'admin' | 'user' | 'viewer' | 'restricted'
  users: number
}

export const RoleAccessPanel = ({
  name,
  permissions,
  type,
  users,
}: RoleAccessPanelProps) => {
  const typeConfig = {
    admin: {
      color: 'border-security-critical',
      icon: Shield,
      bgColor: 'bg-security-critical/10',
      textColor: 'text-security-critical',
    },
    user: {
      color: 'border-security-info',
      icon: Zap,
      bgColor: 'bg-security-info/10',
      textColor: 'text-security-info',
    },
    viewer: {
      color: 'border-security-success',
      icon: Eye,
      bgColor: 'bg-security-success/10',
      textColor: 'text-security-success',
    },
    restricted: {
      color: 'border-security-warning',
      icon: Lock,
      bgColor: 'bg-security-warning/10',
      textColor: 'text-security-warning',
    },
  }

  const Icon = typeConfig[type].icon
  const config = typeConfig[type]

  return (
    <div
      className={cn(
        'bg-card border-2 rounded transition-all duration-300 relative overflow-hidden',
        'hover:scale-[1.02] cursor-pointer group',
        config.color,
      )}
    >
      {/* Background grid effect */}
      <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity">
        <div className="grid-bg h-full w-full opacity-20"></div>
      </div>

      <div className="p-4 relative">
        <div className="flex items-start gap-3 mb-3">
          <div className={cn('p-2 rounded', config.bgColor)}>
            <Icon className={cn('h-5 w-5', config.textColor)} />
          </div>

          <div className="flex-1 min-w-0">
            <h4 className="font-bold text-sm text-foreground uppercase tracking-wide">
              {name}
            </h4>
            <p className="text-[10px] text-muted-foreground font-mono uppercase tracking-wider mt-0.5">
              {permissions} Permissions
            </p>
          </div>

          <div
            className={cn(
              'h-7 w-7 rounded flex items-center justify-center font-bold text-xs border',
              config.bgColor,
              config.textColor,
              config.color,
            )}
          >
            {permissions}
          </div>
        </div>

        <div className="space-y-2 mb-3">
          <div className="h-px bg-border"></div>
          <div className="flex justify-between items-center">
            <span className="text-[10px] text-muted-foreground uppercase tracking-wider font-mono">
              Assigned Users
            </span>
            <span
              className={cn('text-xs font-bold font-mono', config.textColor)}
            >
              {users}
            </span>
          </div>
        </div>

        <div className="flex gap-2">
          <button className="flex-1 text-[10px] py-1.5 rounded bg-muted/50 hover:bg-muted transition-colors font-bold uppercase tracking-wider border border-border">
            Configure
          </button>
          <button
            className={cn(
              'flex-1 text-[10px] py-1.5 rounded transition-colors font-bold uppercase tracking-wider',
              config.bgColor,
              config.textColor,
              'hover:brightness-110',
            )}
          >
            Assign
          </button>
        </div>
      </div>
    </div>
  )
}
