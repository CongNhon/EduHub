import { RoleGate } from "@/components/role-gate";
import { StudentManagement } from "@/features/students/student-management";
export default function StudentsPage() { return <RoleGate allow={["AcademicAdmin"]}><StudentManagement /></RoleGate>; }
