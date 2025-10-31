import { createFileRoute } from '@tanstack/react-router'
import { SecurityLogFeed } from '@/components/SecurityLogFeed.tsx'
import { SecurityUserCard } from '@/components/SecurityUserCard.tsx'
import { RoleAccessPanel } from '@/components/RoleAccessPanel.tsx'

export const Route = createFileRoute('/_authenticated/')({ component: Index })

const mockUsers = [
  {
    name: 'Sarah Mitchell',
    role: 'System Administrator',
    activity: 95,
    permissions: 88,
    status: 'active' as const,
    lastActive: '2m ago',
  },
  {
    name: 'James Rodriguez',
    role: 'Senior Engineer',
    activity: 78,
    permissions: 92,
    status: 'active' as const,
    lastActive: '5m ago',
  },
  {
    name: 'Emily Chen',
    role: 'Security Analyst',
    activity: 65,
    permissions: 75,
    status: 'pending' as const,
    lastActive: '15m ago',
  },
  {
    name: 'Michael Brown',
    role: 'Team Lead',
    activity: 88,
    permissions: 85,
    status: 'active' as const,
    lastActive: '3m ago',
  },
  {
    name: 'Jessica Taylor',
    role: 'Junior Developer',
    activity: 42,
    permissions: 55,
    status: 'locked' as const,
    lastActive: '2h ago',
  },
  {
    name: 'David Kim',
    role: 'DevOps Engineer',
    activity: 91,
    permissions: 87,
    status: 'active' as const,
    lastActive: '1m ago',
  },
]

const mockRoles = [
  { name: 'System Admin', permissions: 127, type: 'admin' as const, users: 8 },
  { name: 'Developer', permissions: 45, type: 'user' as const, users: 34 },
  { name: 'Viewer', permissions: 12, type: 'viewer' as const, users: 89 },
  {
    name: 'Restricted',
    permissions: 3,
    type: 'restricted' as const,
    users: 11,
  },
]

function Index() {
  return (
    <div className="p-6 space-y-6">
      {/* Header with scan line effect */}
      <div className="space-y-2 relative">
        <div className="absolute -inset-4 bg-gradient-to-r from-transparent via-primary/5 to-transparent pointer-events-none"></div>
        <h1 className="text-2xl font-bold text-foreground uppercase tracking-wide">
          User Access Control
        </h1>
        <p className="text-sm text-muted-foreground uppercase tracking-wide font-mono">
          Identity and permissions management dashboard
        </p>
      </div>

      {/* User Grid */}
      <section>
        <div className="flex items-center gap-3 mb-4">
          <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/50 to-transparent"></div>
          <h2 className="text-sm font-bold text-primary uppercase tracking-wider font-mono">
            Active Users
          </h2>
          <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/50 to-transparent"></div>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {mockUsers.map((user) => (
            <SecurityUserCard key={user.name} {...user} />
          ))}
        </div>
      </section>

      {/* Roles Grid */}
      <section>
        <div className="flex items-center gap-3 mb-4">
          <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/50 to-transparent"></div>
          <h2 className="text-sm font-bold text-primary uppercase tracking-wider font-mono">
            Access Roles
          </h2>
          <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/50 to-transparent"></div>
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {mockRoles.map((role) => (
            <RoleAccessPanel key={role.name} {...role} />
          ))}
        </div>
      </section>

      {/* Security Log */}
      <section>
        <div className="flex items-center gap-3 mb-4">
          <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/50 to-transparent"></div>
          <h2 className="text-sm font-bold text-primary uppercase tracking-wider font-mono">
            Event Stream
          </h2>
          <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/50 to-transparent"></div>
        </div>
        <SecurityLogFeed />
      </section>
    </div>
  )
}