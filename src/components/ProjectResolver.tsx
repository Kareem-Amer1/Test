import { useEffect } from "react";
import { Navigate, Outlet, useParams } from "react-router-dom";
import { useProjects } from "@/contexts/ProjectContext";

export function ProjectResolver() {
  const { id } = useParams<{ id: string }>();
  const { projects, setActiveProjectId, activeProject } = useProjects();

  const exists = !!id && projects.some((p) => p.id === id);

  useEffect(() => {
    if (exists && id) setActiveProjectId(id);
    return () => {
      // Don't clear on unmount — keep active project in context for navigation back.
    };
  }, [exists, id, setActiveProjectId]);

  if (!exists) return <Navigate to="/projects" replace />;
  if (!activeProject || activeProject.id !== id) return null;

  return <Outlet />;
}