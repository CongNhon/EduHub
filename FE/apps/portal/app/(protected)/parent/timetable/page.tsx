import { RoleGate } from "@/components/role-gate";
import { RoleTimetable } from "@/features/scheduling/role-timetable";
export default function ParentTimetablePage() { return <RoleGate allow={["Parent"]}><RoleTimetable role="Parent" /></RoleGate>; }
