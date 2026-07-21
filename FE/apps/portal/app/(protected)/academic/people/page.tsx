import { RoleGate } from "@/components/role-gate";
import { PeopleManagement } from "@/features/people/people-management";

export default function AcademicPeoplePage() { return <RoleGate allow={["AcademicAdmin"]}><PeopleManagement /></RoleGate>; }
