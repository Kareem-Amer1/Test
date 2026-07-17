import { useCallback, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useBlocker } from "react-router-dom";
import { toast } from "sonner";

export function useExamLockdown(active: boolean) {
  const { t } = useTranslation();

  const shouldBlock = useCallback(
    ({
      currentLocation,
      nextLocation,
    }: {
      currentLocation: { pathname: string };
      nextLocation: { pathname: string };
    }) => active && currentLocation.pathname !== nextLocation.pathname,
    [active],
  );

  const blocker = useBlocker(shouldBlock);

  useEffect(() => {
    if (blocker.state !== "blocked") return;
    toast.error(t("exams.cannotLeave", "You cannot leave the exam until you submit."));
    blocker.reset();
  }, [blocker, t]);

  useEffect(() => {
    if (!active) return;

    const onBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener("beforeunload", onBeforeUnload);
    return () => window.removeEventListener("beforeunload", onBeforeUnload);
  }, [active]);
}
