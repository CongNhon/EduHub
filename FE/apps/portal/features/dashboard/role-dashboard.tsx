"use client";

import type { UserRole } from "@eduhub/auth";
import { Badge, Card, CardHeader, EmptyState } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Bell, BookOpenCheck, Boxes, CheckCircle2, FileText, HeartPulse, School, Users } from "lucide-react";
import Link from "next/link";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { useSession } from "@/components/session-provider";
import type { ChildSummaryDto, ClassRoomDto, Envelope, NotificationDto, PagedEnvelope, SchoolProfileDto, StudentDto, TeachingAssignmentSummaryDto } from "@/lib/domain";
import { formatDateTime, statusLabel, statusTone } from "@/lib/domain";

const dashboardCopy: Record<UserRole, { eyebrow: string; title: string; description: string }> = {
  Parent: { eyebrow: "TỔNG QUAN GIA ĐÌNH", title: "Thông tin quan trọng của con", description: "Điểm mới, báo cáo và thông báo được sắp theo thời điểm cần chú ý." },
  Student: { eyebrow: "TỔNG QUAN HỌC TẬP", title: "Kết quả và bước tiếp theo", description: "Theo dõi thông báo mới và mở đúng kết quả đã được nhà trường công bố." },
  Teacher: { eyebrow: "KHÔNG GIAN GIÁO VIÊN", title: "Công việc cần hoàn tất", description: "Theo dõi lớp được phân công và trạng thái luồng nhập điểm." },
  AcademicAdmin: { eyebrow: "VẬN HÀNH HỌC VỤ", title: "Điểm nghẽn cần xử lý", description: "Dữ liệu nền, lớp học và hồ sơ học sinh trong một góc nhìn vận hành." },
  SystemAdmin: { eyebrow: "QUẢN TRỊ HỆ THỐNG", title: "Hệ thống có khỏe và an toàn không?", description: "Theo dõi dependency, tài khoản, phân quyền và cấu hình vận hành." },
};

/** RoleDashboard tổng hợp dữ liệu thật phù hợp từng role và ưu tiên next action. */
export function RoleDashboard({ role }: { role: UserRole }) {
  const { request } = useSession();
  const notifications = useQuery({ queryKey: ["notifications", "dashboard", role], queryFn: () => request<PagedEnvelope<NotificationDto>>("/api/v1/notifications?page=1&pageSize=5") });
  const students = useQuery({ queryKey: ["students", "dashboard"], queryFn: () => request<PagedEnvelope<StudentDto>>("/api/v1/students?page=1&pageSize=1"), enabled: role === "AcademicAdmin" });
  const classes = useQuery({ queryKey: ["classes", "dashboard", role], queryFn: () => request<PagedEnvelope<ClassRoomDto>>("/api/v1/classes?page=1&pageSize=50"), enabled: role === "AcademicAdmin" });
  const children = useQuery({ queryKey: ["children", "dashboard"], queryFn: () => request<Envelope<ChildSummaryDto[]>>("/api/v1/me/children"), enabled: role === "Parent" });
  const teachingAssignments = useQuery({ queryKey: ["teaching-assignments", "dashboard"], queryFn: () => request<Envelope<TeachingAssignmentSummaryDto[]>>("/api/v1/me/teaching-assignments"), enabled: role === "Teacher" });
  const school = useQuery({ queryKey: ["school-profile"], queryFn: () => request<Envelope<SchoolProfileDto>>("/api/v1/school-profile") });
  const health = useQuery({ queryKey: ["health", "ready"], queryFn: () => request<Record<string, unknown>>("/health/ready"), enabled: role === "SystemAdmin", refetchInterval: 30_000 });
  const copy = dashboardCopy[role];
  if (notifications.isLoading || students.isLoading || classes.isLoading || children.isLoading || teachingAssignments.isLoading || school.isLoading || health.isLoading) return <><PageHeader {...copy} /><LoadingPanel rows={7} /></>;
  const dashboardError = notifications.error || students.error || classes.error || children.error || teachingAssignments.error || school.error || health.error;
  if (dashboardError) return <><PageHeader {...copy} /><ErrorPanel message={(dashboardError as Error).message} onRetry={() => { void notifications.refetch(); void children.refetch(); void teachingAssignments.refetch(); }} /></>;
  const notificationItems = notifications.data?.data.items || [];
  const unread = notificationItems.filter((item) => !item.isRead).length;
  const classItems = classes.data?.data.items || [];
  const childItems = children.data?.data || [];
  const assignmentItems = teachingAssignments.data?.data || [];
  return <div className="page-stack"><PageHeader {...copy} actions={<Link className="ui-button ui-button--outline" href="/notifications"><Bell size={16} /> Xem thông báo</Link>} />
    {school.data?.data ? <div className="school-context"><School size={18} /><div><b>{school.data.data.name}</b><span>{school.data.data.address || school.data.data.code}</span></div></div> : null}
    <section className="metric-grid">
      {role === "AcademicAdmin" ? <MetricCard label="Hồ sơ học sinh" value={students.data?.data.totalCount || 0} caption="Trong hệ thống" icon={<Users />} /> : null}
      {role === "AcademicAdmin" ? <MetricCard label="Lớp đang hoạt động" value={classItems.filter((item) => item.isActive).length} caption={`${classItems.length} lớp đã tải`} icon={<Boxes />} /> : null}
      <MetricCard label="Thông báo chưa đọc" value={unread} caption="Trong 5 thông báo gần nhất" icon={<Bell />} />
      {role === "SystemAdmin" ? <MetricCard label="Readiness" value={health.data ? "Đã kiểm tra" : "Chưa rõ"} caption="Tự làm mới sau 30 giây" icon={<HeartPulse />} /> : null}
      {role === "Parent" ? <MetricCard label="Học sinh liên kết" value={childItems.length} caption="Con thuộc tài khoản hiện tại" icon={<Users />} /> : null}
      {role === "Teacher" ? <MetricCard label="Phân công giảng dạy" value={assignmentItems.length} caption="Lớp - môn của giáo viên" icon={<BookOpenCheck />} /> : null}
      {role === "Student" ? <MetricCard label="Dữ liệu điểm" value="Published" caption="Chỉ hiển thị kết quả đã công bố" icon={<BookOpenCheck />} /> : null}
    </section>
    {role === "Parent" ? <section className="child-grid">{childItems.length ? childItems.map((child) => <Card key={child.id} className="child-card"><div className="child-card__identity"><span>{child.fullName.charAt(0)}</span><div><small>{child.relationship}</small><h3>{child.fullName}</h3><p>{child.studentCode}</p></div></div><dl><div><dt>Lớp hiện tại</dt><dd>{child.currentClassName || "Chưa xếp lớp"}</dd></div><div><dt>Học kỳ</dt><dd>{child.currentSemesterName || "Chưa xác định"}</dd></div></dl><Link className="ui-button ui-button--outline" href={`/reports?studentId=${child.id}`}><FileText size={16} /> Yêu cầu báo cáo</Link></Card>) : <Card><EmptyState icon={Users} title="Chưa có học sinh liên kết" description="Quản trị học vụ cần gắn tài khoản phụ huynh với hồ sơ học sinh." /></Card>}</section> : null}
    {role === "Teacher" ? <section className="assignment-grid">{assignmentItems.length ? assignmentItems.map((assignment) => <Link key={assignment.id} href={`/teacher/assignments/${assignment.id}/gradebook`} className="assignment-card"><div><span>{assignment.subjectCode}</span><Badge tone={statusTone(assignment.gradebookStatus)}>{statusLabel(assignment.gradebookStatus)}</Badge></div><h3>{assignment.subjectName}</h3><p>{assignment.className} · {assignment.semesterName}</p><small>{assignment.studentCount} học sinh</small><ArrowRight size={18} /></Link>) : <Card><EmptyState icon={BookOpenCheck} title="Chưa có phân công giảng dạy" description="Quản trị học vụ cần phân công lớp, môn và học kỳ cho giáo viên." /></Card>}</section> : null}
    <section className="dashboard-grid">
      <Card className="dashboard-feed"><CardHeader title="Cần chú ý" description="Hoạt động gần nhất thuộc tài khoản hiện tại" action={<Link href="/notifications">Xem tất cả</Link>} />{notificationItems.length ? <div className="activity-list">{notificationItems.map((item) => <Link href={notificationHref(item, role)} key={item.id} className={!item.isRead ? "unread" : ""}><span className="activity-list__icon"><Bell size={17} /></span><div><b>{item.title}</b><p>{item.body}</p><small>{formatDateTime(item.occurredAtUtc)}</small></div><ArrowRight size={16} /></Link>)}</div> : <EmptyState icon={CheckCircle2} title="Không có thông báo mới" description="Các thay đổi điểm, báo cáo và trạng thái hệ thống sẽ xuất hiện tại đây." />}</Card>
      <Card className="next-actions"><CardHeader title="Việc tiếp theo" description="Lối tắt theo đúng vai trò" /><div className="action-list">{roleActions(role).map((action) => <Link key={action.href} href={action.href}><span><action.icon size={18} /></span><div><b>{action.title}</b><p>{action.copy}</p></div><ArrowRight size={16} /></Link>)}</div></Card>
    </section>
  </div>;
}

