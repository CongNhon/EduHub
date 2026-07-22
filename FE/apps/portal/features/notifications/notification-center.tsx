"use client";

import { Badge, Button, Card, CardHeader, EmptyState } from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bell, BookOpenCheck, Check, CheckCheck, CheckCircle2, FileText, RefreshCw } from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, NotificationDto, PagedEnvelope } from "@/lib/domain";
import { formatDateTime } from "@/lib/domain";

type NotificationFilter = "all" | "unread" | "grades" | "reports";

/** NotificationCenter hiển thị inbox, deep-link và optimistic mark-read có rollback. */
export function NotificationCenter() {
  const { request, user } = useSession();
  const queryClient = useQueryClient();
  const [filter, setFilter] = useState<NotificationFilter>("all");
  const unreadQuery = filter === "unread" ? "&isRead=false" : "";
  const notifications = useQuery({ queryKey: ["notifications", { filter }], queryFn: () => request<PagedEnvelope<NotificationDto>>(`/api/v1/notifications?page=1&pageSize=50${unreadQuery}`) });
  const markRead = useMutation({ mutationFn: (id: string) => request<Envelope<NotificationDto>>(`/api/v1/notifications/${id}/read`, { method: "PUT" }), onMutate: async (id) => { await queryClient.cancelQueries({ queryKey: ["notifications"] }); const snapshots = queryClient.getQueriesData({ queryKey: ["notifications"] }); queryClient.setQueriesData<PagedEnvelope<NotificationDto>>({ queryKey: ["notifications"] }, (old) => old ? { ...old, data: { ...old.data, items: old.data.items.map((item) => item.id === id ? { ...item, isRead: true, readAtUtc: new Date().toISOString() } : item) } } : old); return { snapshots }; }, onError: (error, _id, context) => { context?.snapshots.forEach(([key, data]) => queryClient.setQueryData(key, data)); toast.error(sessionErrorMessage(error)); }, onSettled: () => queryClient.invalidateQueries({ queryKey: ["notifications"] }) });
  const markAllRead = useMutation({ mutationFn: (ids: string[]) => Promise.all(ids.map((id) => request<Envelope<NotificationDto>>(`/api/v1/notifications/${id}/read`, { method: "PUT" }))), onSuccess: async () => { toast.success("Đã đánh dấu tất cả thông báo là đã đọc."); await queryClient.invalidateQueries({ queryKey: ["notifications"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  if (notifications.isLoading) return <><PageHeader eyebrow="TRUNG TÂM HOẠT ĐỘNG" title="Thông báo" /><LoadingPanel rows={8} /></>;
  if (notifications.error) return <><PageHeader eyebrow="TRUNG TÂM HOẠT ĐỘNG" title="Thông báo" /><ErrorPanel message={sessionErrorMessage(notifications.error)} onRetry={() => notifications.refetch()} /></>;
  const allItems = notifications.data?.data.items || [];
  const items = allItems.filter((item) => filter === "grades" ? item.type.toLowerCase().includes("grade") : filter === "reports" ? item.type.toLowerCase().includes("report") : true);
  const unread = allItems.filter((item) => !item.isRead).length;
  return <div className="page-stack"><PageHeader eyebrow="TRUNG TÂM HOẠT ĐỘNG" title="Thông báo" description="Điểm số, yêu cầu báo cáo và cảnh báo vận hành dành cho tài khoản của bạn." actions={<Button variant="outline" onClick={() => notifications.refetch()}><RefreshCw size={16} /> Làm mới</Button>} />
    <Card><CardHeader title="Hộp thư" description={`${unread} thông báo chưa đọc`} action={unread > 0 ? <Button variant="ghost" loading={markAllRead.isPending} onClick={() => markAllRead.mutate(allItems.filter((item) => !item.isRead).map((item) => item.id))}><CheckCheck size={16} /> Đọc tất cả</Button> : undefined} /><div className="notification-filters" role="tablist" aria-label="Lọc thông báo">{(["all", "unread", "grades", "reports"] as const).map((value) => <button role="tab" aria-selected={filter === value} key={value} onClick={() => setFilter(value)}>{value === "all" ? "Tất cả" : value === "unread" ? "Chưa đọc" : value === "grades" ? "Điểm số" : "Báo cáo"}</button>)}</div>{items.length ? <div className="notification-list">{items.map((item) => { const href = notificationLink(item, user?.role); const Icon = item.type.toLowerCase().includes("grade") ? BookOpenCheck : item.type.toLowerCase().includes("report") ? FileText : Bell; return <article key={item.id} className={!item.isRead ? "unread" : ""}><span className="notification-list__icon"><Icon size={18} /></span><div><header><b>{item.title}</b>{!item.isRead ? <Badge tone="info">Mới</Badge> : null}</header><p>{item.body}</p><small><span className="notification-category">{notificationCategory(item.type)}</span>{formatDateTime(item.occurredAtUtc)}</small></div><div className="notification-actions">{href ? <Link className="ui-button ui-button--outline" href={href}>Mở chi tiết</Link> : null}{!item.isRead ? <button className="ui-icon-button" title="Đánh dấu đã đọc" aria-label={`Đánh dấu đã đọc: ${item.title}`} onClick={() => markRead.mutate(item.id)}><Check size={17} /></button> : <CheckCircle2 size={18} className="read-check" />}</div></article>; })}</div> : <EmptyState icon={Bell} title="Không có thông báo" description="Không có nội dung phù hợp bộ lọc hiện tại." />}</Card>
  </div>;
}

/** notificationLink tạo deep-link có student và assignment khi payload đủ dữ liệu. */
function notificationLink(item: NotificationDto, role?: string) {
  if (item.studentId && item.assignmentId && role === "Parent") return `/parent/children/${item.studentId}/grades?assignmentId=${item.assignmentId}`;
  if (item.studentId && item.assignmentId && role === "Student") return `/student/grades?studentId=${item.studentId}&assignmentId=${item.assignmentId}`;
  if (item.assignmentId && role === "Teacher") return `/teacher/assignments/${item.assignmentId}/gradebook`;
  if (item.assignmentId && role === "AcademicAdmin") return `/academic/assignments/${item.assignmentId}/gradebook`;
  if (item.type.toLowerCase().includes("report") && role === "AcademicAdmin") return "/reports";
  return null;
}

/** notificationCategory đổi notification type kỹ thuật thành nhóm nội dung dễ đọc. */
function notificationCategory(type: string) {
  const normalized = type.toLowerCase();
  if (normalized.includes("grade")) return "Điểm số";
  if (normalized.includes("report")) return "Báo cáo";
  if (normalized.includes("sync") || normalized.includes("system")) return "Vận hành";
  return "Hệ thống";
}
