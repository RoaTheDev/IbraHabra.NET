import {
  AlertTriangle,
  CheckCircle2,
  Info,
  Terminal,
  XCircle,
} from 'lucide-react'
import { cn } from '@/lib/utils'

interface LogEntry {
  id: string
  type: 'success' | 'warning' | 'error' | 'info'
  message: string
  timestamp: string
  source: string
}

const mockLogs: LogEntry[] = [
  {
    id: '1',
    type: 'success',
    message: 'User role privileges updated',
    timestamp: '14:23:45',
    source: 'AUTH_SVC',
  },
  {
    id: '2',
    type: 'warning',
    message: 'Multiple authentication failures detected',
    timestamp: '14:21:12',
    source: 'SECURITY',
  },
  {
    id: '3',
    type: 'info',
    message: 'New user account provisioned',
    timestamp: '14:18:33',
    source: 'USER_MGT',
  },
  {
    id: '4',
    type: 'error',
    message: 'Unauthorized access attempt blocked',
    timestamp: '14:15:07',
    source: 'FIREWALL',
  },
  {
    id: '5',
    type: 'success',
    message: 'Database synchronization completed',
    timestamp: '14:12:44',
    source: 'DB_SYNC',
  },
  {
    id: '6',
    type: 'info',
    message: 'Session token expired and renewed',
    timestamp: '14:09:21',
    source: 'AUTH_SVC',
  },
  {
    id: '7',
    type: 'warning',
    message: 'Elevated privilege request pending',
    timestamp: '14:05:18',
    source: 'ACCESS_CTL',
  },
]

export const SecurityLogFeed = () => {
  const getLogConfig = (type: LogEntry['type']) => {
    switch (type) {
      case 'success':
        return {
          icon: CheckCircle2,
          color: 'text-security-success',
          borderColor: 'border-l-security-success',
        }
      case 'warning':
        return {
          icon: AlertTriangle,
          color: 'text-security-warning',
          borderColor: 'border-l-security-warning',
        }
      case 'error':
        return {
          icon: XCircle,
          color: 'text-security-critical',
          borderColor: 'border-l-security-critical',
        }
      case 'info':
        return {
          icon: Info,
          color: 'text-security-info',
          borderColor: 'border-l-security-info',
        }
    }
  }

  return (
    <div className="bg-card border border-border rounded overflow-hidden">
      <div className="px-4 py-3 border-b border-border bg-muted/30 flex items-center justify-between">
        <h3 className="font-bold text-sm text-foreground flex items-center gap-2 uppercase tracking-wide">
          <Terminal className="h-4 w-4 text-primary" />
          Security Event Log
        </h3>
        <div className="flex items-center gap-2">
          <div className="h-1.5 w-1.5 bg-security-success rounded-full animate-pulse" />
          <span className="text-[10px] text-security-success font-mono uppercase tracking-wider">
            Live
          </span>
        </div>
      </div>

      <div className="max-h-96 overflow-y-auto font-mono">
        {mockLogs.map((log, index) => {
          const config = getLogConfig(log.type)
          const Icon = config.icon

          return (
            <div
              key={log.id}
              className={cn(
                'px-4 py-2.5 border-b border-border/30 hover:bg-muted/20 transition-colors border-l-2',
                config.borderColor,
                index % 2 === 0 ? 'bg-muted/5' : 'bg-transparent',
              )}
            >
              <div className="flex items-start gap-3">
                <Icon
                  className={cn(
                    'h-3.5 w-3.5 mt-0.5 flex-shrink-0',
                    config.color,
                  )}
                />

                <div className="flex-1 min-w-0">
                  <p className="text-xs text-foreground">{log.message}</p>
                  <div className="flex items-center gap-3 mt-1">
                    <span className="text-[10px] text-security-cyan uppercase tracking-wider">
                      {log.source}
                    </span>
                    <span className="text-[10px] text-muted-foreground">•</span>
                    <span className="text-[10px] text-muted-foreground">
                      {log.timestamp}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          )
        })}
      </div>

      <div className="px-4 py-2 bg-muted/20 border-t border-border">
        <button className="text-[10px] text-primary hover:text-primary/80 font-bold uppercase tracking-wider">
          View Full Log →
        </button>
      </div>
    </div>
  )
}
