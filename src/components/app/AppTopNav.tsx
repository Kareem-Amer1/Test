import { NavLink, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { BarChart3, LayoutGrid, Settings, Users } from "lucide-react";
import { useProjects } from "@/contexts/ProjectContext";

export function AppTopNav() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const { activeProject } = useProjects();
  const base = activeProject ? `/project/${activeProject.id}` : "";

  const items = [
    { to: `${base}/dashboard`, label: t("nav.dashboard", "Dashboard"), icon: LayoutGrid, end: true },
    { to: `${base}/analytics`, label: t("nav.analytics", "Analytics"), icon: BarChart3 },
    { to: `${base}/users`, label: t("nav.users", "Users"), icon: Users },
    { to: `${base}/settings`, label: t("nav.settings", "Settings"), icon: Settings },
  ];

  const isActive = (path: string, end?: boolean) =>
    end ? pathname === path : pathname === path || pathname.startsWith(path + "/");

  return (
    <nav className="flex items-center justify-center gap-0.5 overflow-x-auto min-w-0 flex-1">
      {items.map((it) => {
        const active = isActive(it.to, it.end);
        return (
          <NavLink
            key={it.to}
            to={it.to}
            end={it.end}
            className={[
              "inline-flex items-center justify-center px-2 h-9 rounded-md whitespace-nowrap transition-colors",
              active
                ? "bg-muted text-foreground"
                : "text-muted-foreground hover:text-foreground hover:bg-muted/50",
            ].join(" ")}
          >
            <it.icon className="h-4 w-4 me-1.5 shrink-0" />
            {it.label}
          </NavLink>
        );
      })}
    </nav>
  );
}
