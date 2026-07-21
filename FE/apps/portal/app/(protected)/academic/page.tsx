import { RoleGate } from "@/components/role-gate";
import { RoleDashboard } from "@/features/dashboard/role-dashboard";
export default function AcademicPage() { return <RoleGate allow={["AcademicAdmin"]}><RoleDashboard role="AcademicAdmin" /></RoleGate>; }
