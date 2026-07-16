import { useAgentContext, useFrontendTool } from "@copilotkit/react-core/v2";
import { useNavigate, useParams } from "react-router-dom";
import { z } from "zod";
import { useAuth } from "@/hooks/useAuth";
import { useCopilotUiContext } from "./CopilotProvider";

const PROJECT_TABS = ["dashboard", "analytics", "users", "settings"];

export function CopilotAppContext() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const { isCopilotOpen: _open } = useCopilotUiContext();
  const { id: projectId } = useParams<{ id: string }>();

  useAgentContext({
    description: "Currently signed-in user (id, email).",
    value: { id: user.id, email: user.email },
  });

  useFrontendTool({
    name: "navigateTo",
    description:
      "Navigate the user to an internal app route. " +
      "For project tabs use the tab name directly: 'dashboard', 'analytics', 'users', or 'settings'. " +
      "You can also pass full paths like '/projects' or '/project/<id>/dashboard'.",
    parameters: z.object({
      path: z.string().describe(
        "A tab name ('dashboard', 'analytics', 'users', 'settings') or an absolute path starting with '/'."
      ),
    }),
    handler: async ({ path }) => {
      // Resolve bare tab names to the current project-scoped route
      const tab = path.replace(/^\//, "").toLowerCase();
      if (PROJECT_TABS.includes(tab) && projectId) {
        const resolved = `/project/${projectId}/${tab}`;
        navigate(resolved);
        return { ok: true, navigatedTo: resolved };
      }

      if (!path?.startsWith("/")) return { ok: false, error: "Path must start with /" };
      navigate(path);
      return { ok: true, navigatedTo: path };
    },
  });

  return null;
}
