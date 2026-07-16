import { Link, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { usePositions, useTemplate } from "./usePositions";
import { TemplateDurationForm } from "./components/TemplateDurationForm";
import { QuestionsList } from "./components/QuestionsList";

export default function TemplateEditorPage() {
  const { t } = useTranslation();
  const { positionId = "" } = useParams<{ positionId: string }>();
  const { data: positions } = usePositions();
  const { data: template, isLoading, isError, refetch } = useTemplate(positionId);

  const position = positions?.find((p) => p.id === positionId);

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>;
  }

  if (isError || !template) {
    return (
      <div className="space-y-2">
        <p className="text-sm text-destructive">{t("templates.loadError", "Failed to load template.")}</p>
        <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Button variant="ghost" size="sm" className="-ms-2" asChild>
          <Link to="/positions">
            <ArrowLeft className="h-4 w-4 me-1" />
            {t("positions.back", "Back to positions")}
          </Link>
        </Button>
        <h1 className="text-2xl font-semibold">
          {position?.name ?? t("templates.title", "Exam template")}
        </h1>
        {position?.description && (
          <p className="text-sm text-muted-foreground">{position.description}</p>
        )}
      </div>

      <div className="rounded-lg border border-app-border-strong bg-card p-5">
        <TemplateDurationForm positionId={positionId} durationMinutes={template.durationMinutes} />
      </div>

      <QuestionsList positionId={positionId} questions={template.questions} />
    </div>
  );
}
