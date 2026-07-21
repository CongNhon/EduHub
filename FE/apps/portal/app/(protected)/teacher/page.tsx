import { RoleGate } from "@/components/role-gate";
import { RoleDashboard } from "@/features/dashboard/role-dashboard";
export default function TeacherPage() { return <RoleGate allow={["Teacher"]}><RoleDashboard role="Teacher" /></RoleGate>; }
