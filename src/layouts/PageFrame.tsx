import { Outlet } from "react-router-dom";

/** Shared content inset for authenticated pages inside AppShell. */
export function PageFrame() {
  return (
    <div className="mx-auto w-full max-w-6xl px-6 py-8 lg:px-8">
      <Outlet />
    </div>
  );
}
