import { describe, expect, it } from "vitest";
import { render } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { Sidebar, SidebarProvider } from "@/components/ui/sidebar";
import { SidebarAutoCollapse } from "../SidebarAutoCollapse";

function TestSidebar() {
  return (
    <SidebarProvider>
      <SidebarAutoCollapse />
      <Sidebar collapsible="icon" data-testid="test-sidebar" />
    </SidebarProvider>
  );
}

function App({ initialPath }: { initialPath: string }) {
  return (
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/dashboard" element={<TestSidebar />} />
        <Route path="/agent/digital" element={<TestSidebar />} />
        <Route path="/agent/digital/conversations/:id" element={<TestSidebar />} />
        <Route path="/agent/digital/customers/:id" element={<TestSidebar />} />
      </Routes>
    </MemoryRouter>
  );
}

describe("SidebarAutoCollapse", () => {
  it("keeps the sidebar expanded on non-digital routes", () => {
    const { container } = render(<App initialPath="/dashboard" />);
    const sidebar = container.querySelector("[data-state]");
    expect(sidebar).toHaveAttribute("data-state", "expanded");
  });

  it("collapses the sidebar on /agent/digital", () => {
    const { container } = render(<App initialPath="/agent/digital" />);
    const sidebar = container.querySelector("[data-state]");
    expect(sidebar).toHaveAttribute("data-state", "collapsed");
  });

  it("collapses the sidebar on /agent/digital sub-routes", () => {
    const { container } = render(<App initialPath="/agent/digital/conversations/123" />);
    const sidebar = container.querySelector("[data-state]");
    expect(sidebar).toHaveAttribute("data-state", "collapsed");
  });
});
