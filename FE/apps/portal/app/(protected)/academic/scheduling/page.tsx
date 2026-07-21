import { RoleGate } from "@/components/role-gate";
import { SchedulingManagement } from "@/features/scheduling/scheduling-management";
export default function SchedulingPage() { return <RoleGate allow={["AcademicAdmin"]}><SchedulingManagement /></RoleGate>; }
