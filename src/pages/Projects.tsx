import { useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Logo } from "@/components/app/Logo";
import { ThemeStudio } from "@/components/app/ThemeStudio";
import { LanguageSwitcher } from "@/components/app/LanguageSwitcher";
import { UserMenu } from "@/components/app/UserMenu";
import { PROJECT_COLORS, projectInitials, useProjects, type Project } from "@/contexts/ProjectContext";
import { cn } from "@/lib/utils";
import { useAuth } from "@/hooks/useAuth";

function formatDate(iso: string) {
  try {
    return new Date(iso).toLocaleDateString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  } catch {
    return iso;
  }
}

function ProjectsTopBar() {
  const { t } = useTranslation();
  const { signOut } = useAuth();
  const navigate = useNavigate();
  return (
    <header className="h-14 shrink-0 bg-card border-b border-app-border-strong flex items-center px-4 gap-3 sticky top-0 z-40">
      <Logo />
      <div className="flex-1" />
      <div className="flex items-center gap-2 shrink-0">
        <UserMenu />
        <ThemeStudio />
        <LanguageSwitcher />
        <Button
          variant="ghost"
          size="sm"
          onClick={async () => {
            await signOut();
            navigate("/auth", { replace: true });
          }}
        >
          {t("user.signOut", "Sign out")}
        </Button>
      </div>
    </header>
  );
}

function NewProjectDialog({
  open,
  onOpenChange,
  onCreate,
}: {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  onCreate: (input: { name: string; description: string; color: string }) => void;
}) {
  const { t } = useTranslation();
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [color, setColor] = useState(PROJECT_COLORS[0]);

  const reset = () => {
    setName("");
    setDescription("");
    setColor(PROJECT_COLORS[0]);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(v) => {
        if (!v) reset();
        onOpenChange(v);
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("projects.newTitle", "New project")}</DialogTitle>
          <DialogDescription>
            {t("projects.newDescription", "Give your project a name, description, and color.")}
          </DialogDescription>
        </DialogHeader>
        <form
          className="space-y-4"
          onSubmit={(e) => {
            e.preventDefault();
            if (!name.trim()) return;
            onCreate({ name, description, color });
            reset();
            onOpenChange(false);
          }}
        >
          <div className="space-y-2">
            <Label htmlFor="project-name">{t("projects.fields.name", "Name")}</Label>
            <Input
              id="project-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder={t("projects.fields.namePlaceholder", "My new project")}
              autoFocus
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="project-description">{t("projects.fields.description", "Description")}</Label>
            <Textarea
              id="project-description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder={t("projects.fields.descriptionPlaceholder", "What is this project about?")}
              rows={3}
            />
          </div>
          <div className="space-y-2">
            <Label>{t("projects.fields.color", "Color")}</Label>
            <div className="flex flex-wrap gap-2">
              {PROJECT_COLORS.map((c) => (
                <button
                  key={c}
                  type="button"
                  aria-label={c}
                  onClick={() => setColor(c)}
                  className={cn(
                    "h-8 w-8 rounded-md ring-offset-background transition-all",
                    color === c
                      ? "ring-2 ring-ring ring-offset-2 scale-110"
                      : "hover:scale-105",
                  )}
                  style={{ backgroundColor: c }}
                />
              ))}
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t("common.cancel", "Cancel")}
            </Button>
            <Button type="submit">{t("common.create", "Create")}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function ProjectCard({
  project,
  onDelete,
}: {
  project: Project;
  onDelete: (id: string) => void;
}) {
  const { t } = useTranslation();
  return (
    <Link
      to={`/project/${project.id}/dashboard`}
      className="group relative bg-card rounded-xl border border-app-border-strong p-5 hover:border-primary/50 hover:shadow-md transition-all flex flex-col gap-4"
    >
      <div className="flex items-start gap-4">
        <div
          className="h-14 w-14 rounded-lg flex items-center justify-center text-white font-bold text-lg shrink-0"
          style={{ backgroundColor: project.color }}
        >
          {projectInitials(project.name)}
        </div>
        <div className="flex-1 min-w-0">
          <div className="font-semibold truncate">{project.name}</div>
          <div className="text-xs text-muted-foreground mt-0.5">
            {t("projects.createdOn", "Created {{date}}", { date: formatDate(project.createdAt) })}
          </div>
        </div>
      </div>
      {project.description && (
        <p className="text-sm text-muted-foreground line-clamp-2">{project.description}</p>
      )}
      <button
        type="button"
        onClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
          if (window.confirm(t("projects.confirmDelete", "Delete this project?"))) {
            onDelete(project.id);
          }
        }}
        aria-label={t("common.delete", "Delete")}
        className="absolute top-3 end-3 h-8 w-8 rounded-md inline-flex items-center justify-center text-muted-foreground opacity-0 group-hover:opacity-100 hover:bg-destructive/10 hover:text-destructive transition-all"
      >
        <Trash2 className="h-4 w-4" />
      </button>
    </Link>
  );
}

export default function ProjectsPage() {
  const { t, i18n } = useTranslation();
  const dir = i18n.language === "ar" ? "rtl" : "ltr";
  const { projects, createProject, deleteProject } = useProjects();
  const [dialogOpen, setDialogOpen] = useState(false);

  const sorted = useMemo(
    () =>
      [...projects].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      ),
    [projects],
  );

  return (
    <div data-theme="dashboard" dir={dir} className="min-h-screen bg-background flex flex-col">
      <ProjectsTopBar />
      <main className="flex-1 px-6 py-8 max-w-6xl w-full mx-auto">
        <div className="flex items-center justify-between mb-8 gap-4">
          <div>
            <h1 className="text-2xl font-bold">{t("projects.title", "Projects")}</h1>
            <p className="text-sm text-muted-foreground mt-1">
              {t("projects.subtitle", "Pick a project to continue or create a new one.")}
            </p>
          </div>
          <Button onClick={() => setDialogOpen(true)} className="shrink-0">
            <Plus className="h-4 w-4" />
            {t("projects.new", "New Project")}
          </Button>
        </div>

        {sorted.length === 0 ? (
          <div className="bg-card rounded-xl border border-dashed border-app-border-strong p-12 flex flex-col items-center justify-center text-center gap-3">
            <p className="font-medium">{t("projects.emptyTitle", "No projects yet")}</p>
            <p className="text-sm text-muted-foreground max-w-sm">
              {t("projects.emptyDesc", "Create your first project to get started.")}
            </p>
            <Button onClick={() => setDialogOpen(true)}>
              <Plus className="h-4 w-4" />
              {t("projects.new", "New Project")}
            </Button>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {sorted.map((p) => (
              <ProjectCard key={p.id} project={p} onDelete={deleteProject} />
            ))}
          </div>
        )}
      </main>

      <NewProjectDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onCreate={(input) => createProject(input)}
      />
    </div>
  );
}