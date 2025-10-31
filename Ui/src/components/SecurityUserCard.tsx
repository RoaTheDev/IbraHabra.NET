import { User, ShieldAlert, Clock, CheckCircle2, Lock } from "lucide-react";
import { cn } from "@/lib/utils";

interface SecurityUserCardProps {
  name: string;
  role: string;
  activity: number;
  permissions: number;
  status: "active" | "pending" | "locked";
  lastActive?: string;
}

export const SecurityUserCard = ({ name, role, activity, permissions, status, lastActive = "2m ago" }: SecurityUserCardProps) => {
  const statusConfig = {
    active: { color: "text-security-success", bgColor: "bg-security-success/20", icon: CheckCircle2, label: "ACTIVE" },
    pending: { color: "text-security-warning", bgColor: "bg-security-warning/20", icon: Clock, label: "PENDING" },
    locked: { color: "text-security-critical", bgColor: "bg-security-critical/20", icon: Lock, label: "LOCKED" },
  };

  const StatusIcon = statusConfig[status].icon;

  return (
    <div className="bg-card border border-border rounded hover:border-primary/50 transition-all duration-300 group relative overflow-hidden">
      {/* Scan line effect on hover */}
      <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">
        <div className="absolute inset-x-0 h-px bg-gradient-to-r from-transparent via-primary/50 to-transparent scan-line"></div>
      </div>

      <div className="p-4 relative">
        <div className="flex items-start gap-3">
          {/* Avatar */}
          <div className="relative flex-shrink-0">
            <div className="h-11 w-11 bg-muted/50 rounded flex items-center justify-center border border-border group-hover:border-primary/50 transition-colors">
              <User className="h-5 w-5 text-muted-foreground" />
            </div>
            <div className={cn("absolute -bottom-1 -right-1 rounded-full p-0.5", statusConfig[status].bgColor)}>
              <StatusIcon className={cn("h-3 w-3", statusConfig[status].color)} />
            </div>
          </div>

          {/* Info */}
          <div className="flex-1 min-w-0">
            <h3 className="font-bold text-sm text-foreground truncate uppercase tracking-wide">{name}</h3>
            <p className="text-xs text-muted-foreground uppercase tracking-wide">{role}</p>
            <p className="text-[10px] text-security-neutral font-mono mt-1">Last: {lastActive}</p>
          </div>

          {/* Status Badge */}
          <div className={cn(
            "px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider",
            statusConfig[status].bgColor,
            statusConfig[status].color
          )}>
            {statusConfig[status].label}
          </div>
        </div>

        {/* Activity Metric */}
        <div className="mt-4 space-y-2">
          <div className="flex items-center justify-between text-[10px] font-mono uppercase tracking-wider">
            <span className="text-security-info font-bold">Activity Level</span>
            <span className="text-muted-foreground">{activity}%</span>
          </div>
          <div className="h-1.5 bg-muted/50 rounded-full overflow-hidden">
            <div
              className="h-full bg-gradient-to-r from-security-info to-security-cyan bar-animate relative"
              style={{ width: `${activity}%` }}
            >
              <div className="absolute inset-0 bg-gradient-to-r from-transparent to-white/20" />
            </div>
          </div>
        </div>

        {/* Permission Metric */}
        <div className="mt-2 space-y-2">
          <div className="flex items-center justify-between text-[10px] font-mono uppercase tracking-wider">
            <span className="text-security-success font-bold">Permission Usage</span>
            <span className="text-muted-foreground">{permissions}%</span>
          </div>
          <div className="h-1.5 bg-muted/50 rounded-full overflow-hidden">
            <div
              className="h-full bg-gradient-to-r from-security-success to-security-cyan bar-animate relative"
              style={{ width: `${permissions}%` }}
            >
              <div className="absolute inset-0 bg-gradient-to-r from-transparent to-white/20" />
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="mt-4 pt-3 border-t border-border/50 flex gap-2">
          <button className="flex-1 text-[10px] py-1.5 rounded bg-muted/50 hover:bg-muted transition-colors font-bold uppercase tracking-wider border border-border hover:border-primary/50">
            Details
          </button>
          <button className="px-3 py-1.5 rounded bg-primary/20 hover:bg-primary/30 text-primary transition-colors">
            <ShieldAlert className="h-3 w-3" />
          </button>
        </div>
      </div>
    </div>
  );
};