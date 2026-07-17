import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createBrowserRouter, Navigate, RouterProvider } from "react-router-dom";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { AuthProvider } from "@/hooks/useAuth";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { RoleRoute } from "@/components/RoleRoute";
import { AppShell } from "./layouts/AppShell";
import { PageFrame } from "./layouts/PageFrame";
import Auth from "./pages/Auth";
import Dashboard from "./pages/Dashboard";
import UsersAdmin from "./pages/UsersAdmin";
import PositionsPage from "./features/positions/PositionsPage";
import TemplateEditorPage from "./features/positions/TemplateEditorPage";
import ExamSessionPage from "./features/exams/ExamSessionPage";
import ExamDetailPage from "./features/exams/ExamDetailPage";
import ExamsPage from "./features/exams/ExamsPage";
import ProfilePage from "./features/profile/ProfilePage";
import NotFound from "./pages/NotFound";
import { POST_LOGIN_ROUTE } from "./config/appMode";
import { USER_ROLES } from "@/lib/apiClient";

const queryClient = new QueryClient();

const router = createBrowserRouter([
  { path: "/", element: <Navigate to={POST_LOGIN_ROUTE} replace /> },
  { path: "/auth", element: <Auth /> },
  {
    element: <ProtectedRoute />,
    children: [
      { path: "/exams/:examId/session", element: <ExamSessionPage /> },
      {
        element: <AppShell />,
        children: [
          {
            element: <PageFrame />,
            children: [
              { path: "/dashboard", element: <Dashboard /> },
              { path: "/positions", element: <PositionsPage /> },
              { path: "/positions/:positionId/template", element: <TemplateEditorPage /> },
              { path: "/exams", element: <ExamsPage /> },
              { path: "/exams/:examId", element: <ExamDetailPage /> },
              { path: "/profile", element: <ProfilePage /> },
              {
                element: <RoleRoute allowedRoles={[USER_ROLES.SuperAdmin]} />,
                children: [{ path: "/users", element: <UsersAdmin /> }],
              },
            ],
          },
        ],
      },
    ],
  },
  { path: "*", element: <NotFound /> },
]);

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
