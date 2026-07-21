import { RoleGate } from "@/components/role-gate";
import { StudentProfile } from "@/features/profiles/student-profile";
export default function StudentProfilePage() { return <RoleGate allow={["Student"]}><StudentProfile /></RoleGate>; }
