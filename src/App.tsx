import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider } from "@/hooks/useAuth";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { RoleRoute } from "@/components/RoleRoute";
import { AppShell } from "./layouts/AppShell";
import Auth from "./pages/Auth";
import Dashboard from "./pages/Dashboard";
import UsersAdmin from "./pages/UsersAdmin";
import PositionsPage from "./features/positions/PositionsPage";
import TemplateEditorPage from "./features/positions/TemplateEditorPage";
import NotFound from "./pages/NotFound";
import { POST_LOGIN_ROUTE } from "./config/appMode";
import { USER_ROLES } from "@/lib/apiClient";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/" element={<Navigate to={POST_LOGIN_ROUTE} replace />} />
            <Route path="/auth" element={<Auth />} />
            <Route element={<ProtectedRoute />}>
              <Route element={<AppShell />}>
                <Route path="/dashboard" element={<Dashboard />} />
                <Route path="/positions" element={<PositionsPage />} />
                <Route path="/positions/:positionId/template" element={<TemplateEditorPage />} />
                <Route element={<RoleRoute allowedRoles={[USER_ROLES.SuperAdmin]} />}>
                  <Route path="/users" element={<UsersAdmin />} />
                </Route>
              </Route>
            </Route>
            <Route path="*" element={<NotFound />} />
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
