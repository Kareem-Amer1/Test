import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider } from "@/hooks/useAuth";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { ProjectProvider } from "@/contexts/ProjectContext";
import { ProjectResolver } from "@/components/ProjectResolver";
import { AppShell } from "./layouts/AppShell";
import Auth from "./pages/Auth";
import Dashboard from "./pages/Dashboard";
import Analytics from "./pages/Analytics";
import Settings from "./pages/Settings";
import Projects from "./pages/Projects";
import NotFound from "./pages/NotFound";
import { PROJECT_CONTROLLER_MODE, POST_LOGIN_ROUTE } from "./config/appMode";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <BrowserRouter>
        <AuthProvider>
          <ProjectProvider>
            <Routes>
              <Route path="/" element={<Navigate to={POST_LOGIN_ROUTE} replace />} />
              <Route path="/auth" element={<Auth />} />
              <Route element={<ProtectedRoute />}>
                {PROJECT_CONTROLLER_MODE && (
                  <>
                    <Route path="/projects" element={<Projects />} />
                    <Route path="/project/:id" element={<ProjectResolver />}>
                      <Route element={<AppShell />}>
                        <Route index element={<Navigate to="dashboard" replace />} />
                        <Route path="dashboard" element={<Dashboard />} />
                        <Route path="analytics" element={<Analytics />} />
                        <Route path="users" element={<Settings />} />
                        <Route path="settings" element={<Settings />} />
                      </Route>
                    </Route>
                  </>
                )}
                <Route element={<AppShell />}>
                  <Route path="/dashboard" element={<Dashboard />} />
                  <Route path="/analytics" element={<Analytics />} />
                  <Route path="/users" element={<Settings />} />
                  <Route path="/settings" element={<Settings />} />
                </Route>
              </Route>
              <Route path="*" element={<NotFound />} />
            </Routes>
          </ProjectProvider>
        </AuthProvider>
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
