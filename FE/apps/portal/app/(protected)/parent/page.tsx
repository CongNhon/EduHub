import { RoleGate } from "@/components/role-gate";
import { RoleDashboard } from "@/features/dashboard/role-dashboard";
export default function ParentPage() { return <RoleGate allow={["Parent"]}><RoleDashboard role="Parent" /></RoleGate>; }
