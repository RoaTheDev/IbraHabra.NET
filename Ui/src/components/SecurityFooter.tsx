import { Activity, AlertCircle, Clock, Shield, Users } from 'lucide-react'

export const SecurityFooter = () => {
  const stats = [
    {
      icon: Shield,
      label: 'Security Level',
      value: 'HIGH',
      color: 'text-security-success',
    },
    {
      icon: Users,
      label: 'Active Sessions',
      value: '142',
      color: 'text-security-info',
    },
    {
      icon: AlertCircle,
      label: 'Alerts',
      value: '8',
      color: 'text-security-warning',
    },
    {
      icon: Activity,
      label: 'System Load',
      value: '47%',
      color: 'text-security-cyan',
    },
    {
      icon: Clock,
      label: 'Uptime',
      value: '99.9%',
      color: 'text-security-success',
    },
  ]

  return (
    <footer className="bg-card/95 backdrop-blur-md border-t border-border py-2.5 px-6 lg:ml-64">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div className="flex items-center gap-4">
          {stats.map((stat) => (
            <div key={stat.label} className="flex items-center gap-2">
              <stat.icon className={`h-3.5 w-3.5 ${stat.color}`} />
              <div className="flex items-baseline gap-1.5">
                <span className="text-xs font-bold text-foreground font-mono">
                  {stat.value}
                </span>
                <span className="text-[10px] text-muted-foreground uppercase tracking-wider">
                  {stat.label}
                </span>
              </div>
            </div>
          ))}
        </div>

        <div className="flex items-center gap-2 text-[10px] text-security-success font-mono uppercase tracking-wider">
          <div className="h-1.5 w-1.5 bg-security-success rounded-full animate-pulse" />
          <span>All Systems Secure</span>
        </div>
      </div>
    </footer>
  )
}
