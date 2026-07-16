import { Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AppTopBar } from "@/components/app/AppTopBar";
import { AppSidebar } from "@/components/app/AppSidebar";
import { SidebarProvider, SidebarInset } from "@/components/ui/sidebar";
import { useLayoutTemplate } from "@/layouts/template/useLayoutTemplate";
import { AppTopNav } from "@/components/app/AppTopNav";
import { FancySidebar } from "@/components/app/FancySidebar";
import { FancyTopBar } from "@/components/app/FancyTopBar";
import { X } from "lucide-react";
import { useFocusMode } from "@/hooks/useFocusMode";
import { CopilotProvider } from "@/components/copilot/CopilotProvider";
import { CopilotAppContext } from "@/components/copilot/CopilotAppContext";

function FocusExitButton() {
  const { setFocus } = useFocusMode();
  const { t } = useTranslation();
  return (
    <button
      type="button"
      onClick={() => setFocus(false)}
      aria-label={t("focusMode.exit", "Exit focus mode")}
      className="fixed top-4 end-4 z-50 h-9 w-9 inline-flex items-center justify-center rounded-full border border-app-border-strong bg-card/80 backdrop-blur text-muted-foreground hover:text-foreground hover:border-foreground/40 transition-colors"
    >
      <X className="h-4 w-4" />
    </button>
  );
}

export function AppShell() {
  return (
    <CopilotProvider>
      <CopilotAppContext />
      <AppShellInner />
    </CopilotProvider>
  );
}

function AppShellInner() {
  const { i18n } = useTranslation();
  const dir = i18n.language === "ar" ? "rtl" : "ltr";
  const { template } = useLayoutTemplate();
  const { focus, setFocus } = useFocusMode();

  if (focus) {
    return (
      <div data-theme="dashboard" dir={dir} className="h-screen bg-muted/30 overflow-hidden flex flex-col">
        <FocusExitButton />
        <main className="flex-1 min-h-0 overflow-auto">
          <div className="mx-auto max-w-6xl px-8 py-10">
            <Outlet />
          </div>
        </main>
      </div>
    );
  }

  if (template === "topnav") {
    return (
      <div data-theme="dashboard" dir={dir} className="h-screen bg-background overflow-hidden flex flex-col">
        <AppTopBar hideSidebarTrigger />
        <div className="h-12 shrink-0 bg-card border-b border-app-border-strong px-4 flex items-center overflow-x-auto">
          <AppTopNav />
        </div>
        <main className="flex-1 min-h-0 flex flex-col overflow-auto">
          <Outlet />
        </main>
      </div>
    );
  }

  if (template === "bento") {
    return (
      <div data-theme="dashboard" dir={dir} className="h-screen bg-muted/40 overflow-hidden">
        <SidebarProvider className="flex-col h-full min-h-0">
          <AppTopBar />
          <div className="flex flex-1 min-h-0 w-full overflow-hidden gap-3 p-3">
            <AppSidebar />
            <SidebarInset className="flex-1 min-w-0 min-h-0 flex flex-col overflow-hidden rounded-2xl border border-app-border-strong shadow-lg bg-card">
              <main key={dir} className="flex-1 min-h-0 flex flex-col overflow-auto animate-fade-in">
                <Outlet />
              </main>
            </SidebarInset>
          </div>
        </SidebarProvider>
      </div>
    );
  }

  if (template === "split") {
    return (
      <div data-theme="dashboard" dir={dir} className="h-screen bg-background overflow-hidden">
        <SidebarProvider className="flex-col h-full min-h-0">
          <AppTopBar />
          <div className="flex flex-1 min-h-0 w-full overflow-hidden">
            <AppSidebar />
            <SidebarInset className="flex-1 min-w-0 min-h-0 flex flex-col overflow-hidden">
              <main className="flex-1 min-h-0 flex flex-col overflow-auto">
                <Outlet />
              </main>
            </SidebarInset>
            <aside className="hidden lg:flex w-64 shrink-0 border-l border-app-border-strong bg-card/60 flex-col p-4 text-xs text-muted-foreground">
              <div className="font-semibold uppercase tracking-wider text-[10px] mb-2 text-foreground/70">Activity</div>
              <div className="opacity-70">Secondary panel — reserved for context, notifications, or quick actions.</div>
            </aside>
          </div>
        </SidebarProvider>
      </div>
    );
  }

  if (template === "brutalist") {
    return (
      <div data-theme="dashboard" dir={dir} className="h-screen bg-muted/60 overflow-hidden">
        <SidebarProvider className="flex-col h-full min-h-0">
          <AppTopBar />
          <div className="flex flex-1 min-h-0 w-full overflow-hidden gap-4 p-4">
            <AppSidebar />
            <SidebarInset className="flex-1 min-w-0 min-h-0 flex flex-col overflow-hidden bg-card border-2 border-foreground shadow-[4px_4px_0_0_hsl(var(--foreground))] rounded-none">
              <main className="flex-1 min-h-0 flex flex-col overflow-auto">
                <Outlet />
              </main>
            </SidebarInset>
          </div>
        </SidebarProvider>
      </div>
    );
  }

  if (template === "fancy") {
    return (
      <div data-theme="dashboard" dir={dir} className="fancy-shell" data-layout-variant="fancy">
        <FancySidebar />
        <div className="fancy-main-col">
          <FancyTopBar />
          <main className="fancy-main">
            <Outlet />
          </main>
        </div>
      </div>
    );
  }

  return (
    <div data-theme="dashboard" dir={dir} className="h-screen bg-background overflow-hidden">
      <SidebarProvider className="flex-col h-full min-h-0">
        <AppTopBar />
        <div className="flex flex-1 min-h-0 w-full overflow-hidden">
          <AppSidebar />
          <SidebarInset className="flex-1 min-w-0 min-h-0 flex flex-col overflow-hidden">
            <main className="flex-1 min-h-0 flex flex-col overflow-auto">
              <Outlet />
            </main>
          </SidebarInset>
        </div>
      </SidebarProvider>
    </div>
  );
}
