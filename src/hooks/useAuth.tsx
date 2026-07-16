import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { api, clearTokens, decodeToken, getAccessToken, logoutRequest, USER_ROLES, type UserRole } from "@/lib/apiClient";

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
}

interface AuthContextValue {
  user: AuthUser | null;
  loading: boolean;
  signOut: () => Promise<void>;
  refreshUser: () => Promise<void>;
  isSuperAdmin: boolean;
  isHR: boolean;
  hasRole: (role: UserRole | UserRole[]) => boolean;
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  loading: true,
  signOut: async () => {},
  refreshUser: async () => {},
  isSuperAdmin: false,
  isHR: false,
  hasRole: () => false,
});

const AUTH_EVENT = "he-auth-changed";

export function notifyAuthChanged() {
  window.dispatchEvent(new Event(AUTH_EVENT));
}

function userFromToken(): AuthUser | null {
  const p = decodeToken();
  if (!p?.sub) return null;
  const roleRaw = Array.isArray(p.role) ? p.role[0] : p.role;
  const role = roleRaw === USER_ROLES.SuperAdmin || roleRaw === USER_ROLES.HR
    ? roleRaw
    : USER_ROLES.HR;
  return {
    id: p.sub,
    email: p.email ?? "",
    fullName: p.fullName ?? "",
    role,
  };
}

async function fetchMe(): Promise<AuthUser | null> {
  if (!getAccessToken()) return null;
  try {
    const me = await api.get<{ id: string; email: string; fullName: string; role: string }>("/auth/me");
    const role = me.role === USER_ROLES.SuperAdmin ? USER_ROLES.SuperAdmin : USER_ROLES.HR;
    return { id: me.id, email: me.email, fullName: me.fullName, role };
  } catch {
    return userFromToken();
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    setUser(await fetchMe());
  }, []);

  useEffect(() => {
    let active = true;
    (async () => {
      const next = await fetchMe();
      if (active) {
        setUser(next);
        setLoading(false);
      }
    })();
    const onChange = () => {
      void refreshUser();
    };
    window.addEventListener(AUTH_EVENT, onChange);
    window.addEventListener("storage", onChange);
    return () => {
      active = false;
      window.removeEventListener(AUTH_EVENT, onChange);
      window.removeEventListener("storage", onChange);
    };
  }, [refreshUser]);

  const signOut = useCallback(async () => {
    await logoutRequest();
    clearTokens();
    setUser(null);
    notifyAuthChanged();
  }, []);

  const hasRole = useCallback(
    (role: UserRole | UserRole[]) => {
      if (!user) return false;
      const allowed = Array.isArray(role) ? role : [role];
      return allowed.includes(user.role);
    },
    [user],
  );

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        signOut,
        refreshUser,
        isSuperAdmin: user?.role === USER_ROLES.SuperAdmin,
        isHR: user?.role === USER_ROLES.HR,
        hasRole,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
