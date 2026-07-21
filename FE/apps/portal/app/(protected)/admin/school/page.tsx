import { RoleGate } from "@/components/role-gate";
import { SchoolProfile } from "@/features/school/school-profile";

export default function AdminSchoolPage() { return <RoleGate allow={["SystemAdmin"]}><SchoolProfile /></RoleGate>; }
