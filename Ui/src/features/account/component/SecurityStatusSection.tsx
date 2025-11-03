import { Activity, Key, Shield } from 'lucide-react'

export function SecurityStatusSection({ userInfo }: { userInfo: any }) {
  return (
    <section className="bg-card border border-border rounded-lg p-6">
      <div className="flex items-center gap-2 mb-4">
        <Activity className="h-5 w-5 text-primary" />
        <h2 className="text-lg font-semibold">Security Status</h2>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="p-3 bg-muted/30 border border-border rounded">
          <div className="flex items-center gap-2 mb-1">
            <Key className="h-4 w-4 text-muted-foreground" />
            <span className="text-xs text-muted-foreground uppercase tracking-wide">Authentication</span>
          </div>
          <div className="text-sm font-medium">Token-based Auth</div>
        </div>

        <div className="p-3 bg-muted/30 border border-border rounded">
          <div className="flex items-center gap-2 mb-1">
            <Shield className="h-4 w-4 text-muted-foreground" />
            <span className="text-xs text-muted-foreground uppercase tracking-wide">2FA Status</span>
          </div>
          <div className="text-sm font-medium">
            {userInfo?.is2FaEnabled ? 'Protected' : 'Not Protected'}
          </div>
        </div>
      </div>
    </section>
  )
}
