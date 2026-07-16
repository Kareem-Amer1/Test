import { Sun, Moon } from "lucide-react";
import { useTheme } from "@/layouts/template/useTheme";
import { useTranslation } from "react-i18next";

export function ThemeToggle() {
  const { theme, toggle } = useTheme();
  const { t } = useTranslation();
  const isDark = theme === "dark";
  return (
    <button
      type="button"
      onClick={toggle}
      aria-label={t(isDark ? "theme.switchToLight" : "theme.switchToDark", isDark ? "Switch to light" : "Switch to dark")}
      className="inline-flex items-center justify-center w-9 h-9 rounded-md text-foreground hover:text-primary transition-colors"
    >
      {isDark ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
    </button>
  );
}
