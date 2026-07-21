import { RoleGate } from "@/components/role-gate";
import { SystemHealth } from "@/features/health/system-health";
export default function SystemHealthPage() { return <RoleGate allow={["SystemAdmin"]}><SystemHealth /></RoleGate>; }
