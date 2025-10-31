import { Activity, Menu, ShieldCheck } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useEffect, useState } from 'react'

interface SecurityNavbarProps {
  onMenuClick: () => void
}

export const SecurityNavbar = ({ onMenuClick }: SecurityNavbarProps) => {
  const [currentTime, setCurrentTime] = useState<string>('')

  useEffect(() => {
    setCurrentTime(new Date().toLocaleTimeString('en-US', { hour12: false }))

    const interval = setInterval(() => {
      setCurrentTime(new Date().toLocaleTimeString('en-US', { hour12: false }))
    }, 1000)

    return () => clearInterval(interval)
  }, [])

  return (
    <nav className="h-14 bg-card/95 backdrop-blur-md border-b border-primary/30 flex items-center px-6 sticky top-0 z-50 lg:ml-64">
      <Button
        variant="ghost"
        size="icon"
        onClick={onMenuClick}
        className="mr-4 hover:bg-primary/10 hover:text-primary lg:hidden"
      >
        <Menu className="h-5 w-5" />
      </Button>

      <div className="flex items-center gap-3">
        <div className="relative">
          <ShieldCheck className="h-7 w-7 text-primary" />
          <div className="absolute inset-0 bg-primary/30 blur-lg rounded-full"></div>
        </div>
        <div>
          <h1 className="text-lg font-bold text-foreground tracking-wide">
            SENTINEL IAM
          </h1>
          <p className="text-[10px] text-security-cyan uppercase tracking-wider">
            Security Operations Center
          </p>
        </div>
      </div>

      <div className="ml-auto flex items-center gap-4">
        <div className="hidden md:flex items-center gap-2 px-3 py-1.5 bg-card border border-security-success/40 rounded">
          <Activity className="h-3 w-3 text-security-success animate-pulse" />
          <span className="text-xs font-mono text-security-success uppercase tracking-wide">
            Online
          </span>
        </div>
        <div className="text-xs font-mono text-muted-foreground" suppressHydrationWarning>
          {currentTime || '00:00:00'}
        </div>
      </div>
    </nav>
  )
}