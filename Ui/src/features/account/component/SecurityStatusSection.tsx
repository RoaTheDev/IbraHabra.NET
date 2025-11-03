import { Shield, CheckCircle2, AlertTriangle, XCircle, Lock, Unlock } from 'lucide-react'
import { AdminUserInfoResponse } from '@/features/admin/auth/adminAuthTypes'

interface SecurityStatusSectionProps {
  userInfo?: AdminUserInfoResponse
}

export function SecurityStatusSection({ userInfo }: SecurityStatusSectionProps) {
  const calculateSecurityScore = () => {
    let score = 0
    let maxScore = 100

    if (userInfo?.twoFactorEnabled) {
      score += 75
    }

    if (userInfo?.emailConfirmed) {
      score += 25
    }


    return { score, maxScore }
  }

  const { score, maxScore } = calculateSecurityScore()
  const percentage = (score / maxScore) * 100

  // Determine security level
  const getSecurityLevel = () => {
    if (percentage >= 80) {
      return {
        level: 'Excellent',
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-50 dark:bg-green-950/20',
        borderColor: 'border-green-200 dark:border-green-900',
        icon: CheckCircle2,
        description: 'Your account has strong security measures in place.',
      }
    } else if (percentage >= 50) {
      return {
        level: 'Good',
        color: 'text-blue-600 dark:text-blue-400',
        bgColor: 'bg-blue-50 dark:bg-blue-950/20',
        borderColor: 'border-blue-200 dark:border-blue-900',
        icon: Shield,
        description: 'Your account is reasonably secure, but could be improved.',
      }
    } else if (percentage >= 25) {
      return {
        level: 'Fair',
        color: 'text-yellow-600 dark:text-yellow-400',
        bgColor: 'bg-yellow-50 dark:bg-yellow-950/20',
        borderColor: 'border-yellow-200 dark:border-yellow-900',
        icon: AlertTriangle,
        description: 'Your account security needs attention.',
      }
    } else {
      return {
        level: 'Weak',
        color: 'text-red-600 dark:text-red-400',
        bgColor: 'bg-red-50 dark:bg-red-950/20',
        borderColor: 'border-red-200 dark:border-red-900',
        icon: XCircle,
        description: 'Your account is at risk. Please improve your security.',
      }
    }
  }

  const securityLevel = getSecurityLevel()
  const StatusIcon = securityLevel.icon

  // Security checklist items
  const securityChecks = [
    {
      label: 'Two-Factor Authentication',
      enabled: userInfo?.twoFactorEnabled,
      weight: 'High Impact',
      recommendation: 'Enable 2FA to add an extra layer of security',
    },
    {
      label: 'Email Verified',
      enabled: userInfo?.emailConfirmed,
      weight: 'Medium Impact',
      recommendation: 'Verify your email to secure account recovery',
    },

  ]

  return (
    <section className="bg-card border border-border rounded-lg p-6">
      <div className="flex items-center gap-2 mb-4">
        <Shield className="h-5 w-5 text-primary" />
        <h2 className="text-lg font-semibold">Security Status</h2>
      </div>

      {/* Security Scorecard */}
      <div className={`p-4 rounded-lg border ${securityLevel.bgColor} ${securityLevel.borderColor} mb-6`}>
        <div className="flex items-start justify-between mb-3">
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg bg-white dark:bg-background`}>
              <StatusIcon className={`h-6 w-6 ${securityLevel.color}`} />
            </div>
            <div>
              <h3 className={`font-semibold text-lg ${securityLevel.color}`}>
                {securityLevel.level} Security
              </h3>
              <p className="text-sm text-muted-foreground mt-0.5">
                {securityLevel.description}
              </p>
            </div>
          </div>
          <div className="text-right">
            <div className={`text-2xl font-bold ${securityLevel.color}`}>
              {score}/{maxScore}
            </div>
            <p className="text-xs text-muted-foreground">Security Score</p>
          </div>
        </div>

        {/* Progress Bar */}
        <div className="relative h-3 bg-muted rounded-full overflow-hidden">
          <div
            className={`absolute inset-y-0 left-0 transition-all duration-500 rounded-full ${
              percentage >= 80
                ? 'bg-green-500'
                : percentage >= 50
                  ? 'bg-blue-500'
                  : percentage >= 25
                    ? 'bg-yellow-500'
                    : 'bg-red-500'
            }`}
            style={{ width: `${percentage}%` }}
          />
        </div>
      </div>

      {/* Security Checklist */}
      <div className="space-y-3">
        <h3 className="text-sm font-semibold text-foreground mb-3">Security Checklist</h3>
        {securityChecks.map((check, index) => (
          <div
            key={index}
            className="flex items-start gap-3 p-3 rounded-lg border border-border bg-muted/30 hover:bg-muted/50 transition-colors"
          >
            <div className="mt-0.5">
              {check.enabled ? (
                <div className="p-1 rounded-full bg-green-100 dark:bg-green-950/30">
                  <CheckCircle2 className="h-4 w-4 text-green-600 dark:text-green-400" />
                </div>
              ) : (
                <div className="p-1 rounded-full bg-red-100 dark:bg-red-950/30">
                  <XCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
                </div>
              )}
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <p className="text-sm font-medium text-foreground">
                  {check.label}
                </p>
                <span className={`text-xs px-2 py-0.5 rounded-full ${
                  check.weight === 'High Impact'
                    ? 'bg-red-100 dark:bg-red-950/30 text-red-700 dark:text-red-300'
                    : 'bg-yellow-100 dark:bg-yellow-950/30 text-yellow-700 dark:text-yellow-300'
                }`}>
                  {check.weight}
                </span>
              </div>
              {!check.enabled && (
                <p className="text-xs text-muted-foreground">
                  {check.recommendation}
                </p>
              )}
            </div>
            {check.enabled ? (
              <Lock className="h-4 w-4 text-green-600 dark:text-green-400 mt-0.5" />
            ) : (
              <Unlock className="h-4 w-4 text-red-600 dark:text-red-400 mt-0.5" />
            )}
          </div>
        ))}
      </div>

      {/* Quick Tips */}
      {percentage < 80 && (
        <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-900 rounded-lg">
          <h4 className="text-sm font-semibold text-blue-900 dark:text-blue-100 mb-2">
            💡 Quick Security Tips
          </h4>
          <ul className="text-xs text-blue-800 dark:text-blue-200 space-y-1">
            {!userInfo?.twoFactorEnabled && (
              <li>• Enable two-factor authentication for maximum protection</li>
            )}
            {!userInfo?.emailConfirmed && (
              <li>• Verify your email address to enable account recovery</li>
            )}

            <li>• Never share your password or recovery codes with anyone</li>
            <li>• Regularly review your account activity and sessions</li>
          </ul>
        </div>
      )}

      {/* Perfect Security Message */}
      {percentage === 100 && (
        <div className="mt-6 p-4 bg-green-50 dark:bg-green-950/20 border border-green-200 dark:border-green-900 rounded-lg">
          <div className="flex items-center gap-2">
            <CheckCircle2 className="h-5 w-5 text-green-600 dark:text-green-400" />
            <div>
              <h4 className="text-sm font-semibold text-green-900 dark:text-green-100">
                Perfect Security! 🎉
              </h4>
              <p className="text-xs text-green-800 dark:text-green-200 mt-1">
                Your account has all recommended security features enabled. Keep up the great work!
              </p>
            </div>
          </div>
        </div>
      )}
    </section>
  )
}