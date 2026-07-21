import { RoleGate } from "@/components/role-gate";
import { RoleDashboard } from "@/features/dashboard/role-dashboard";
export default function StudentPage() { return <RoleGate allow={["Student"]}><RoleDashboard role="Student" /></RoleGate>; }
