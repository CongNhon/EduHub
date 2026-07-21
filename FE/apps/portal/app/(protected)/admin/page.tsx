import { RoleGate } from "@/components/role-gate";
import { RoleDashboard } from "@/features/dashboard/role-dashboard";
export default function AdminPage() { return <RoleGate allow={["SystemAdmin"]}><RoleDashboard role="SystemAdmin" /></RoleGate>; }
