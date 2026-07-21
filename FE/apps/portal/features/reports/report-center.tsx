"use client";

import { Badge, Button, Card, CardHeader, DataTable, Dialog, EmptyState, Field, Select, type DataColumn } from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Download, FileClock, FileText, ShieldCheck, X } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { ChildSummaryDto, Envelope, PagedEnvelope, ReportRequestDto, SemesterDto } from "@/lib/domain";
import { formatDateTime, statusLabel, statusTone } from "@/lib/domain";

/** ReportCenter xử lý luồng phụ huynh gửi yêu cầu và quản trị học vụ duyệt báo cáo PDF. */
export function ReportCenter({ initialStudentId = "" }: { initialStudentId?: string }) {
  const { request, user } = useSession();
  const queryClient = useQueryClient();
  const staff = user?.role === "AcademicAdmin" || user?.role === "SystemAdmin";
  const [studentId, setStudentId] = useState(initialStudentId);
  const [semesterId, setSemesterId] = useState("");
  const [purpose, setPurpose] = useState("");
  const [status, setStatus] = useState("");
  const [reviewing, setReviewing] = useState<ReportRequestDto | null>(null);
  const [reviewNote, setReviewNote] = useState("");
  const semesters = useQuery({ queryKey: ["semesters", "report"], queryFn: () => request<PagedEnvelope<SemesterDto>>("/api/v1/semesters?page=1&pageSize=100") });
  const children = useQuery({ queryKey: ["children", "report"], queryFn: () => request<Envelope<ChildSummaryDto[]>>("/api/v1/me/children"), enabled: user?.role === "Parent" });
  const requestQuery = new URLSearchParams({ page: "1", pageSize: "100", ...(status ? { status } : {}) }).toString();
  const reportRequests = useQuery({ queryKey: ["report-requests", status, user?.role], queryFn: () => request<PagedEnvelope<ReportRequestDto>>(`/api/v1/reports/requests?${requestQuery}`), enabled: Boolean(user), refetchInterval: (query) => { const items = (query.state.data as PagedEnvelope<ReportRequestDto> | undefined)?.data.items || []; return items.some((item) => ["Approved", "Generating"].includes(item.status) || ["Queued", "Processing"].includes(item.jobStatus || "")) ? 3000 : false; } });
  const createRequest = useMutation({ mutationFn: () => request<Envelope<ReportRequestDto>>("/api/v1/reports/requests", { method: "POST", body: JSON.stringify({ studentId, semesterId, purpose }) }), onSuccess: async () => { toast.success("Đã gửi yêu cầu tới quản trị học vụ."); setPurpose(""); await queryClient.invalidateQueries({ queryKey: ["report-requests"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const reviewRequest = useMutation({ mutationFn: (approve: boolean) => { if (!reviewing) throw new Error("Chưa chọn yêu cầu."); return request<Envelope<ReportRequestDto>>(`/api/v1/reports/requests/${reviewing.id}/review`, { method: "PUT", body: JSON.stringify({ approve, note: reviewNote || null }) }); }, onSuccess: async (result) => { toast.success(result.data.status === "Rejected" ? "Đã từ chối yêu cầu." : "Đã duyệt và đưa báo cáo vào hàng đợi."); setReviewing(null); setReviewNote(""); await queryClient.invalidateQueries({ queryKey: ["report-requests"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const download = useCallback(async (jobId: string, studentCode: string) => { try { const response = await request<Response>(`/api/v1/reports/jobs/${jobId}/download`, { raw: true }); const blob = await response.blob(); const url = URL.createObjectURL(blob); const link = document.createElement("a"); link.href = url; link.download = `EduHub-${studentCode}-report.pdf`; link.click(); URL.revokeObjectURL(url); } catch (error) { toast.error(sessionErrorMessage(error)); } }, [request]);
  const rows = reportRequests.data?.data.items || [];
  const columns = useMemo<DataColumn<ReportRequestDto>[]>(() => [
    { key: "student", header: "Học sinh", cell: (row) => <div className="entity-cell"><span><FileText size={17} /></span><div><b>{row.studentName}</b><small>{row.studentCode} · {row.semesterName}</small></div></div> },
    { key: "requester", header: "Người yêu cầu", cell: (row) => row.requesterName },
    { key: "purpose", header: "Mục đích", cell: (row) => <span className="report-purpose">{row.purpose}</span> },
    { key: "requested", header: "Thời điểm", cell: (row) => formatDateTime(row.requestedAtUtc) },
    { key: "status", header: "Trạng thái", cell: (row) => <div><Badge tone={statusTone(row.jobStatus || row.status)}>{statusLabel(row.jobStatus || row.status)}</Badge>{row.reviewNote ? <small className="table-subtext">{row.reviewNote}</small> : null}</div> },
    { key: "actions", header: "", className: "table-action", cell: (row) => <div className="row-actions">{staff && ["Pending", "Reviewing"].includes(row.status) ? <Button variant="outline" onClick={() => { setReviewing(row); setReviewNote(""); }}><ShieldCheck size={15} /> Duyệt</Button> : null}{row.reportJobId && (row.jobStatus === "Completed" || row.status === "Completed") ? <Button variant="ghost" onClick={() => void download(row.reportJobId!, row.studentCode)}><Download size={15} /> PDF</Button> : null}</div> },
  ], [download, staff]);
  const loading = semesters.isLoading || children.isLoading || reportRequests.isLoading;
  const error = semesters.error || children.error || reportRequests.error;
  if (loading) return <><PageHeader eyebrow="BÁO CÁO HỌC TẬP" title={staff ? "Duyệt yêu cầu báo cáo" : "Báo cáo của con"} /><LoadingPanel rows={8} /></>;
  if (error) return <><PageHeader eyebrow="BÁO CÁO HỌC TẬP" title={staff ? "Duyệt yêu cầu báo cáo" : "Báo cáo của con"} /><ErrorPanel message={sessionErrorMessage(error)} onRetry={() => { void semesters.refetch(); void children.refetch(); void reportRequests.refetch(); }} /></>;
  return <div className="page-stack"><PageHeader eyebrow={staff ? "INBOX HỌC VỤ" : "BÁO CÁO HỌC TẬP"} title={staff ? "Duyệt yêu cầu báo cáo" : "Yêu cầu báo cáo của con"} description={staff ? "Học vụ kiểm tra học sinh, học kỳ và mục đích trước khi tạo PDF chính thức." : "Chọn đúng con và học kỳ đã có điểm công bố; học vụ sẽ duyệt, hệ thống tạo PDF rồi thông báo để tải."} />
    {!staff ? <Card><CardHeader title="Gửi yêu cầu mới" description="Yêu cầu được chuyển tới quản trị học vụ, không tạo PDF ngay lập tức." /><div className="report-form"><div className="form-grid-2"><Field label="Học sinh" htmlFor="reportStudent" required><Select id="reportStudent" value={studentId} onChange={(event) => { const childId = event.target.value; setStudentId(childId); setSemesterId(children.data?.data.find((child) => child.id === childId)?.currentSemesterId || ""); }}><option value="">Chọn con</option>{children.data?.data.map((child) => <option key={child.id} value={child.id}>{child.fullName} · {child.currentClassName || "Chưa xếp lớp"}</option>)}</Select></Field><Field label="Học kỳ" htmlFor="reportSemester" required><Select id="reportSemester" value={semesterId} onChange={(event) => setSemesterId(event.target.value)}><option value="">Chọn học kỳ</option>{semesters.data?.data.items.map((semester) => <option key={semester.id} value={semester.id}>{semester.name}</option>)}</Select></Field></div><Field label="Mục đích sử dụng" htmlFor="reportPurpose" required><textarea id="reportPurpose" className="ui-textarea" value={purpose} maxLength={500} onChange={(event) => setPurpose(event.target.value)} placeholder="Ví dụ: theo dõi kết quả học kỳ, bổ sung hồ sơ học tập..." /></Field><Button onClick={() => createRequest.mutate()} disabled={!studentId || !semesterId || purpose.trim().length < 3} loading={createRequest.isPending}><FileText size={16} /> Gửi quản trị học vụ</Button></div></Card> : null}
    <Card><CardHeader title={staff ? "Yêu cầu cần xử lý" : "Lịch sử yêu cầu"} description={`${reportRequests.data?.data.totalCount || 0} yêu cầu`} action={staff ? <Select value={status} onChange={(event) => setStatus(event.target.value)} aria-label="Lọc trạng thái báo cáo"><option value="">Tất cả trạng thái</option><option value="Pending">Chờ duyệt</option><option value="Approved">Đã duyệt</option><option value="Rejected">Từ chối</option><option value="Completed">Hoàn tất</option><option value="Failed">Lỗi</option></Select> : undefined} /><DataTable columns={columns} rows={rows} rowKey={(row) => row.id} empty={<EmptyState icon={FileClock} title={staff ? "Không có yêu cầu cần xử lý" : "Chưa gửi yêu cầu báo cáo"} description={staff ? "Các yêu cầu mới từ phụ huynh sẽ xuất hiện tại đây." : "Chọn con, học kỳ và nêu rõ mục đích sử dụng báo cáo."} />} /></Card>
    <Dialog open={Boolean(reviewing)} onOpenChange={(open) => { if (!open) setReviewing(null); }} title="Duyệt yêu cầu báo cáo" description={reviewing ? `${reviewing.studentName} · ${reviewing.semesterName}` : undefined} footer={<><Button variant="danger" onClick={() => reviewRequest.mutate(false)} loading={reviewRequest.isPending} disabled={!reviewNote.trim()}><X size={16} /> Từ chối</Button><Button onClick={() => reviewRequest.mutate(true)} loading={reviewRequest.isPending}><Check size={16} /> Duyệt & tạo PDF</Button></>}><div className="review-request"><dl className="detail-grid"><div><dt>Phụ huynh</dt><dd>{reviewing?.requesterName}</dd></div><div><dt>Yêu cầu lúc</dt><dd>{formatDateTime(reviewing?.requestedAtUtc)}</dd></div></dl><div><b>Mục đích</b><p>{reviewing?.purpose}</p></div><Field label="Ghi chú duyệt / lý do từ chối" htmlFor="reviewNote"><textarea id="reviewNote" className="ui-textarea" value={reviewNote} onChange={(event) => setReviewNote(event.target.value)} placeholder="Bắt buộc khi từ chối" /></Field></div></Dialog>
  </div>;
}
