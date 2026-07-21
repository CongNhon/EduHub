import { RoleGate } from "@/components/role-gate";
import { ClassManagement } from "@/features/classes/class-management";
export default function ClassesPage() { return <RoleGate allow={["AcademicAdmin"]}><ClassManagement /></RoleGate>; }
