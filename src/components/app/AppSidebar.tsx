import { NavLink, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { LayoutGrid, Users, Briefcase, ClipboardList } from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar";
import { SidebarAutoCollapse } from "./SidebarAutoCollapse";
import { useAuth } from "@/hooks/useAuth";

export function AppSidebar() {
  const { t, i18n } = useTranslation();
  const { pathname } = useLocation();
  const { isSuperAdmin } = useAuth();

  const items = [
    { to: "/dashboard", label: t("nav.dashboard", "Dashboard"), icon: LayoutGrid, end: true },
    { to: "/positions", label: t("nav.positions", "Positions"), icon: Briefcase, end: true },
    { to: "/exams", label: t("nav.exams", "Exams"), icon: ClipboardList, end: true },
    ...(isSuperAdmin
      ? [{ to: "/users", label: t("nav.users", "HR Accounts"), icon: Users, end: true as const }]
      : []),
  ];

  const isActive = (path: string, end?: boolean) =>
    end ? pathname === path : pathname === path || pathname.startsWith(path + "/");

  const side = i18n.language === "ar" ? "right" : "left";

  return (
    <>
      <SidebarAutoCollapse />
      <Sidebar collapsible="icon" side={side} className="top-14 h-[calc(100svh-3.5rem)]">
        <SidebarHeader className="border-b border-app-border-strong px-3 py-2">
          <span className="text-xs font-semibold uppercase tracking-wide text-muted-foreground group-data-[collapsible=icon]:hidden">
            HireExam
          </span>
        </SidebarHeader>
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupContent>
              <SidebarMenu>
                {items.map((item) => {
                  const active = isActive(item.to, item.end);
                  return (
                    <SidebarMenuItem key={item.to}>
                      <SidebarMenuButton asChild isActive={active} tooltip={item.label}>
                        <NavLink to={item.to} end={item.end}>
                          <item.icon className="h-4 w-4 shrink-0" />
                          <span>{item.label}</span>
                        </NavLink>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  );
                })}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
      </Sidebar>
    </>
  );
}
