import { useEffect, useRef } from "react";
import { useLocation } from "react-router-dom";
import { useSidebar } from "@/components/ui/sidebar";

/**
 * Auto-collapses the shadcn/ui main sidebar on any /agent/digital route and
 * restores the previous open state when leaving the Digital Workspace.
 * Manual expansion while on the route is allowed; the original state is still
 * restored on exit.
 */
export function SidebarAutoCollapse() {
  const { pathname } = useLocation();
  const { open, setOpen } = useSidebar();
  const wasOpenRef = useRef<boolean | null>(null);
  const inDigitalRef = useRef(false);

  const isDigital = pathname === "/agent/digital" || pathname.startsWith("/agent/digital/");

  useEffect(() => {
    if (isDigital && !inDigitalRef.current) {
      // Entering the Digital Workspace — save current sidebar state and collapse.
      wasOpenRef.current = open;
      setOpen(false);
      inDigitalRef.current = true;
    } else if (!isDigital && inDigitalRef.current) {
      // Leaving the Digital Workspace — restore the saved sidebar state.
      if (wasOpenRef.current !== null) {
        setOpen(wasOpenRef.current);
      }
      wasOpenRef.current = null;
      inDigitalRef.current = false;
    }
  }, [isDigital, open, setOpen]);

  return null;
}
