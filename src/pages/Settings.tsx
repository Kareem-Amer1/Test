import { useTranslation } from "react-i18next";
import { Bell, Lock, Palette, User } from "lucide-react";

const SECTIONS = [
  { icon: User,    title: "Profile",        description: "Manage your name, email, and avatar." },
  { icon: Lock,    title: "Security",       description: "Password, two-factor authentication, and sessions." },
  { icon: Bell,    title: "Notifications",  description: "Choose what you get notified about." },
  { icon: Palette, title: "Appearance",     description: "Customize the theme, language, and layout." },
];

export default function Settings() {
  const { t } = useTranslation();

  return (
    <div className="flex-1 p-6 space-y-6 max-w-3xl">
      <div>
        <h2 className="text-xl font-bold">{t("nav.settings", "Settings")}</h2>
        <p className="text-sm text-muted-foreground mt-1">Manage your account and preferences.</p>
      </div>

      <div className="space-y-3">
        {SECTIONS.map((s) => (
          <button
            key={s.title}
            className="w-full bg-card rounded-xl border border-app-border-strong p-4 flex items-center gap-4 text-start hover:border-primary/40 transition-colors"
          >
            <div className="h-10 w-10 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
              <s.icon className="h-5 w-5" />
            </div>
            <div className="flex-1 min-w-0">
              <div className="text-sm font-medium">{s.title}</div>
              <div className="text-xs text-muted-foreground">{s.description}</div>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
