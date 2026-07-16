export function getCopilotLabels(isRtl: boolean) {
  return {
    title: isRtl ? "المساعد الذكي" : "AI Assistant",
    placeholder: isRtl
      ? "اكتب سؤالك…"
      : "Ask a question…",
    initial: isRtl
      ? "مرحباً — كيف يمكنني مساعدتك؟"
      : "Hi — how can I help you today?",
    error: isRtl ? "حدث خطأ. حاول مرة أخرى." : "Something went wrong. Try again.",
    stopGenerating: isRtl ? "إيقاف" : "Stop generating",
    regenerateResponse: isRtl ? "إعادة المحاولة" : "Regenerate response",
    copyToClipboard: isRtl ? "نسخ" : "Copy",
    thumbsUp: isRtl ? "مفيد" : "Thumbs up",
    thumbsDown: isRtl ? "غير مفيد" : "Thumbs down",
    copied: isRtl ? "تم النسخ" : "Copied",
  };
}
