import { RoleGate } from "@/components/role-gate";
import { PublishedGrades } from "@/features/grades/published-grades";
export default async function ParentGradesPage({ params, searchParams }: { params: Promise<{ studentId: string }>; searchParams: Promise<{ assignmentId?: string }> }) { const [{ studentId }, query] = await Promise.all([params, searchParams]); return <RoleGate allow={["Parent"]}><PublishedGrades studentId={studentId} assignmentId={query.assignmentId} /></RoleGate>; }
