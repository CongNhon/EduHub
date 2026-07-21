import type { ReactNode } from "react";
import { PortalShell } from "@/components/portal-shell";

/** Protected layout đặt mọi screen nghiệp vụ trong portal shell có session guard. */
export default function ProtectedLayout({ children }: { children: ReactNode }) { return <PortalShell>{children}</PortalShell>; }
