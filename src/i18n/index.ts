import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import ar from "./locales/ar.json";
import en from "./locales/en.json";
import hireExamEn from "./locales/hireexam.en.json";
import hireExamAr from "./locales/hireexam.ar.json";

const STORAGE_KEY = "app.lang";

function deepMerge<T extends Record<string, unknown>>(base: T, overlay: Record<string, unknown>): T {
  const out = { ...base } as Record<string, unknown>;
  for (const [key, value] of Object.entries(overlay)) {
    const existing = out[key];
    if (
      value &&
      typeof value === "object" &&
      !Array.isArray(value) &&
      existing &&
      typeof existing === "object" &&
      !Array.isArray(existing)
    ) {
      out[key] = deepMerge(existing as Record<string, unknown>, value as Record<string, unknown>);
    } else {
      out[key] = value;
    }
  }
  return out as T;
}

if (typeof window !== "undefined" && window.localStorage.getItem(STORAGE_KEY) == null) {
  window.localStorage.setItem(STORAGE_KEY, "en");
}

const enTranslation = deepMerge(en as Record<string, unknown>, hireExamEn as Record<string, unknown>);
const arTranslation = deepMerge(ar as Record<string, unknown>, hireExamAr as Record<string, unknown>);

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      ar: { translation: arTranslation },
      en: { translation: enTranslation },
    },
    fallbackLng: "en",
    supportedLngs: ["ar", "en"],
    interpolation: { escapeValue: false },
    detection: {
      order: ["localStorage"],
      lookupLocalStorage: STORAGE_KEY,
      caches: ["localStorage"],
    },
  });

export function applyDocumentDirection(lng: string) {
  const dir = lng === "ar" ? "rtl" : "ltr";
  document.documentElement.setAttribute("dir", dir);
  document.documentElement.setAttribute("lang", lng);
}

applyDocumentDirection(i18n.language || "en");
i18n.on("languageChanged", applyDocumentDirection);

export default i18n;
