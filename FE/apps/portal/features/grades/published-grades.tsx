"use client";

import { Badge, Card, CardHeader, DataTable, EmptyState, type DataColumn } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { BookOpenCheck, FileText, School, UserRound } from "lucide-react";
import Link from "next/link";
import { useMemo } from "react";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { FeatureGap } from "@/components/feature-gap";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, PublishedGradebookDto } from "@/lib/domain";
import { formatDateTime } from "@/lib/domain";

type GradeRow = PublishedGradebookDto["grades"][number];

/** PublishedGrades hiển thị duy nhất điểm Published mà backend xác nhận parent ownership. */
export function PublishedGrades({ studentId, assignmentId = "", studentMode = false }: { studentId?: string; assignmentId?: string; studentMode?: boolean }) {
  const routeStudentId = studentId || "";
  const { request } = useSession();
  const grades = useQuery({ queryKey: ["published-grades", routeStudentId, assignmentId], queryFn: () => request<Envelope<PublishedGradebookDto>>(`/api/v1/assignments/${assignmentId}/students/${routeStudentId}/grades/published`), enabled: Boolean(routeStudentId && assignmentId && !studentMode) });
  const columns = useMemo<DataColumn<GradeRow>[]>(() => [{ key: "component", header: "Thành phần điểm", cell: (row) => <div className="entity-cell"><span><BookOpenCheck size={17} /></span><div><b>{row.componentName}</b><small>Trọng số {row.weight}% · phiên bản {row.publicationVersion}</small></div></div> }, { key: "score", header: "Điểm", cell: (row) => <strong className="grade-score">{row.score}<small>/{row.maxScore}</small></strong> }, { key: "status", header: "Trạng thái", cell: () => <Badge tone="success">Đã công bố</Badge> }], []);
  if (studentMode) return <div className="page-stack"><PageHeader eyebrow="KẾT QUẢ HỌC TẬP" title="Điểm số" description="Chỉ hiển thị điểm đã được nhà trường công bố." /><Card className="gap-card"><FeatureGap title="Backend chưa có endpoint điểm dành cho Student" description="API Published hiện chỉ cho role Parent. Portal không gọi endpoint Parent bằng tài khoản Student hoặc tự suy đoán studentId." gap="FE-GAP: GET /me/student/grades" /></Card></div>;
  if (!routeStudentId || !assignmentId) return <div className="page-stack"><PageHeader eyebrow="KẾT QUẢ HỌC TẬP" title="Điểm đã công bố" description="Mở từ một thông báo điểm mới để giữ đúng học sinh và assignment." /><Card><EmptyState icon={BookOpenCheck} title="Chưa chọn kết quả" description="Khi có điểm mới, hãy mở thông báo tương ứng. EduHub sẽ dẫn tới đúng học sinh và sổ điểm mà không yêu cầu nhập ID." /></Card></div>;
  if (grades.isLoading) return <><PageHeader eyebrow="KẾT QUẢ HỌC TẬP" title="Điểm đã công bố" /><LoadingPanel rows={6} /></>;
  if (grades.error) return <><PageHeader eyebrow="KẾT QUẢ HỌC TẬP" title="Điểm đã công bố" /><ErrorPanel message={sessionErrorMessage(grades.error)} onRetry={() => grades.refetch()} /></>;
  const result = grades.data?.data;
  const rows = result?.grades || [];
  return <div className="page-stack"><PageHeader eyebrow="KẾT QUẢ HỌC TẬP" title={result ? `${result.subjectName} · ${result.studentName}` : "Điểm đã công bố"} description={result ? `${result.className} · ${result.semesterName} · Giáo viên ${result.teacherName}` : "Điểm nháp hoặc dữ liệu đang điều chỉnh không hiển thị tại đây."} actions={<Link className="ui-button ui-button--outline" href={`/reports?studentId=${routeStudentId}`}><FileText size={16} /> Yêu cầu báo cáo</Link>} />{result ? <section className="grade-context"><div><span><UserRound size={18} /></span><small>Học sinh</small><b>{result.studentName}</b><p>{result.studentCode}</p></div><div><span><School size={18} /></span><small>Lớp · Môn học</small><b>{result.className}</b><p>{result.subjectName} ({result.subjectCode})</p></div><div><span><BookOpenCheck size={18} /></span><small>Công bố</small><b>{result.semesterName}</b><p>{formatDateTime(result.publishedAtUtc)}</p></div></section> : null}<Card><CardHeader title="Thành phần điểm" description={`${rows.length} thành phần đã công bố`} /><DataTable columns={columns} rows={rows} rowKey={(row) => row.componentId} empty={<EmptyState icon={BookOpenCheck} title="Chưa có điểm được công bố" description="Điểm Draft hoặc Submitted sẽ không xuất hiện ở portal gia đình." />} />{result?.remark ? <div className="published-remark"><b>Nhận xét của giáo viên</b><p>{result.remark}</p></div> : null}</Card></div>;
}
