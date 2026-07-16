import { Link, NavLink, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeftRight, BarChart3, LayoutGrid, Settings, Users } from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar";
import { SidebarAutoCollapse } from "./SidebarAutoCollapse";
import { projectInitials, useProjects } from "@/contexts/ProjectContext";

export function AppSidebar() {
  const { t, i18n } = useTranslation();
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

  const side = i18n.language === "ar" ? "right" : "left";

  return (
    <>
      <SidebarAutoCollapse />
      <Sidebar collapsible="icon" side={side} className="top-14 h-[calc(100svh-3.5rem)]">
        {activeProject && (
          <SidebarHeader className="border-b border-app-border-strong">
            <Link
              to="/projects"
              className="flex items-center gap-2 rounded-md p-2 hover:bg-accent transition-colors group/proj"
              title={t("projects.backToProjects", "Back to projects")}
            >
              <div
                className="h-8 w-8 rounded-md flex items-center justify-center text-white text-xs font-bold shrink-0"
                style={{ backgroundColor: activeProject.color }}
              >
                {projectInitials(activeProject.name)}
              </div>
              <div className="flex-1 min-w-0 group-data-[collapsible=icon]:hidden">
                <div className="text-sm font-semibold truncate">{activeProject.name}</div>
                <div className="text-[10px] text-muted-foreground truncate">
                  {t("projects.backToProjects", "Back to projects")}
                </div>
              </div>
            </Link>
          </SidebarHeader>
        )}
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
        <SidebarFooter className="border-t border-app-border-strong">
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild tooltip={t("projects.switch", "Switch project")}>
                <Link to="/projects">
                  <ArrowLeftRight className="h-4 w-4 shrink-0" />
                  <span>{t("projects.switch", "Switch project")}</span>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarFooter>
      </Sidebar>
    </>
  );
}
