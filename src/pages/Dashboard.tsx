import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Activity, Clock, TrendingUp, Users } from "lucide-react";
import {
  AreaChart, Area, BarChart, Bar, PieChart, Pie, Cell,
  CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis, Legend,
} from "recharts";
import { cn } from "@/lib/utils";

// ── Mock data ────────────────────────────────────────────────────────────────

const WEEKLY_VISITS = [
  { day: "Mon", visitors: 420, pageViews: 1100 },
  { day: "Tue", visitors: 380, pageViews: 980 },
  { day: "Wed", visitors: 510, pageViews: 1340 },
  { day: "Thu", visitors: 470, pageViews: 1200 },
  { day: "Fri", visitors: 620, pageViews: 1580 },
  { day: "Sat", visitors: 290, pageViews: 740 },
  { day: "Sun", visitors: 340, pageViews: 890 },
];

const TOP_PAGES = [
  { page: "/home", views: 3200 },
  { page: "/pricing", views: 2100 },
  { page: "/docs", views: 1850 },
  { page: "/blog", views: 1400 },
  { page: "/about", views: 980 },
];

const TRAFFIC_SOURCES = [
  { name: "Organic", value: 45, color: "hsl(var(--primary))" },
  { name: "Direct", value: 25, color: "hsl(var(--status-answered))" },
  { name: "Referral", value: 18, color: "hsl(var(--status-noanswer))" },
  { name: "Social", value: 12, color: "hsl(var(--status-missed))" },
];

const RECENT_ACTIVITY = [
  { id: 1, user: "Alice Johnson", action: "Signed up", time: "2 min ago" },
  { id: 2, user: "Bob Smith", action: "Upgraded to Pro", time: "14 min ago" },
  { id: 3, user: "Carol White", action: "Submitted a support ticket", time: "32 min ago" },
  { id: 4, user: "David Lee", action: "Completed onboarding", time: "1 hr ago" },
  { id: 5, user: "Eva Martinez", action: "Exported report", time: "2 hrs ago" },
];

// ── Stat Card ────────────────────────────────────────────────────────────────

type Accent = "blue" | "green" | "amber" | "red";

const ACCENTS: Record<Accent, { border: string; bg: string; icon: string }> = {
  blue:  { border: "border-r-primary",            bg: "bg-primary/10",           icon: "text-primary" },
  green: { border: "border-r-status-answered",    bg: "bg-status-answered/10",   icon: "text-status-answered" },
  amber: { border: "border-r-status-noanswer",    bg: "bg-status-noanswer/10",   icon: "text-status-noanswer" },
  red:   { border: "border-r-status-missed",      bg: "bg-status-missed/10",     icon: "text-status-missed" },
};

function StatCard({
  label,
  value,
  change,
  icon: Icon,
  accent = "blue",
}: {
  label: string;
  value: string | number;
  change?: string;
  icon: React.ElementType;
  accent?: Accent;
}) {
  const a = ACCENTS[accent];
  return (
    <div className={cn("bg-card rounded-xl shadow-sm border border-app-border-strong border-r-4 p-4 flex items-center gap-4", a.border)}>
      <div className={cn("h-11 w-11 rounded-lg flex items-center justify-center", a.bg, a.icon)}>
        <Icon className="h-5 w-5" />
      </div>
      <div className="flex-1 min-w-0">
        <div className="text-[11px] text-muted-foreground">{label}</div>
        <div className="text-2xl font-bold tabular leading-tight">{value}</div>
        {change && <div className="text-[11px] text-muted-foreground mt-0.5">{change}</div>}
      </div>
    </div>
  );
}

// ── Dashboard Page ────────────────────────────────────────────────────────────

type Tab = "overview" | "traffic";

