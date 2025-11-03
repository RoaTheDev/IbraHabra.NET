import { Button } from '@/components/ui/button'
import { LogOut, AlertTriangle } from 'lucide-react'

export function DangerZoneSection({ onLogout }: { onLogout: () => void }) {
  return (
    <section className="bg-card border border-destructive/30 rounded-lg p-6">
      <div className="flex items-center gap-2 mb-4">
        <AlertTriangle className="h-5 w-5 text-destructive" />
        <h2 className="text-lg font-semibold text-destructive">Danger Zone</h2>
      </div>

      <div className="space-y-3">
        <p className="text-sm text-muted-foreground">
          Logout from your account and clear all session data.
        </p>

        <Button
          onClick={onLogout}
          variant="outline"
          className="border-destructive/50 text-destructive hover:bg-destructive/10"
        >
          <LogOut className="h-4 w-4 mr-2" /> Logout
        </Button>
      </div>
    </section>
  )
}
