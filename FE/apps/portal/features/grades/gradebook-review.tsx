"use client";

import { Badge, Card, CardHeader, DataTable, EmptyState, Select, type DataColumn } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { ArrowRight, BookOpenCheck } from "lucide-react";
import Link from "next/link";
import { useMemo, useState } from "react";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, TeachingAssignmentSummaryDto } from "@/lib/domain";
import { statusLabel, statusTone } from "@/lib/domain";

/** GradebookReview liệt kê sổ điểm để quản trị học vụ duyệt, công bố, mở lại hoặc khóa. */
export function GradebookReview() {
  const { request } = useSession();
  const [status, setStatus] = useState("");
  const assignments = useQuery({ queryKey: ["teaching-assignments", "academic-review"], queryFn: () => request<Envelope<TeachingAssignmentSummaryDto[]>>("/api/v1/teaching-assignments") });
  const rows = (assignments.data?.data || []).filter((item) => !status || item.gradebookStatus === status);
  const columns = useMemo<DataColumn<TeachingAssignmentSummaryDto>[]>(() => [
    { key: "subject", header: "Môn học", cell: (row) => <div className="entity-cell"><span><BookOpenCheck size={17} /></span><div><b>{row.subjectName}</b><small>{row.subjectCode} · {row.semesterName}</small></div></div> },
    { key: "class", header: "Lớp", cell: (row) => <div><b>{row.className}</b><small className="table-subtext">{row.studentCount} học sinh</small></div> },
    { key: "teacher", header: "Giáo viên", cell: (row) => row.teacherName },
    { key: "status", header: "Sổ điểm", cell: (row) => <Badge tone={statusTone(row.gradebookStatus)}>{statusLabel(row.gradebookStatus)}</Badge> },
    { key: "action", header: "", className: "table-action", cell: (row) => <Link className="ui-button ui-button--ghost" href={`/academic/assignments/${row.id}/gradebook`}>Mở sổ <ArrowRight size={15} /></Link> },
  ], []);
  if (assignments.isLoading) return <><PageHeader eyebrow="ACADEMIC WORKFLOW" title="Duyệt & Công bố điểm" /><LoadingPanel rows={8} /></>;
  if (assignments.error) return <><PageHeader eyebrow="ACADEMIC WORKFLOW" title="Duyệt & Công bố điểm" /><ErrorPanel message={sessionErrorMessage(assignments.error)} onRetry={() => assignments.refetch()} /></>;
  return <div className="page-stack"><PageHeader eyebrow="ACADEMIC WORKFLOW" title="Duyệt & Công bố điểm" description="Teacher submit -> AcademicAdmin review -> publish/lock -> Parent notification." /><Card><CardHeader title="Danh sách sổ điểm" description={`${rows.length} phân công phù hợp`} action={<Select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">Tất cả trạng thái</option><option value="Draft">Bản nháp</option><option value="Submitted">Chờ duyệt</option><option value="Published">Đã công bố</option><option value="Locked">Đã khóa</option><option value="Reopened">Đang điều chỉnh</option></Select>} /><DataTable columns={columns} rows={rows} rowKey={(row) => row.id} empty={<EmptyState icon={BookOpenCheck} title="Không có sổ điểm phù hợp" description="Tạo phân công giáo viên trong màn hình quản lý lớp." />} /></Card></div>;
}
