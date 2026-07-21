import { RoleGate } from "@/components/role-gate";
import { ReportCenter } from "@/features/reports/report-center";
export default async function ReportsPage({ searchParams }: { searchParams: Promise<{ studentId?: string }> }) { const query = await searchParams; return <RoleGate allow={["Parent", "AcademicAdmin"]}><ReportCenter initialStudentId={query.studentId} /></RoleGate>; }
