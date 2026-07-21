import { RoleGate } from "@/components/role-gate";
import { GradeConfigurationManagement } from "@/features/grades/grade-configuration-management";
export default function GradeConfigurationsPage() { return <RoleGate allow={["AcademicAdmin"]}><GradeConfigurationManagement /></RoleGate>; }
