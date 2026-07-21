import { RoleGate } from "@/components/role-gate";
import { TeacherGradebook } from "@/features/grades/teacher-gradebook";
export default async function GradebookPage({ params }: { params: Promise<{ assignmentId: string }> }) { const { assignmentId } = await params; return <RoleGate allow={["Teacher"]}><TeacherGradebook assignmentId={assignmentId} /></RoleGate>; }
