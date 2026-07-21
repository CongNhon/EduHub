import { RoleGate } from "@/components/role-gate";
import { AcademicManagement } from "@/features/academics/academic-management";
export default function AcademicsPage() { return <RoleGate allow={["AcademicAdmin"]}><AcademicManagement /></RoleGate>; }
