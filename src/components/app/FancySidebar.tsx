import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import {
  ChevronLeft,
  ChevronRight,
  LayoutGrid,
  Users,
  Briefcase,
  ClipboardList,
  type LucideIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Logo } from "./Logo";
import { useAuth } from "@/hooks/useAuth";

type Item = { to: string; label: string; icon: LucideIcon; end?: boolean; badge?: string };
type Group = { label: string; items: Item[] };

export function FancySidebar() {
  const { t } = useTranslation();
  const { pathname } = useLocation();
  const [collapsed, setCollapsed] = useState(false);
  const { isSuperAdmin } = useAuth();

  const workspace: Item[] = [
    { to: "/dashboard", label: t("nav.dashboard", "Dashboard"), icon: LayoutGrid, end: true },
    { to: "/positions", label: t("nav.positions", "Positions"), icon: Briefcase, end: true },
    { to: "/exams", label: t("nav.exams", "Exams"), icon: ClipboardList, end: true },
  ];
  const manage: Item[] = isSuperAdmin
    ? [{ to: "/users", label: t("nav.users", "HR Accounts"), icon: Users, end: true }]
    : [];

  const groups: Group[] = [
    { label: t("fancy.sections.workspace", "Workspace"), items: workspace },
    ...(manage.length ? [{ label: t("fancy.sections.manage", "Manage"), items: manage }] : []),
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
        {groups.map((group) => (
          <div key={group.label} className="fancy-nav-group">
            {!collapsed && <div className="fancy-nav-label">{group.label}</div>}
            {group.items.map((item) => {
              const active = isActive(item.to, item.end);
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.end}
                  className={cn("fancy-nav-item", active && "fancy-nav-item--active")}
                  title={collapsed ? item.label : undefined}
                >
                  <item.icon className="h-4 w-4 shrink-0" />
                  {!collapsed && <span>{item.label}</span>}
                  {!collapsed && item.badge && (
                    <span className="ms-auto text-[10px] font-medium opacity-70">{item.badge}</span>
                  )}
                </NavLink>
              );
            })}
          </div>
        ))}
      </nav>

      <button type="button" className="fancy-collapse-btn" onClick={toggle} aria-label="Toggle sidebar">
        {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
      </button>
    </aside>
  );
}
