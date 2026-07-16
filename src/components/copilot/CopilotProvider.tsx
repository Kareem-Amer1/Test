import { createContext, ReactNode, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { CopilotKitProvider, CopilotChat } from "@copilotkit/react-core/v2";
import "@copilotkit/react-core/v2/styles.css";
import "@/copilot-sidebar-overrides.css";
import { useTranslation } from "react-i18next";
import { X } from "lucide-react";
import { toast } from "sonner";
import { getCopilotLabels } from "@/copilot/shared/constants";

const RUNTIME_URL =
  (import.meta.env.VITE_COPILOT_RUNTIME_URL as string | undefined) ??
  "https://copilotkit.alkhwarizmi.pro/api/copilotkit";

interface CopilotUiContextValue {
  isCopilotOpen: boolean;
  setIsCopilotOpen: (open: boolean) => void;
  toggleCopilot: () => void;
  copilotAvailable: boolean;
  threadId: string | null;
  setThreadId: (id: string | null) => void;
}

const CopilotUiContext = createContext<CopilotUiContextValue | null>(null);

export function useCopilotUiContext() {
  const ctx = useContext(CopilotUiContext);
  if (!ctx) throw new Error("useCopilotUiContext must be used inside CopilotProvider");
  return ctx;
}

export function CopilotProvider({ children }: { children: ReactNode }) {
  const { i18n } = useTranslation();
  const isRtl = i18n.language?.toLowerCase().startsWith("ar") ?? false;

  const [isCopilotOpen, setIsCopilotOpen] = useState(false);
  const [chatMounted, setChatMounted] = useState(false);
  const [threadId, setThreadId] = useState<string | null>(null);

  const copilotAvailable = Boolean(RUNTIME_URL);

  const toggleCopilot = useCallback(() => {
    if (!copilotAvailable) {
      toast.error(isRtl ? "المساعد غير متاح حالياً." : "Assistant is not available right now.");
      return;
    }
    setChatMounted(true);
    setIsCopilotOpen((o) => !o);
  }, [copilotAvailable, isRtl]);

  useEffect(() => {
    if (!isCopilotOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setIsCopilotOpen(false);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [isCopilotOpen]);

  const value = useMemo<CopilotUiContextValue>(
    () => ({ isCopilotOpen, setIsCopilotOpen, toggleCopilot, copilotAvailable, threadId, setThreadId }),
    [isCopilotOpen, toggleCopilot, copilotAvailable, threadId],
  );

  const effectiveThreadId = useMemo(() => {
    if (threadId) return threadId;
    return crypto.randomUUID();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const labels = getCopilotLabels(isRtl);

  return (
    <CopilotUiContext.Provider value={value}>
      <CopilotKitProvider runtimeUrl={RUNTIME_URL}>
        <div
          className="alk-copilot-shell"
          dir={isRtl ? "rtl" : "ltr"}
          data-copilot-open={isCopilotOpen ? "true" : "false"}
        >
          <div className="alk-shell-content">{children}</div>

          <aside
            className="alk-copilot-overlay"
            data-copilot-panel=""
            aria-hidden={!isCopilotOpen}
            aria-label={labels.title}
          >
            <header className="flex items-center justify-between px-4 py-3 border-b border-border bg-background/95 backdrop-blur shrink-0">
              <div className="text-sm font-medium">{labels.title}</div>
              <button
                type="button"
                onClick={() => setIsCopilotOpen(false)}
                aria-label={isRtl ? "إغلاق المساعد" : "Close assistant"}
                className="inline-flex h-8 w-8 items-center justify-center rounded-full text-muted-foreground hover:bg-muted hover:text-foreground transition"
              >
                <X className="h-4 w-4" />
              </button>
            </header>
            <div className="flex-1 min-h-0 overflow-hidden">
              {chatMounted && (
                <CopilotChat
                  key={effectiveThreadId}
                  agentId="default"
                  threadId={effectiveThreadId}
                  className="alk-copilot-chat"
                  autoScroll="pin-to-send"
                />
              )}
            </div>
          </aside>
        </div>
      </CopilotKitProvider>
    </CopilotUiContext.Provider>
  );
}
