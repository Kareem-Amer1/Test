import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { clearTokens, decodeToken, getAccessToken, logoutRequest } from "@/lib/apiClient";

export interface AuthUser {
  id: string;
  email: string;
  role?: string;
}

interface AuthContextValue {
  user: AuthUser | null;
  loading: boolean;
  signOut: () => Promise<void>;
  refreshUser: () => void;
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  loading: true,
  signOut: async () => {},
  refreshUser: () => {},
});

const AUTH_EVENT = "vf-auth-changed";

export function notifyAuthChanged() {
  window.dispatchEvent(new Event(AUTH_EVENT));
}

function readUser(): AuthUser | null {
  if (!getAccessToken()) return null;
  const p = decodeToken();
  if (!p?.sub) return null;
  const role = Array.isArray(p.role) ? p.role[0] : p.role;
  return { id: p.sub, email: p.email ?? "", role };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshUser = useCallback(() => {
    setUser(readUser());
  }, []);

  useEffect(() => {
    setUser(readUser());
    setLoading(false);
    const onChange = () => setUser(readUser());
    window.addEventListener(AUTH_EVENT, onChange);
    window.addEventListener("storage", onChange);
    return () => {
      window.removeEventListener(AUTH_EVENT, onChange);
      window.removeEventListener("storage", onChange);
    };
  }, []);

  const signOut = useCallback(async () => {
    await logoutRequest();
    clearTokens();
    setUser(null);
    notifyAuthChanged();
  }, []);

  return (
    <AuthContext.Provider value={{ user, loading, signOut, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
