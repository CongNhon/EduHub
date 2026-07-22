import { RoleGate } from "@/components/role-gate";
import { AdminAnalyticsReport } from "@/features/analytics/admin-analytics-report";

/** AdminReportsPage mở DevExpress Web Document Viewer cho SystemAdmin. */
export default function AdminReportsPage() { return <RoleGate allow={["SystemAdmin"]}><AdminAnalyticsReport /></RoleGate>; }