/** notificationHref tạo deep-link đúng role từ notification có student/assignment. */
function notificationHref(item: NotificationDto, role: UserRole) {
  if (item.type.startsWith("Report")) return "/reports";
  if (item.studentId && item.assignmentId && role === "Parent") return `/parent/children/${item.studentId}/grades?assignmentId=${item.assignmentId}`;
  if (item.studentId && item.assignmentId && role === "Student") return `/student/grades?studentId=${item.studentId}&assignmentId=${item.assignmentId}`;
  return "/notifications";
}

/** roleActions định nghĩa next action ngắn theo role, không thêm module backend chưa có. */
function roleActions(role: UserRole) {
  const map = {
    Parent: [{ title: "Yêu cầu báo cáo", copy: "Chọn con và học kỳ để gửi học vụ duyệt.", href: "/reports", icon: FileText }],
    Student: [{ title: "Xem thông báo mới", copy: "Mở đúng kết quả được nhà trường công bố.", href: "/notifications", icon: Bell }],
    Teacher: [{ title: "Mở sổ điểm", copy: "Nhập điểm và nhận xét theo lớp được phân công.", href: "/teacher/classes", icon: BookOpenCheck }],
    AcademicAdmin: [{ title: "Duyệt báo cáo", copy: "Xử lý yêu cầu PDF từ phụ huynh.", href: "/reports", icon: FileText }, { title: "Quản lý học sinh", copy: "Tạo, cập nhật và liên kết phụ huynh.", href: "/academic/students", icon: Users }, { title: "Lớp và ghi danh", copy: "Tổ chức lớp và thêm học sinh.", href: "/academic/classes", icon: Boxes }],
    SystemAdmin: [{ title: "Tài khoản & phân quyền", copy: "Quản lý giáo viên, phụ huynh và quản trị viên.", href: "/admin/people", icon: Users }, { title: "Sức khỏe hệ thống", copy: "Kiểm tra Postgres, Redis, Mongo và Ministry.", href: "/admin/system-health", icon: HeartPulse }],
  } satisfies Record<UserRole, { title: string; copy: string; href: string; icon: typeof School }[]>;
  return map[role];
}
