import { useTranslation } from "react-i18next";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { USER_ROLES } from "@/lib/apiClient";
import { ChangePasswordForm } from "./components/ChangePasswordForm";
import { ProfileNameForm } from "./components/ProfileNameForm";
import { useProfile } from "./useProfile";

function formatDateTime(value: string) {
  try {
    return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(new Date(value));
  } catch {
    return value;
  }
}

export default function ProfilePage() {
  const { t } = useTranslation();
  const { data: profile, isLoading, isError, refetch } = useProfile();

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }

  if (isError || !profile) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">{t("profile.loadError", "Failed to load profile.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>
          {t("common.retry", "Retry")}
        </Button>
      </div>
    );
  }

  const roleLabel =
    profile.role === USER_ROLES.SuperAdmin
      ? t("profile.roleSuperAdmin", "Super Admin")
      : t("profile.roleHr", "HR");

  return (
    <div className="space-y-6 max-w-2xl">
      <div>
        <h1 className="text-2xl font-semibold">{t("profile.title", "Profile")}</h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t("profile.subtitle", "Manage your account details and password.")}
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{t("profile.accountInfo", "Account information")}</CardTitle>
          <CardDescription>{t("profile.accountInfoHint", "Your login email and role are managed by the system.")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                {t("profile.email", "Email")}
              </p>
              <p className="text-sm mt-1">{profile.email}</p>
            </div>
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                {t("dashboard.role", "Role")}
              </p>
              <div className="mt-1">
                <Badge variant="secondary">{roleLabel}</Badge>
              </div>
            </div>
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
                {t("profile.memberSince", "Member since")}
              </p>
              <p className="text-sm mt-1">{formatDateTime(profile.createdAt)}</p>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t("profile.personalDetails", "Personal details")}</CardTitle>
        </CardHeader>
        <CardContent>
          <ProfileNameForm profile={profile} />
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t("profile.security", "Security")}</CardTitle>
          <CardDescription>{t("profile.securityHint", "Choose a strong password with at least 8 characters.")}</CardDescription>
        </CardHeader>
        <CardContent>
          <ChangePasswordForm />
        </CardContent>
      </Card>
    </div>
  );
}
