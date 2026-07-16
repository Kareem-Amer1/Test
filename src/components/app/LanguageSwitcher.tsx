import { Globe } from "lucide-react";
import { useTranslation } from "react-i18next";

export function LanguageSwitcher() {
  const { i18n, t } = useTranslation();

  const toggle = () => {
    const next = i18n.language === "ar" ? "en" : "ar";
    i18n.changeLanguage(next);
  };

  return (
    <button
      type="button"
      onClick={toggle}
      className="inline-flex items-center gap-1.5 px-2 h-9 text-sm font-medium text-foreground hover:text-primary transition-colors"
      aria-label={t("language.switchTo")}
    >
      <span>{t("language.switchTo")}</span>
      <Globe className="h-4 w-4" />
    </button>
  );
}
