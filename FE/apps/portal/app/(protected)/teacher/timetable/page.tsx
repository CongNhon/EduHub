import { RoleGate } from "@/components/role-gate";
import { RoleTimetable } from "@/features/scheduling/role-timetable";
export default function TeacherTimetablePage() { return <RoleGate allow={["Teacher"]}><RoleTimetable role="Teacher" /></RoleGate>; }
