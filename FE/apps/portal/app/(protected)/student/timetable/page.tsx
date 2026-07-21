import { RoleGate } from "@/components/role-gate";
import { RoleTimetable } from "@/features/scheduling/role-timetable";
export default function StudentTimetablePage() { return <RoleGate allow={["Student"]}><RoleTimetable role="Student" /></RoleGate>; }
