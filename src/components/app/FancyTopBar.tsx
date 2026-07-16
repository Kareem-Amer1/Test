import { useTranslation } from "react-i18next";
import { Bell, Search } from "lucide-react";
import { UserMenu } from "./UserMenu";
import { LanguageSwitcher } from "./LanguageSwitcher";
import { ThemeStudio } from "./ThemeStudio";
import { CopilotNavbarChatButton } from "@/copilot/shared/CopilotNavbarChatButton";
import { useCopilotUiContext } from "@/components/copilot/CopilotProvider";

export function FancyTopBar() {
  const { t } = useTranslation();
  const { isCopilotOpen, copilotAvailable, toggleCopilot } = useCopilotUiContext();
  return (
    <header className="fancy-topbar">
      <div className="fancy-search">
        <Search className="h-4 w-4 opacity-60" />
        <input
          type="text"
          placeholder={t("fancy.search", "Search…")}
          className="fancy-search-input"
        />
        <kbd className="fancy-kbd">⌘K</kbd>
      </div>

      <div className="fancy-topbar-spacer" />

      <button type="button" className="fancy-icon-btn fancy-notif" aria-label="notifications">
        <Bell className="h-4 w-4" />
        <span className="fancy-notif-dot" aria-hidden />
      </button>

      <div className="fancy-icon-btn"><ThemeStudio /></div>
      <div className="fancy-icon-btn"><LanguageSwitcher /></div>
      <div className="fancy-icon-btn">
        <CopilotNavbarChatButton open={isCopilotOpen} available={copilotAvailable} onToggle={toggleCopilot} />
      </div>
      <div className="fancy-userchip"><UserMenu /></div>
    </header>
  );
}
