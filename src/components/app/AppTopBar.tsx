import { SidebarTrigger } from "@/components/ui/sidebar";
import { Logo } from "./Logo";
import { UserMenu } from "./UserMenu";
import { LanguageSwitcher } from "./LanguageSwitcher";
import { ThemeStudio } from "./ThemeStudio";
import { FocusModeToggle } from "./FocusModeToggle";
import { CopilotNavbarChatButton } from "@/copilot/shared/CopilotNavbarChatButton";
import { useCopilotUiContext } from "@/components/copilot/CopilotProvider";

export function AppTopBar({ hideSidebarTrigger = false }: { hideSidebarTrigger?: boolean }) {
  const { isCopilotOpen, copilotAvailable, toggleCopilot } = useCopilotUiContext();
  return (
    <header className="h-14 shrink-0 bg-card border-b border-app-border-strong flex items-center px-4 gap-3 sticky top-0 z-40">
      {!hideSidebarTrigger && <SidebarTrigger />}
      <Logo />
      <div className="flex-1" />
      <div className="flex items-center gap-2 shrink-0">
        <UserMenu />
        <ThemeStudio />
        <FocusModeToggle />
        <LanguageSwitcher />
        <CopilotNavbarChatButton
          open={isCopilotOpen}
          available={copilotAvailable}
          onToggle={toggleCopilot}
        />
      </div>
    </header>
  );
}
