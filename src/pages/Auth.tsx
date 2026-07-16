import { useEffect, useState } from "react";
import { useNavigate, Navigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth, notifyAuthChanged } from "@/hooks/useAuth";
import { api, setTokens } from "@/lib/apiClient";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { toast } from "sonner";
import { Logo } from "@/components/app/Logo";
import { Eye, EyeOff } from "lucide-react";
import { POST_LOGIN_ROUTE } from "@/config/appMode";

interface AuthTokensResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export default function Auth() {
  const { t, i18n } = useTranslation();
  const { user, loading } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [mode, setMode] = useState<"signin" | "signup">("signin");

  useEffect(() => {
    if (!loading && user) navigate(POST_LOGIN_ROUTE, { replace: true });
  }, [user, loading, navigate]);

  if (loading) return null;
  if (user) return <Navigate to={POST_LOGIN_ROUTE} replace />;

  const dir = i18n.language === "ar" ? "rtl" : "ltr";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const em = email.trim();
    if (!em || !password) {
      toast.error(t("auth.missingFields", "Please fill in all fields."));
      return;
    }
    setSubmitting(true);
    try {
      const path = mode === "signup" ? "/auth/register" : "/auth/login";
      const tokens = await api.post<AuthTokensResponse>(path, { email: em, password });
      setTokens(tokens.accessToken, tokens.refreshToken);
      notifyAuthChanged();
      toast.success(
        mode === "signup"
          ? t("auth.signupSuccess", "Account created.")
          : t("auth.loginSuccess", "Signed in successfully."),
      );
      navigate(POST_LOGIN_ROUTE, { replace: true });
    } catch (err: unknown) {
      toast.error(err instanceof Error ? err.message : String(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div dir={dir} className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-6">
        <div className="flex justify-center"><Logo /></div>
        <div className="bg-card border border-app-border-strong rounded-xl p-6 shadow-sm">
          <Tabs value={mode} onValueChange={(v) => setMode(v as "signin" | "signup")} className="mb-4">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="signin">{t("auth.loginButton", "Sign In")}</TabsTrigger>
              <TabsTrigger value="signup">{t("auth.signupButton", "Sign Up")}</TabsTrigger>
            </TabsList>
            <TabsContent value="signin" />
            <TabsContent value="signup" />
          </Tabs>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="email">{t("auth.email", "Email")}</Label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                autoComplete="email"
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="password">{t("auth.password", "Password")}</Label>
              <div className="relative">
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  minLength={6}
                  autoComplete={mode === "signup" ? "new-password" : "current-password"}
                  className="pe-10"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((v) => !v)}
                  className="absolute inset-y-0 end-0 flex items-center px-3 text-muted-foreground hover:text-foreground"
                  aria-label={showPassword ? "Hide password" : "Show password"}
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </div>
            <Button type="submit" disabled={submitting} className="w-full">
              {submitting ? "…" : mode === "signup"
                ? t("auth.signupButton", "Sign Up")
                : t("auth.loginButton", "Sign In")}
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}
