import { describe, expect, it } from "vitest";
import i18n, { applyDocumentDirection } from "@/i18n";

describe("i18n RTL", () => {
  it("sets rtl dir and lang for Arabic", () => {
    applyDocumentDirection("ar");
    expect(document.documentElement.getAttribute("dir")).toBe("rtl");
    expect(document.documentElement.getAttribute("lang")).toBe("ar");
  });

  it("sets ltr dir for English", () => {
    applyDocumentDirection("en");
    expect(document.documentElement.getAttribute("dir")).toBe("ltr");
    expect(document.documentElement.getAttribute("lang")).toBe("en");
  });

  it("provides HireExam exam strings in Arabic", async () => {
    await i18n.changeLanguage("ar");
    expect(i18n.t("exams.submit")).toBe("تسليم الامتحان");
    expect(i18n.t("nav.positions")).toBe("الوظائف");
  });

  it("provides HireExam exam strings in English", async () => {
    await i18n.changeLanguage("en");
    expect(i18n.t("exams.submit")).toBe("Submit exam");
    expect(i18n.t("dashboard.totalExams")).toBe("Total exams");
  });
});
