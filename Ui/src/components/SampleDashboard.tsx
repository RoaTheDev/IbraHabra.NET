import { useState } from 'react'
import {
  Activity,
  Bell,
  Home,
  Key,
  Lock,
  LogOut,
  Search,
  Settings,
  Shield,
  UserPlus,
  Users,
} from 'lucide-react'

export function SampleDashboard() {
  const [activeTab, setActiveTab] = useState('overview')

  const menuItems = [
    { id: 'overview', label: 'Overview', icon: Home },
    { id: 'identities', label: 'Identities', icon: Users },
    { id: 'access', label: 'Access Control', icon: Lock },
    { id: 'audit', label: 'Audit Logs', icon: Activity },
    { id: 'settings', label: 'Settings', icon: Settings },
  ]

  return (
    <div className="flex h-screen bg-stone-50">
      {/* Vertical Sidebar */}
      <aside className="w-72 bg-gradient-to-b from-slate-900 to-slate-800 flex flex-col">
        {/* Brand */}
        <div className="p-6 border-b border-slate-700/50">
          <div className="flex items-center gap-3">
            <div className="relative">
              <div className="w-10 h-10 bg-gradient-to-br from-emerald-400 to-teal-500 rounded-lg transform rotate-3"></div>
              <div className="absolute inset-0 w-10 h-10 bg-gradient-to-br from-emerald-500 to-teal-600 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-lg">IH</span>
              </div>
            </div>
            <div>
              <h1 className="text-white font-bold text-lg tracking-tight">
                IBraHabra
              </h1>
              <p className="text-emerald-400 text-xs font-medium">.NET</p>
            </div>
          </div>
        </div>

        {/* Search */}
        <div className="px-4 pt-6 pb-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Search..."
              className="w-full bg-slate-800/50 border border-slate-700/50 rounded-lg pl-10 pr-4 py-2.5 text-sm text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/50 focus:border-emerald-500/50 transition-all"
            />
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-4 space-y-1">
          {menuItems.map((item) => {
            const Icon = item.icon
            const isActive = activeTab === item.id
            return (
              <button
                key={item.id}
                onClick={() => setActiveTab(item.id)}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all ${
                  isActive
                    ? 'bg-gradient-to-r from-emerald-500/20 to-teal-500/20 text-emerald-400 shadow-lg shadow-emerald-500/10'
                    : 'text-slate-300 hover:bg-slate-800/50 hover:text-white'
                }`}
              >
                <Icon className="w-5 h-5 flex-shrink-0" />
                <span className="font-medium text-sm">{item.label}</span>
                {isActive && (
                  <div className="ml-auto w-1.5 h-1.5 bg-emerald-400 rounded-full"></div>
                )}
              </button>
            )
          })}
        </nav>

        {/* User Section */}
        <div className="p-4 border-t border-slate-700/50">
          <div className="flex items-center gap-3 px-3 py-3 bg-slate-800/50 rounded-xl">
            <div className="w-10 h-10 bg-gradient-to-br from-emerald-400 to-teal-500 rounded-full flex items-center justify-center">
              <span className="text-sm font-bold text-slate-900">SA</span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-white truncate">
                System Admin
              </p>
              <p className="text-xs text-slate-400 truncate">admin@ibra.net</p>
            </div>
            <button className="p-2 hover:bg-slate-700/50 rounded-lg transition-colors">
              <LogOut className="w-4 h-4 text-slate-400" />
            </button>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Top Bar */}
        <header className="h-20 bg-white border-b border-stone-200 flex items-center justify-between px-8">
          <div>
            <h2 className="text-2xl font-bold text-slate-900 mb-1">
              {menuItems.find((item) => item.id === activeTab)?.label}
            </h2>
            <p className="text-sm text-slate-500">
              Manage your identity and access infrastructure
            </p>
          </div>
          <div className="flex items-center gap-3">
            <button className="relative p-3 hover:bg-stone-100 rounded-xl transition-colors">
              <Bell className="w-5 h-5 text-slate-600" />
              <div className="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full"></div>
            </button>
            <button className="flex items-center gap-2 px-5 py-3 bg-gradient-to-r from-emerald-500 to-teal-500 text-white font-medium rounded-xl hover:from-emerald-600 hover:to-teal-600 transition-all shadow-lg shadow-emerald-500/20">
              <UserPlus className="w-4 h-4" />
              <span>New Identity</span>
            </button>
          </div>
        </header>

        {/* Content Area */}
        <div className="flex-1 overflow-auto bg-stone-50">
          <div className="p-8 max-w-[1600px] mx-auto">
            {/* Stats Grid */}
            <div className="grid grid-cols-4 gap-6 mb-8">
              <div className="bg-white rounded-2xl p-6 border border-stone-200 hover:shadow-xl hover:shadow-emerald-500/5 transition-all group">
                <div className="flex items-start justify-between mb-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-emerald-100 to-teal-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                    <Users className="w-6 h-6 text-emerald-600" />
                  </div>
                  <span className="text-xs font-bold text-emerald-600 bg-emerald-50 px-3 py-1.5 rounded-lg">
                    LIVE
                  </span>
                </div>
                <h3 className="text-3xl font-bold text-slate-900 mb-1">
                  2,847
                </h3>
                <p className="text-sm text-slate-500 font-medium">
                  Active Identities
                </p>
                <div className="mt-4 pt-4 border-t border-stone-100">
                  <span className="text-xs text-emerald-600 font-semibold">
                    ↑ 18.2% from last month
                  </span>
                </div>
              </div>

              <div className="bg-white rounded-2xl p-6 border border-stone-200 hover:shadow-xl hover:shadow-violet-500/5 transition-all group">
                <div className="flex items-start justify-between mb-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-violet-100 to-purple-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                    <Shield className="w-6 h-6 text-violet-600" />
                  </div>
                  <span className="text-xs font-bold text-violet-600 bg-violet-50 px-3 py-1.5 rounded-lg">
                    PROTECTED
                  </span>
                </div>
                <h3 className="text-3xl font-bold text-slate-900 mb-1">156</h3>
                <p className="text-sm text-slate-500 font-medium">
                  Security Policies
                </p>
                <div className="mt-4 pt-4 border-t border-stone-100">
                  <span className="text-xs text-slate-500 font-semibold">
                    12 updated today
                  </span>
                </div>
              </div>

              <div className="bg-white rounded-2xl p-6 border border-stone-200 hover:shadow-xl hover:shadow-amber-500/5 transition-all group">
                <div className="flex items-start justify-between mb-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-amber-100 to-orange-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                    <Key className="w-6 h-6 text-amber-600" />
                  </div>
                  <span className="text-xs font-bold text-amber-600 bg-amber-50 px-3 py-1.5 rounded-lg">
                    ATTENTION
                  </span>
                </div>
                <h3 className="text-3xl font-bold text-slate-900 mb-1">23</h3>
                <p className="text-sm text-slate-500 font-medium">
                  Expiring Credentials
                </p>
                <div className="mt-4 pt-4 border-t border-stone-100">
                  <span className="text-xs text-amber-600 font-semibold">
                    5 expire this week
                  </span>
                </div>
              </div>

              <div className="bg-white rounded-2xl p-6 border border-stone-200 hover:shadow-xl hover:shadow-blue-500/5 transition-all group">
                <div className="flex items-start justify-between mb-4">
                  <div className="w-12 h-12 bg-gradient-to-br from-blue-100 to-cyan-100 rounded-xl flex items-center justify-center group-hover:scale-110 transition-transform">
                    <Activity className="w-6 h-6 text-blue-600" />
                  </div>
                  <span className="text-xs font-bold text-blue-600 bg-blue-50 px-3 py-1.5 rounded-lg">
                    24H
                  </span>
                </div>
                <h3 className="text-3xl font-bold text-slate-900 mb-1">
                  8,924
                </h3>
                <p className="text-sm text-slate-500 font-medium">
                  Auth Requests
                </p>
                <div className="mt-4 pt-4 border-t border-stone-100">
                  <span className="text-xs text-blue-600 font-semibold">
                    ↑ 8.4% from yesterday
                  </span>
                </div>
              </div>
            </div>

            {/* Main Content Card */}
            <div className="bg-white rounded-2xl border border-stone-200 overflow-hidden shadow-sm">
              <div className="p-6 border-b border-stone-200 bg-gradient-to-r from-slate-50 to-stone-50">
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="text-lg font-bold text-slate-900 mb-1">
                      Recent Access Events
                    </h3>
                    <p className="text-sm text-slate-500">
                      Monitor authentication and authorization activity
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <select className="px-4 py-2 bg-white border border-stone-200 rounded-xl text-sm font-medium text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500/50">
                      <option>Last 24 hours</option>
                      <option>Last 7 days</option>
                      <option>Last 30 days</option>
                    </select>
                  </div>
                </div>
              </div>
              <div className="divide-y divide-stone-100">
                {[
                  {
                    user: 'Sarah Chen',
                    action: 'Role assigned: DevOps Lead',
                    time: '2 min ago',
                    status: 'success',
                  },
                  {
                    user: 'Marcus Johnson',
                    action: 'Password reset requested',
                    time: '8 min ago',
                    status: 'pending',
                  },
                  {
                    user: 'Priya Patel',
                    action: 'API key generated',
                    time: '15 min ago',
                    status: 'success',
                  },
                  {
                    user: 'Alex Rivera',
                    action: 'Failed login attempt (3x)',
                    time: '22 min ago',
                    status: 'warning',
                  },
                  {
                    user: 'Emma Wilson',
                    action: 'MFA enabled',
                    time: '1 hour ago',
                    status: 'success',
                  },
                ].map((event, i) => (
                  <div
                    key={i}
                    className="p-6 hover:bg-stone-50 transition-colors"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gradient-to-br from-slate-100 to-stone-100 rounded-xl flex items-center justify-center">
                          <span className="text-sm font-bold text-slate-700">
                            {event.user
                              .split(' ')
                              .map((n) => n[0])
                              .join('')}
                          </span>
                        </div>
                        <div>
                          <p className="text-sm font-semibold text-slate-900">
                            {event.user}
                          </p>
                          <p className="text-sm text-slate-500 mt-0.5">
                            {event.action}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-center gap-4">
                        <span className="text-sm text-slate-400 font-medium">
                          {event.time}
                        </span>
                        <span
                          className={`px-3 py-1.5 rounded-lg text-xs font-bold ${
                            event.status === 'success'
                              ? 'bg-emerald-50 text-emerald-700'
                              : event.status === 'warning'
                                ? 'bg-amber-50 text-amber-700'
                                : 'bg-blue-50 text-blue-700'
                          }`}
                        >
                          {event.status.toUpperCase()}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}
