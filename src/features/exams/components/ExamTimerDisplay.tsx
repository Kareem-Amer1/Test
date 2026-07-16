import { cn } from "@/lib/utils";

interface Props {
  formatted: string;
  isExpired: boolean;
  urgent?: boolean;
}

export function ExamTimerDisplay({ formatted, isExpired, urgent }: Props) {
  return (
    <div
      className={cn(
        "font-mono text-2xl font-semibold tabular-nums px-4 py-2 rounded-lg border",
        isExpired && "text-destructive border-destructive/50 bg-destructive/10",
        urgent && !isExpired && "text-amber-600 border-amber-500/50 bg-amber-500/10",
        !isExpired && !urgent && "border-app-border-strong bg-muted/50",
      )}
    >
      {formatted}
    </div>
  );
}
