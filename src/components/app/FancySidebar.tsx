import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  BarChart3,
  ChevronLeft,
  ChevronRight,
  LayoutGrid,
  Settings,
  Users,
  type LucideIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Logo } from "./Logo";
import { useProjects } from "@/contexts/ProjectContext";

type Item = { to: string; label: string; icon: LucideIcon; end?: boolean; badge?: string };
type Group = { label: string; items: Item[] };

export function FancySidebar() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const [collapsed, setCollapsed] = useState(false);
  const { activeProject } = useProjects();
  const base = activeProject ? `/project/${activeProject.id}` : "";

  const workspace: Item[] = [
    { to: `${base}/dashboard`, label: t("nav.dashboard", "Dashboard"), icon: LayoutGrid, end: true },
    { to: `${base}/analytics`, label: t("nav.analytics", "Analytics"), icon: BarChart3 },
  ];
  const manage: Item[] = [
    { to: `${base}/users`, label: t("nav.users", "Users"), icon: Users },
    { to: `${base}/settings`, label: t("nav.settings", "Settings"), icon: Settings },
  ];

  const groups: Group[] = [
    { label: t("fancy.sections.workspace", "Workspace"), items: workspace },
    { label: t("fancy.sections.manage", "Manage"), items: manage },
  ];

  const isActive = (path: string, end?: boolean) =>
    end ? pathname === path : pathname === path || pathname.startsWith(path + "/");

  const toggle = () => setCollapsed((c) => !c);

  return (
    <aside className="fancy-sidebar" data-collapsed={collapsed || undefined}>
      <div className="fancy-brand">
        <Logo />
      </div>

      <nav className="fancy-nav">
        {groups.map((g) =>
          g.items.length ? (
            <div key={g.label} className="fancy-nav-group">
              <div className="fancy-nav-label">{g.label}</div>
              <ul>
                {g.items.map((it) => {
                  const active = isActive(it.to, it.end);
                  return (
                    <li key={it.to}>
                      <NavLink
                        to={it.to}
                        end={it.end}
                        className={cn("fancy-nav-item", active && "is-active")}
                      >
                        {active && <span className="fancy-nav-bar" aria-hidden />}
                        <it.icon className="fancy-nav-icon" />
                        <span className="fancy-nav-text">{it.label}</span>
                        {it.badge && <span className="fancy-nav-badge">{it.badge}</span>}
                      </NavLink>
                    </li>
                  );
                })}
              </ul>
            </div>
          ) : null
        )}
      </nav>

      <div className="fancy-sidebar-footer">
        <button
          type="button"
          onClick={toggle}
          className="fancy-toggle-btn"
          aria-label={collapsed ? t("sidebar.expand", "Expand") : t("sidebar.collapse", "Collapse")}
        >
          {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
          <span className="fancy-toggle-label">{collapsed ? t("sidebar.expand", "Expand") : t("sidebar.collapse", "Collapse")}</span>
        </button>
      </div>
    </aside>
  );
}
