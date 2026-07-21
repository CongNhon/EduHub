import { RoleGate } from "@/components/role-gate";
import { ProfileRequestManagement } from "@/features/profiles/profile-request-management";
export default function ProfileRequestsPage() { return <RoleGate allow={["AcademicAdmin"]}><ProfileRequestManagement /></RoleGate>; }
