import { RoleGate } from "@/components/role-gate";
import { GradebookReview } from "@/features/grades/gradebook-review";

export default function AcademicGradebooksPage() { return <RoleGate allow={["AcademicAdmin"]}><GradebookReview /></RoleGate>; }
