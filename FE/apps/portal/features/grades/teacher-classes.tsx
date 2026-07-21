"use client";

import { Badge, Card, EmptyState } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { ArrowRight, BookOpenCheck, Users } from "lucide-react";
import Link from "next/link";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, TeachingAssignmentSummaryDto } from "@/lib/domain";
import { statusLabel, statusTone } from "@/lib/domain";

/** TeacherClasses hiển thị đúng lớp-môn-học kỳ thuộc giáo viên đang đăng nhập. */
export function TeacherClasses() {
  const { request } = useSession();
  const assignments = useQuery({ queryKey: ["teaching-assignments"], queryFn: () => request<Envelope<TeachingAssignmentSummaryDto[]>>("/api/v1/me/teaching-assignments") });
  if (assignments.isLoading) return <><PageHeader eyebrow="KHÔNG GIAN GIÁO VIÊN" title="Lớp được phân công" /><LoadingPanel rows={6} /></>;
  if (assignments.error) return <><PageHeader eyebrow="KHÔNG GIAN GIÁO VIÊN" title="Lớp được phân công" /><ErrorPanel message={sessionErrorMessage(assignments.error)} onRetry={() => assignments.refetch()} /></>;
  const items = assignments.data?.data || [];
  return <div className="page-stack"><PageHeader eyebrow="KHÔNG GIAN GIÁO VIÊN" title="Lớp & Sổ điểm" description="Mỗi phân công xác định một lớp, môn học và học kỳ giáo viên được phép cập nhật." />{items.length ? <section className="assignment-grid">{items.map((item) => <Link key={item.id} href={`/teacher/assignments/${item.id}/gradebook`} className="assignment-card"><div><span>{item.subjectCode}</span><Badge tone={statusTone(item.gradebookStatus)}>{statusLabel(item.gradebookStatus)}</Badge></div><h3>{item.subjectName}</h3><p>{item.className} · {item.semesterName}</p><small><Users size={14} /> {item.studentCount} học sinh</small><ArrowRight size={18} /></Link>)}</section> : <Card><EmptyState icon={BookOpenCheck} title="Chưa có lớp được phân công" description="Quản trị học vụ cần phân công giáo viên với lớp, môn và học kỳ." /></Card>}</div>;
}
