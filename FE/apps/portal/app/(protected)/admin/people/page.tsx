import { RoleGate } from "@/components/role-gate";
import { PeopleManagement } from "@/features/people/people-management";

export default function AdminPeoplePage() { return <RoleGate allow={["SystemAdmin"]}><PeopleManagement /></RoleGate>; }
