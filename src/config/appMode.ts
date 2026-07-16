/**
 * Project Controller Mode.
 *
 * When enabled (VITE_PROJECT_CONTROLLER_MODE="true"):
 *   - After login the user lands on /projects.
 *   - The projects API is fetched so a project can be selected.
 *
 * When disabled:
 *   - After login the user is taken straight to /dashboard.
 *   - The projects API is never called and project context stays empty.
 */
export const PROJECT_CONTROLLER_MODE: boolean =
  String(import.meta.env.VITE_PROJECT_CONTROLLER_MODE ?? "")
    .toLowerCase()
    .trim() === "true";

export const POST_LOGIN_ROUTE = PROJECT_CONTROLLER_MODE ? "/projects" : "/dashboard";