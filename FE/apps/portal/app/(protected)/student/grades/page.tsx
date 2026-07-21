import { RoleGate } from "@/components/role-gate";
import { PublishedGrades } from "@/features/grades/published-grades";
export default function StudentGradesPage() { return <RoleGate allow={["Student"]}><PublishedGrades studentMode /></RoleGate>; }
