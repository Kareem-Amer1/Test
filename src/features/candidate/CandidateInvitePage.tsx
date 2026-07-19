import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import {
  useCandidateExamSession,
  useInviteInfo,
  useStartCandidateExam,
} from "@/features/invitations/useInvitations";
import {
  CandidateExamCompleteShell,
  CandidateExamSession,
  InvitePageShell,
} from "./CandidateExamSession";

const schema = z.object({
  fullName: z.string().min(2, "At least 2 characters"),
  email: z.string().email("Invalid email"),
  mobile: z.string().min(6, "Mobile number required"),
});

type FormData = z.infer<typeof schema>;

export default function CandidateInvitePage() {
  const { t } = useTranslation();
  const { token = "" } = useParams<{ token: string }>();
  const { data: info, isLoading: infoLoading, isError: infoError, refetch } = useInviteInfo(token);
  const [examStarted, setExamStarted] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const shouldLoadSession = !!info && (info.canResume || examStarted);
  const {
    data: session,
    isLoading: sessionLoading,
    isError: sessionError,
    error: sessionErr,
  } = useCandidateExamSession(token, shouldLoadSession && !submitted);

  const startExam = useStartCandidateExam(token);
  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { fullName: "", email: "", mobile: "" },
  });

  useEffect(() => {
    const apiError = sessionErr as (Error & { code?: string }) | undefined;
    if (sessionError && apiError?.code === "exams.time_expired") {
      setSubmitted(true);
    }
  }, [sessionError, sessionErr]);

  const onRegister = handleSubmit(async (values) => {
    try {
      await startExam.mutateAsync({
        fullName: values.fullName.trim(),
        email: values.email.trim(),
        mobile: values.mobile.trim(),
      });
      setExamStarted(true);
    } catch (err) {
      toast.error(err instanceof Error ? err.message : String(err));
    }
  });

  if (infoLoading) {
    return (
      <InvitePageShell>
        <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>
      </InvitePageShell>
    );
  }

  if (infoError || !info) {
    return (
      <InvitePageShell>
        <div className="space-y-2">
          <p className="text-sm text-destructive">{t("invitations.invalidLink", "This invitation link is invalid.")}</p>
          <Button variant="outline" size="sm" onClick={() => refetch()}>{t("common.retry", "Retry")}</Button>
        </div>
      </InvitePageShell>
    );
  }

  if (info.isCompleted || submitted) {
    return (
      <CandidateExamCompleteShell
        session={{
          id: "",
          candidateName: "",
          positionName: info.positionName,
          durationMinutes: 0,
          elapsedSeconds: 0,
          remainingSeconds: 0,
          startedAt: "",
          status: "Submitted",
          questions: [],
          answers: [],
        }}
      />
    );
  }

  if (info.isExpired && !info.canResume) {
    return (
      <InvitePageShell>
        <div className="text-center space-y-3 py-8">
          <h1 className="text-xl font-semibold">{t("invitations.expiredTitle", "Link expired")}</h1>
          <p className="text-sm text-muted-foreground">
            {t("invitations.expiredMessage", "This invitation link has expired. Please contact HR for a new link.")}
          </p>
        </div>
      </InvitePageShell>
    );
  }

  if (shouldLoadSession) {
    if (sessionLoading) {
      return (
        <InvitePageShell>
          <p className="text-sm text-muted-foreground">{t("common.loading", "Loading…")}</p>
        </InvitePageShell>
      );
    }

    if (sessionError && submitted) {
      return (
        <CandidateExamCompleteShell
          session={{
            id: "",
            candidateName: "",
            positionName: info.positionName,
            durationMinutes: 0,
            elapsedSeconds: 0,
            remainingSeconds: 0,
            startedAt: "",
            status: "Submitted",
            questions: [],
            answers: [],
          }}
        />
      );
    }

    if (sessionError || !session) {
      return (
        <InvitePageShell>
          <p className="text-sm text-destructive">{t("exams.loadError", "Failed to load exam.")}</p>
        </InvitePageShell>
      );
    }

    if (session.status === "Submitted" || session.status === "Graded") {
      return <CandidateExamCompleteShell session={session} />;
    }

    return (
      <CandidateExamSession
        token={token}
        session={session}
        onSubmitted={() => setSubmitted(true)}
      />
    );
  }

  return (
    <InvitePageShell>
      <div className="space-y-6 py-4">
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-semibold">{t("invitations.welcomeTitle", "Exam invitation")}</h1>
          <p className="text-muted-foreground">
            {t("invitations.welcomePosition", "You have been invited to take an exam for {{position}}.", {
              position: info.positionName,
            })}
          </p>
        </div>

        <form onSubmit={onRegister} className="space-y-4 rounded-lg border bg-card p-6">
          <div className="space-y-1.5">
            <Label htmlFor="fullName">{t("invitations.fullName", "Full name")}</Label>
            <Input id="fullName" {...register("fullName")} autoFocus />
            {errors.fullName && <p className="text-xs text-destructive">{errors.fullName.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="email">{t("invitations.email", "Email address")}</Label>
            <Input id="email" type="email" {...register("email")} />
            {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="mobile">{t("invitations.mobile", "Mobile number")}</Label>
            <Input id="mobile" type="tel" {...register("mobile")} />
            {errors.mobile && <p className="text-xs text-destructive">{errors.mobile.message}</p>}
          </div>
          <Button type="submit" className="w-full" disabled={startExam.isPending}>
            {t("invitations.beginExam", "Begin exam")}
          </Button>
        </form>
      </div>
    </InvitePageShell>
  );
}
