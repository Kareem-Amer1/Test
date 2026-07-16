import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { useAuth } from "@/hooks/useAuth";
import { toast } from "sonner";
import { PROJECT_CONTROLLER_MODE } from "@/config/appMode";
import { api } from "@/lib/apiClient";

interface ProjectApiResponse {
  id: string;
  name: string;
  description: string;
  color: string;
  createdAt: string;
  updatedAt: string;
}

function fromApi(p: ProjectApiResponse): Project {
  return {
    id: p.id,
    name: p.name,
    description: p.description ?? "",
    color: p.color,
    createdAt: p.createdAt,
  };
}

export interface Project {
  id: string;
  name: string;
  description: string;
  color: string;
  createdAt: string;
}

interface ProjectContextValue {
  projects: Project[];
  loading: boolean;
  activeProject: Project | null;
  setActiveProjectId: (id: string | null) => void;
  createProject: (input: { name: string; description: string; color: string }) => Promise<Project | null>;
  deleteProject: (id: string) => Promise<void>;
}

export const PROJECT_COLORS = [
  "#3b82f6",
  "#10b981",
  "#f59e0b",
  "#ef4444",
  "#8b5cf6",
  "#ec4899",
  "#06b6d4",
  "#64748b",
];

const ProjectContext = createContext<ProjectContextValue | null>(null);

export function ProjectProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(false);
  const [activeId, setActiveId] = useState<string | null>(null);

  useEffect(() => {
    if (!user || !PROJECT_CONTROLLER_MODE) {
      setProjects([]);
      return;
    }
    let cancelled = false;
    setLoading(true);
    api
      .get<ProjectApiResponse[]>("/projects")
      .then((data) => {
        if (cancelled) return;
        setProjects((data ?? []).map(fromApi));
      })
      .catch((err: unknown) => {
        if (!cancelled) toast.error(err instanceof Error ? err.message : String(err));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [user]);

  const createProject: ProjectContextValue["createProject"] = useCallback(
    async (input) => {
      if (!user) return null;
      try {
        const data = await api.post<ProjectApiResponse>("/projects", {
          name: input.name.trim(),
          description: input.description.trim(),
          color: input.color,
        });
        const project = fromApi(data);
        setProjects((prev) => [project, ...prev]);
        return project;
      } catch (err) {
        toast.error(err instanceof Error ? err.message : "Failed to create project");
        return null;
      }
    },
    [user],
  );

  const deleteProject = useCallback(async (id: string) => {
    try {
      await api.deleteRaw(`/projects/${id}`);
      setProjects((prev) => prev.filter((p) => p.id !== id));
      setActiveId((curr) => (curr === id ? null : curr));
    } catch (err) {
      toast.error(err instanceof Error ? err.message : "Failed to delete project");
    }
  }, []);

  const activeProject = useMemo(
    () => projects.find((p) => p.id === activeId) ?? null,
    [projects, activeId],
  );

  const value = useMemo<ProjectContextValue>(
    () => ({
      projects,
      loading,
      activeProject,
      setActiveProjectId: setActiveId,
      createProject,
      deleteProject,
    }),
    [projects, loading, activeProject, createProject, deleteProject],
  );

  return <ProjectContext.Provider value={value}>{children}</ProjectContext.Provider>;
}

export function useProjects() {
  const ctx = useContext(ProjectContext);
  if (!ctx) throw new Error("useProjects must be used within a ProjectProvider");
  return ctx;
}

export function projectInitials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "PR";
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[1][0]).toUpperCase();
}