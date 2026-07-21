import { RoleGate } from "@/components/role-gate";
import { TeacherClasses } from "@/features/grades/teacher-classes";
export default function TeacherClassesPage() { return <RoleGate allow={["Teacher"]}><TeacherClasses /></RoleGate>; }
