"use client";

import { Badge, Button, Card, CardHeader, Dialog, EmptyState, Field, Input } from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, BookOpenCheck, CheckCircle2, LockKeyhole, MessageSquareText, RotateCcw, Save, Send } from "lucide-react";
import Link from "next/link";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, GradebookDto } from "@/lib/domain";
import { statusLabel, statusTone } from "@/lib/domain";

/** TeacherGradebook cho giáo viên nhập điểm và nhận xét học sinh theo assignment được phân công. */
export function TeacherGradebook({ assignmentId }: { assignmentId: string }) {
  const { request, user } = useSession();
  const queryClient = useQueryClient();
  const [scores, setScores] = useState<Record<string, string>>({});
  const [remarks, setRemarks] = useState<Record<string, string>>({});
  const [reopenOpen, setReopenOpen] = useState(false);
  const [reopenReason, setReopenReason] = useState("");
  const gradebook = useQuery({ queryKey: ["gradebook", assignmentId], queryFn: () => request<Envelope<GradebookDto>>(`/api/v1/assignments/${assignmentId}/gradebook`) });
  const saveGrades = useMutation({ mutationFn: async () => {
    const data = gradebook.data?.data;
    if (!data) throw new Error("Sổ điểm chưa sẵn sàng.");
    const items = data.students.flatMap((student) => student.grades.flatMap((grade) => { const value = scores[`${student.studentId}:${grade.componentId}`]; return value === "" || value === undefined ? [] : [{ studentId: student.studentId, componentId: grade.componentId, score: Number(value), version: grade.version, reason: "Teacher gradebook bulk save" }]; }));
    return request<Envelope<{ successCount: number; errorCount: number }>>(`/api/v1/assignments/${assignmentId}/grades/bulk`, { method: "PUT", body: JSON.stringify({ atomic: true, items }) });
  }, onSuccess: async (result) => { toast.success(`Đã lưu ${result.data.successCount} ô điểm.`); setScores({}); await queryClient.invalidateQueries({ queryKey: ["gradebook", assignmentId] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const saveRemark = useMutation({ mutationFn: ({ studentId }: { studentId: string }) => { const student = gradebook.data?.data.students.find((item) => item.studentId === studentId); return request<Envelope<unknown>>(`/api/v1/assignments/${assignmentId}/students/${studentId}/remark`, { method: "PUT", body: JSON.stringify({ content: remarks[studentId] ?? student?.remark ?? "", version: student?.remarkVersion ?? null }) }); }, onSuccess: async (_, { studentId }) => { toast.success("Đã lưu nhận xét học sinh."); setRemarks((current) => { const next = { ...current }; delete next[studentId]; return next; }); await queryClient.invalidateQueries({ queryKey: ["gradebook", assignmentId] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const submit = useMutation({ mutationFn: () => request<Envelope<unknown>>(`/api/v1/assignments/${assignmentId}/grades/submit`, { method: "POST" }), onSuccess: async () => { toast.success("Đã nộp sổ điểm cho quản trị học vụ."); await queryClient.invalidateQueries({ queryKey: ["gradebook", assignmentId] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const changeState = useMutation({ mutationFn: ({ action, body }: { action: "publish" | "reopen" | "lock"; body?: object }) => request<Envelope<unknown>>(`/api/v1/assignments/${assignmentId}/grades/${action}`, { method: "POST", ...(body ? { body: JSON.stringify(body) } : {}) }), onSuccess: async (_, variables) => { toast.success(variables.action === "publish" ? "Đã công bố điểm cho phụ huynh." : variables.action === "lock" ? "Đã khóa sổ điểm." : "Đã mở lại sổ điểm cho giáo viên chỉnh sửa."); setReopenOpen(false); setReopenReason(""); await queryClient.invalidateQueries({ queryKey: ["gradebook", assignmentId] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const changedCount = useMemo(() => {
    const data = gradebook.data?.data;
    if (!data) return 0;
    return data.students.reduce((count, student) => count + student.grades.filter((grade) => { const value = scores[`${student.studentId}:${grade.componentId}`]; return value !== undefined && (grade.score?.toString() || "") !== value; }).length, 0);
  }, [gradebook.data, scores]);
  if (gradebook.isLoading) return <><PageHeader eyebrow="SỔ ĐIỂM" title="Nhập điểm & Nhận xét" /><LoadingPanel rows={8} /></>;
  if (gradebook.error) return <><PageHeader eyebrow="SỔ ĐIỂM" title="Nhập điểm & Nhận xét" /><ErrorPanel message={sessionErrorMessage(gradebook.error)} onRetry={() => gradebook.refetch()} /></>;
  const data = gradebook.data?.data;
  if (!data) return <Card><EmptyState icon={BookOpenCheck} title="Không có sổ điểm" description="Phân công giảng dạy chưa có cấu hình điểm hoặc danh sách lớp." /></Card>;
  const teacherCanEdit = user?.role === "Teacher" && (data.status === "Draft" || data.status === "Reopened");
  const staff = user?.role === "AcademicAdmin" || user?.role === "SystemAdmin";
  const backHref = user?.role === "Teacher" ? "/teacher/classes" : "/academic/gradebooks";
  return <div className="page-stack"><PageHeader eyebrow={staff ? "DUYỆT SỔ ĐIỂM" : "SỔ ĐIỂM GIÁO VIÊN"} title={`${data.subjectName} · ${data.className}`} description={`${data.semesterName} · ${data.teacherName} · ${data.students.length} học sinh`} actions={<><Link className="ui-button ui-button--ghost" href={backHref}><ArrowLeft size={16} /> Danh sách sổ điểm</Link><Badge tone={statusTone(data.status)}>{statusLabel(data.status)}</Badge></>} />
    {staff ? <div className="gradebook-review-bar"><span><b>Academic workflow</b><small>Teacher submit → AcademicAdmin publish → Parent notification</small></span><div>{data.status === "Submitted" ? <Button onClick={() => changeState.mutate({ action: "publish" })} loading={changeState.isPending}><Send size={16} /> Công bố điểm</Button> : null}{data.status === "Published" || data.status === "Locked" ? <Button variant="outline" onClick={() => setReopenOpen(true)}><RotateCcw size={16} /> Mở lại</Button> : null}{data.status === "Published" ? <Button variant="outline" onClick={() => changeState.mutate({ action: "lock" })} loading={changeState.isPending}><LockKeyhole size={16} /> Khóa sổ</Button> : null}</div></div> : null}
    <Card><CardHeader title="Bảng điểm lớp" description={teacherCanEdit ? "Điểm được lưu ở trạng thái nháp cho đến khi giáo viên nộp sổ." : "Sổ điểm đang ở chế độ chỉ đọc theo trạng thái và vai trò hiện tại."} action={teacherCanEdit ? <div className="gradebook-actions"><span>{changedCount} thay đổi chưa lưu</span><Button variant="outline" onClick={() => saveGrades.mutate()} loading={saveGrades.isPending} disabled={!changedCount}><Save size={16} /> Lưu điểm</Button><Button onClick={() => submit.mutate()} loading={submit.isPending} disabled={Boolean(changedCount)}><CheckCircle2 size={16} /> Nộp sổ điểm</Button></div> : undefined} /><div className="gradebook-wrap"><table className="gradebook-table"><thead><tr><th>Học sinh</th>{data.components.map((component) => <th key={component.id}><b>{component.name}</b><small>{component.weight}% · tối đa {component.maxScore}</small></th>)}<th>Nhận xét môn học</th></tr></thead><tbody>{data.students.map((student) => <tr key={student.studentId}><td><b>{student.fullName}</b><small>{student.studentCode}</small></td>{data.components.map((component) => { const cell = student.grades.find((grade) => grade.componentId === component.id); const key = `${student.studentId}:${component.id}`; return <td key={component.id}><Input type="number" min="0" max={component.maxScore} step="0.01" value={scores[key] ?? cell?.score?.toString() ?? ""} disabled={!teacherCanEdit} aria-label={`${component.name} của ${student.fullName}`} onChange={(event) => setScores((current) => ({ ...current, [key]: event.target.value }))} /><small>{cell?.status ? statusLabel(cell.status) : "Chưa nhập"}</small></td>; })}<td><div className="remark-editor"><textarea value={remarks[student.studentId] ?? student.remark ?? ""} disabled={!teacherCanEdit} maxLength={1000} aria-label={`Nhận xét ${student.fullName}`} onChange={(event) => setRemarks((current) => ({ ...current, [student.studentId]: event.target.value }))} placeholder="Nhận xét học tập, tiến bộ hoặc nội dung cần lưu ý" />{teacherCanEdit ? <button className="ui-icon-button" title="Lưu nhận xét" aria-label={`Lưu nhận xét ${student.fullName}`} onClick={() => saveRemark.mutate({ studentId: student.studentId })} disabled={saveRemark.isPending || (student.remark ?? "") === (remarks[student.studentId] ?? student.remark ?? "")}><MessageSquareText size={16} /></button> : null}</div></td></tr>)}</tbody></table></div></Card>
    <Dialog open={reopenOpen} onOpenChange={setReopenOpen} title="Mở lại sổ điểm" description="Điểm sẽ trở về trạng thái giáo viên có thể chỉnh sửa." footer={<><Button variant="ghost" onClick={() => setReopenOpen(false)}>Hủy</Button><Button onClick={() => changeState.mutate({ action: "reopen", body: { reason: reopenReason } })} disabled={reopenReason.trim().length < 3} loading={changeState.isPending}><RotateCcw size={16} /> Xác nhận mở lại</Button></>}><Field label="Lý do mở lại" htmlFor="reopenReason" required><Input id="reopenReason" value={reopenReason} onChange={(event) => setReopenReason(event.target.value)} /></Field></Dialog>
  </div>;
}