export default function Dashboard() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<Tab>("overview");

  const stats = useMemo(() => ([
    { label: t("dashboard.totalVisitors", "Total Visitors"), value: "3,030", change: "+12% from last week", icon: Users, accent: "blue" as Accent },
    { label: t("dashboard.pageViews", "Page Views"),        value: "7,830", change: "+8% from last week",  icon: TrendingUp, accent: "green" as Accent },
    { label: t("dashboard.avgSession", "Avg. Session"),     value: "4m 12s", change: "-3% from last week", icon: Clock, accent: "amber" as Accent },
    { label: t("dashboard.activeUsers", "Active Users"),    value: "142",    change: "Right now",          icon: Activity, accent: "red" as Accent },
  ]), [t]);

  return (
    <div className="flex-1 p-6 space-y-6">
      {/* Header */}
      <div className="space-y-3">
        <div className="flex items-center gap-3">
          <h2 className="text-xl font-bold">{t("dashboard.title", "Dashboard")}</h2>
          <span className="text-xs text-muted-foreground">{t("dashboard.subtitle", "Welcome back!")}</span>
        </div>

        {/* Tab nav */}
        <nav className="flex items-center gap-1 overflow-x-auto border-b border-border">
          {(["overview", "traffic"] as Tab[]).map((id) => (
            <button
              key={id}
              onClick={() => setTab(id)}
              className={cn(
                "relative px-4 py-3 text-sm whitespace-nowrap transition-colors capitalize",
                tab === id ? "text-primary font-medium" : "text-muted-foreground hover:text-foreground",
              )}
            >
              {id}
              {tab === id && <span className="absolute inset-x-3 -bottom-px h-0.5 bg-primary rounded-full" />}
            </button>
          ))}
        </nav>
      </div>

      {tab === "overview" && (
        <>
          {/* Stat cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {stats.map((s) => <StatCard key={s.label} {...s} />)}
          </div>

          {/* Charts row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            {/* Area chart */}
            <div className="bg-card rounded-xl border border-app-border-strong p-4">
              <div className="text-sm font-semibold mb-3">Weekly Visitors</div>
              <div className="h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart data={WEEKLY_VISITS}>
                    <defs>
                      <linearGradient id="visitors" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="hsl(var(--primary))" stopOpacity={0.2} />
                        <stop offset="95%" stopColor="hsl(var(--primary))" stopOpacity={0} />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--app-border-strong))" />
                    <XAxis dataKey="day" stroke="hsl(var(--muted-foreground))" fontSize={11} />
                    <YAxis stroke="hsl(var(--muted-foreground))" fontSize={11} />
                    <Tooltip />
                    <Area type="monotone" dataKey="visitors" stroke="hsl(var(--primary))" fill="url(#visitors)" strokeWidth={2} />
                    <Area type="monotone" dataKey="pageViews" stroke="hsl(var(--status-answered))" fill="none" strokeWidth={2} />
                  </AreaChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Pie chart */}
            <div className="bg-card rounded-xl border border-app-border-strong p-4">
              <div className="text-sm font-semibold mb-3">Traffic Sources</div>
              <div className="h-64">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie data={TRAFFIC_SOURCES} dataKey="value" innerRadius={50} outerRadius={80} paddingAngle={2}>
                      {TRAFFIC_SOURCES.map((d, i) => <Cell key={i} fill={d.color} />)}
                    </Pie>
                    <Tooltip />
                    <Legend wrapperStyle={{ fontSize: 11 }} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>
          </div>

          {/* Activity feed */}
          <div className="bg-card rounded-xl border border-app-border-strong p-4">
            <div className="text-sm font-semibold mb-3">Recent Activity</div>
            <ol className="space-y-3">
              {RECENT_ACTIVITY.map((a) => (
                <li key={a.id} className="flex items-start gap-3">
                  <div className="h-2 w-2 rounded-full bg-primary mt-2 shrink-0" />
                  <div className="flex-1 min-w-0">
                    <div className="text-xs">
                      <span className="font-medium">{a.user}</span> — {a.action}
                    </div>
                    <div className="text-[10px] text-muted-foreground">{a.time}</div>
                  </div>
                </li>
              ))}
            </ol>
          </div>
        </>
      )}

      {tab === "traffic" && (
        <div className="bg-card rounded-xl border border-app-border-strong p-4">
          <div className="text-sm font-semibold mb-3">Top Pages</div>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={TOP_PAGES} layout="vertical" margin={{ right: 12 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--app-border-strong))" />
                <XAxis type="number" stroke="hsl(var(--muted-foreground))" fontSize={11} />
                <YAxis type="category" dataKey="page" stroke="hsl(var(--muted-foreground))" fontSize={11} width={90} />
                <Tooltip />
                <Bar dataKey="views" fill="hsl(var(--primary))" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
    </div>
  );
}
