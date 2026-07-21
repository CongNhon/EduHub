import { RoleGate } from "@/components/role-gate";
import { SyncMonitor } from "@/features/sync/sync-monitor";
export default function SyncPage() { return <RoleGate allow={["AcademicAdmin"]}><SyncMonitor /></RoleGate>; }
