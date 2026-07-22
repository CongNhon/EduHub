import { RoleGate } from "@/components/role-gate";
import { AdminAnalyticsDashboard } from "@/features/analytics/admin-analytics-dashboard";

/** AdminPage hiển thị DevExpress analytics dashboard dành riêng cho SystemAdmin. */
export default function AdminPage() { return <RoleGate allow={["SystemAdmin"]}><AdminAnalyticsDashboard /></RoleGate>; }
