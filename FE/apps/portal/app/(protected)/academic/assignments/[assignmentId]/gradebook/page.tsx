import { RoleGate } from "@/components/role-gate";
import { TeacherGradebook } from "@/features/grades/teacher-gradebook";

export default async function AcademicGradebookPage({ params }: { params: Promise<{ assignmentId: string }> }) { const { assignmentId } = await params; return <RoleGate allow={["AcademicAdmin"]}><TeacherGradebook assignmentId={assignmentId} /></RoleGate>; }
