import { RoleGate } from "@/components/role-gate";
import { StudentImport } from "@/features/imports/student-import";
export default function StudentImportsPage() { return <RoleGate allow={["AcademicAdmin"]}><StudentImport /></RoleGate>; }
