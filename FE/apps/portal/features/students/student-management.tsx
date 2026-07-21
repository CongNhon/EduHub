"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { Badge, Button, Card, CardHeader, DataTable, Dialog, EmptyState, Field, Input, Select, type DataColumn } from "@eduhub/ui";
import { studentSchema } from "@eduhub/validation";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, ChevronDown, ChevronLeft, ChevronRight, Eye, Link2, Mail, Pencil, Plus, Search, School, Unlink, UserRound, Users } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { ClassRoomDto, Envelope, PagedEnvelope, StudentDetailDto, StudentDto, UserAccountDto } from "@/lib/domain";
import { formatDate, statusLabel, statusTone } from "@/lib/domain";
import { useDebouncedValue } from "@/lib/use-debounced-value";

type StudentForm = z.infer<typeof studentSchema>;
const statusValues: Record<string, number> = { Active: 0, Suspended: 1, Graduated: 2, Withdrawn: 3 };

/** StudentManagement quản lý danh sách, tạo và cập nhật hồ sơ học sinh bằng bảng và dialog. */
export function StudentManagement() {
  const { request } = useSession();
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [classRoomId, setClassRoomId] = useState("");
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<StudentDto | null>(null);
  const [detailId, setDetailId] = useState("");
  const [parentUserId, setParentUserId] = useState("");
  const [relationship, setRelationship] = useState("Cha/Mẹ");
  const debouncedSearch = useDebouncedValue(search.trim(), 300);
  const queryString = new URLSearchParams({ page: String(page), pageSize: "10", ...(debouncedSearch ? { search: debouncedSearch } : {}), ...(status ? { status } : {}), ...(classRoomId ? { classRoomId } : {}) }).toString();
  const students = useQuery({ queryKey: ["students", { debouncedSearch, status, classRoomId, page }], queryFn: () => request<PagedEnvelope<StudentDto>>(`/api/v1/students?${queryString}`) });
  const classes = useQuery({ queryKey: ["classes", "student-filter"], queryFn: () => request<PagedEnvelope<ClassRoomDto>>("/api/v1/classes?page=1&pageSize=100") });
  const detail = useQuery({ queryKey: ["student-detail", detailId], queryFn: () => request<Envelope<StudentDetailDto>>(`/api/v1/students/${detailId}/detail`), enabled: Boolean(detailId) });
  const parents = useQuery({ queryKey: ["users", "parents", detailId], queryFn: () => request<PagedEnvelope<UserAccountDto>>("/api/v1/users?role=Parent&isActive=true&page=1&pageSize=100"), enabled: Boolean(detailId) });
  const createForm = useForm<StudentForm>({ resolver: zodResolver(studentSchema), defaultValues: { studentCode: "", fullName: "", dateOfBirth: "" } });
  const editForm = useForm<{ fullName: string; dateOfBirth: string; status: string }>({ defaultValues: { fullName: "", dateOfBirth: "", status: "Active" } });

  const createStudent = useMutation({ mutationFn: (values: StudentForm) => request<Envelope<StudentDto>>("/api/v1/students", { method: "POST", body: JSON.stringify(values) }), onSuccess: async () => { toast.success("Đã tạo hồ sơ học sinh."); setCreateOpen(false); createForm.reset(); await queryClient.invalidateQueries({ queryKey: ["students"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const updateStudent = useMutation({ mutationFn: (values: { fullName: string; dateOfBirth: string; status: string }) => { if (!editing) throw new Error("Chưa chọn học sinh."); return request<Envelope<StudentDto>>(`/api/v1/students/${editing.id}`, { method: "PUT", body: JSON.stringify({ fullName: values.fullName, dateOfBirth: values.dateOfBirth, status: statusValues[values.status], version: editing.version }) }); }, onSuccess: async () => { toast.success("Đã cập nhật hồ sơ học sinh."); setEditing(null); await queryClient.invalidateQueries({ queryKey: ["students"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const linkParent = useMutation({ mutationFn: () => request<Envelope<unknown>>(`/api/v1/students/${detailId}/parents/${parentUserId}`, { method: "POST", body: JSON.stringify({ relationship }) }), onSuccess: async () => { toast.success("Đã liên kết phụ huynh với học sinh."); setParentUserId(""); await Promise.all([queryClient.invalidateQueries({ queryKey: ["student-detail", detailId] }), queryClient.invalidateQueries({ queryKey: ["students"] })]); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const unlinkParent = useMutation({ mutationFn: (userId: string) => request<Envelope<unknown>>(`/api/v1/students/${detailId}/parents/${userId}`, { method: "DELETE" }), onSuccess: async () => { toast.success("Đã ngừng liên kết phụ huynh."); await Promise.all([queryClient.invalidateQueries({ queryKey: ["student-detail", detailId] }), queryClient.invalidateQueries({ queryKey: ["students"] })]); }, onError: (error) => toast.error(sessionErrorMessage(error)) });

  const rows = students.data?.data.items || [];
  const classItems = useMemo(() => classes.data?.data.items || [], [classes.data]);
  const columns = useMemo<DataColumn<StudentDto>[]>(() => [
    { key: "student", header: "Học sinh", cell: (row) => <div className="entity-cell"><span><UserRound size={17} /></span><div><b>{row.fullName}</b><small>{row.studentCode}</small></div></div> },
    { key: "dob", header: "Ngày sinh", cell: (row) => formatDate(row.dateOfBirth) },
    { key: "class", header: "Lớp", cell: (row) => <div><b>{row.currentClassName || "Chưa xếp lớp"}</b>{row.currentClassCode ? <small className="table-subtext">{row.currentClassCode}</small> : null}</div> },
    { key: "guardians", header: "Phụ huynh", cell: (row) => `${row.guardianCount} liên kết` },
    { key: "account", header: "Tài khoản", cell: (row) => row.accountEmail ? <div><b>{row.accountEmail}</b><small className="table-subtext">{row.accountIsActive ? "Đang hoạt động" : "Đã khóa"}</small></div> : "Chưa liên kết" },
    { key: "status", header: "Trạng thái", cell: (row) => <Badge tone={statusTone(row.status)}>{statusLabel(row.status)}</Badge> },
    { key: "action", header: "", className: "table-action", cell: (row) => <div className="row-actions"><button className="ui-icon-button" title={`Xem ${row.fullName}`} aria-label={`Xem chi tiết ${row.fullName}`} onClick={() => setDetailId(row.id)}><Eye size={16} /></button><button className="ui-icon-button" title={`Sửa ${row.fullName}`} aria-label={`Sửa hồ sơ ${row.fullName}`} onClick={() => { setEditing(row); editForm.reset({ fullName: row.fullName, dateOfBirth: row.dateOfBirth, status: row.status }); }}><Pencil size={16} /></button></div> },
  ], [editForm]);

  if (students.isLoading || classes.isLoading) return <><PageHeader eyebrow="HỌC SINH & PHỤ HUYNH" title="Hồ sơ học sinh" description="Tìm kiếm, tạo và cập nhật hồ sơ trong một danh sách có phân trang." /><LoadingPanel rows={9} /></>;
  if (students.error || classes.error) return <><PageHeader eyebrow="HỌC SINH & PHỤ HUYNH" title="Hồ sơ học sinh" /><ErrorPanel message={sessionErrorMessage(students.error || classes.error)} onRetry={() => { void students.refetch(); void classes.refetch(); }} /></>;

  return <div className="page-stack"><PageHeader eyebrow="HỌC SINH & PHỤ HUYNH" title="Hồ sơ học sinh" description="Tìm kiếm, tạo và cập nhật hồ sơ trong một danh sách có phân trang." actions={<Button onClick={() => setCreateOpen(true)}><Plus size={17} /> Thêm học sinh</Button>} />
    <Card><CardHeader title="Danh sách học sinh" description={`${students.data?.data.totalCount || 0} hồ sơ phù hợp`} />
      <div className="table-toolbar"><div className="search-box"><Search size={17} /><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder="Tìm tên, mã, lớp hoặc phụ huynh" aria-label="Tìm học sinh" /></div><ClassFilterPicker classes={classItems} value={classRoomId} onChange={(value) => { setClassRoomId(value); setPage(1); }} /><Select value={status} onChange={(event) => { setStatus(event.target.value); setPage(1); }} aria-label="Lọc trạng thái"><option value="">Tất cả trạng thái</option><option value="0">Đang hoạt động</option><option value="1">Tạm dừng</option><option value="2">Đã tốt nghiệp</option><option value="3">Đã thôi học</option></Select></div>
      <DataTable columns={columns} rows={rows} rowKey={(row) => row.id} empty={<EmptyState icon={Users} title="Chưa có học sinh" description="Thêm hồ sơ đầu tiên hoặc thay đổi điều kiện tìm kiếm." action={<Button onClick={() => setCreateOpen(true)}><Plus size={16} /> Thêm học sinh</Button>} />} />
      <div className="pagination"><span>Trang {students.data?.data.page || 1} / {Math.max(1, students.data?.data.totalPages || 1)}</span><div><button className="ui-icon-button" onClick={() => setPage((value) => Math.max(1, value - 1))} disabled={page <= 1} aria-label="Trang trước"><ChevronLeft size={17} /></button><button className="ui-icon-button" onClick={() => setPage((value) => value + 1)} disabled={page >= (students.data?.data.totalPages || 1)} aria-label="Trang sau"><ChevronRight size={17} /></button></div></div>
    </Card>

    <Dialog open={createOpen} onOpenChange={setCreateOpen} title="Thêm học sinh" description="Tạo hồ sơ cơ bản trước khi ghi danh vào lớp." footer={<><Button variant="ghost" onClick={() => setCreateOpen(false)}>Hủy</Button><Button form="create-student-form" type="submit" loading={createStudent.isPending}>Tạo hồ sơ</Button></>}><form id="create-student-form" className="dialog-form" onSubmit={createForm.handleSubmit((values) => createStudent.mutate(values))}><Field label="Mã học sinh" htmlFor="studentCode" error={createForm.formState.errors.studentCode?.message} required><Input id="studentCode" placeholder="HS2026001" {...createForm.register("studentCode")} /></Field><Field label="Họ và tên" htmlFor="fullName" error={createForm.formState.errors.fullName?.message} required><Input id="fullName" placeholder="Nguyễn Minh Anh" {...createForm.register("fullName")} /></Field><Field label="Ngày sinh" htmlFor="dateOfBirth" error={createForm.formState.errors.dateOfBirth?.message} required><Input id="dateOfBirth" type="date" {...createForm.register("dateOfBirth")} /></Field></form></Dialog>

    <Dialog open={Boolean(editing)} onOpenChange={(open) => { if (!open) setEditing(null); }} title="Cập nhật học sinh" description={editing ? `${editing.fullName} · ${editing.studentCode} · v${editing.version}` : undefined} footer={<><Button variant="ghost" onClick={() => setEditing(null)}>Hủy</Button><Button form="edit-student-form" type="submit" loading={updateStudent.isPending}>Lưu thay đổi</Button></>}><form id="edit-student-form" className="dialog-form" onSubmit={editForm.handleSubmit((values) => updateStudent.mutate(values))}><Field label="Họ và tên" htmlFor="editFullName" required><Input id="editFullName" {...editForm.register("fullName", { required: true })} /></Field><Field label="Ngày sinh" htmlFor="editDateOfBirth" required><Input id="editDateOfBirth" type="date" {...editForm.register("dateOfBirth", { required: true })} /></Field><Field label="Trạng thái" htmlFor="editStatus" required><Select id="editStatus" {...editForm.register("status")}><option value="Active">Đang hoạt động</option><option value="Suspended">Tạm dừng</option><option value="Graduated">Đã tốt nghiệp</option><option value="Withdrawn">Đã thôi học</option></Select></Field></form></Dialog>

    <Dialog open={Boolean(detailId)} onOpenChange={(open) => { if (!open) { setDetailId(""); setParentUserId(""); } }} title={detail.data?.data.student.fullName || "Chi tiết học sinh"} description={detail.data ? `${detail.data.data.student.studentCode} · ${detail.data.data.student.currentClassName || "Chưa xếp lớp"}` : "Đang tải hồ sơ"}>{detail.isLoading ? <LoadingPanel rows={5} /> : detail.error ? <ErrorPanel message={sessionErrorMessage(detail.error)} onRetry={() => detail.refetch()} /> : detail.data ? <div className="student-detail"><section><h3><UserRound size={17} /> Hồ sơ</h3><dl className="detail-grid"><div><dt>Ngày sinh</dt><dd>{formatDate(detail.data.data.student.dateOfBirth)}</dd></div><div><dt>Trạng thái</dt><dd>{statusLabel(detail.data.data.student.status)}</dd></div><div><dt>Lớp hiện tại</dt><dd>{detail.data.data.student.currentClassName || "Chưa xếp lớp"}</dd></div><div><dt>Tài khoản</dt><dd>{detail.data.data.student.accountEmail || "Chưa liên kết"}</dd></div></dl></section><section><h3><Users size={17} /> Phụ huynh</h3>{detail.data.data.guardians.length ? <div className="guardian-list">{detail.data.data.guardians.map((guardian) => <div key={guardian.linkId}><span><b>{guardian.fullName}</b><small><Mail size={13} /> {guardian.email} · {guardian.relationship}</small></span><button className="ui-icon-button danger-icon" title="Ngừng liên kết" aria-label={`Ngừng liên kết ${guardian.fullName}`} onClick={() => unlinkParent.mutate(guardian.parentUserId)} disabled={unlinkParent.isPending}><Unlink size={16} /></button></div>)}</div> : <p className="muted-copy">Chưa có phụ huynh được liên kết.</p>}<div className="link-parent-form"><Select value={parentUserId} onChange={(event) => setParentUserId(event.target.value)} aria-label="Chọn phụ huynh"><option value="">Chọn tài khoản phụ huynh</option>{parents.data?.data.items.filter((parent) => !detail.data?.data.guardians.some((guardian) => guardian.parentUserId === parent.id)).map((parent) => <option key={parent.id} value={parent.id}>{parent.fullName} · {parent.email}</option>)}</Select><Input value={relationship} onChange={(event) => setRelationship(event.target.value)} aria-label="Quan hệ với học sinh" placeholder="Cha/Mẹ" /><Button onClick={() => linkParent.mutate()} disabled={!parentUserId || !relationship.trim()} loading={linkParent.isPending}><Link2 size={16} /> Liên kết</Button></div></section><section><h3><School size={17} /> Lịch sử lớp</h3>{detail.data.data.enrollments.length ? <div className="enrollment-history">{detail.data.data.enrollments.map((enrollment) => <div key={enrollment.id}><span><b>{enrollment.className}</b><small>{enrollment.classCode} · {enrollment.semesterName}</small></span><Badge tone={statusTone(enrollment.status)}>{statusLabel(enrollment.status)}</Badge></div>)}</div> : <p className="muted-copy">Học sinh chưa có lịch sử ghi danh.</p>}</section></div> : null}</Dialog>
  </div>;
}

/** ClassFilterPicker cho chọn lớp theo hai cấp khối và lớp cụ thể. */
function ClassFilterPicker({ classes, value, onChange }: { classes: ClassRoomDto[]; value: string; onChange: (value: string) => void }) {
  const [open, setOpen] = useState(false);
  const [gradeLevel, setGradeLevel] = useState<number | null>(null);
  const selectedClass = classes.find((item) => item.id === value);
  const gradeLevels = useMemo(() => [...new Set(classes.map((item) => item.gradeLevel))].sort((left, right) => left - right), [classes]);
  const visibleClasses = classes.filter((item) => item.gradeLevel === gradeLevel).sort((left, right) => left.classCode.localeCompare(right.classCode, "vi"));

  const openPicker = () => {
    setGradeLevel(selectedClass?.gradeLevel ?? gradeLevels[0] ?? null);
    setOpen(true);
  };

  return <>
    <Button className="class-filter-trigger" variant="outline" onClick={openPicker} aria-label="Chọn lớp để lọc">
      <School size={16} />
      <span>{selectedClass ? `${selectedClass.classCode} · ${selectedClass.name}` : "Tất cả lớp"}</span>
      <ChevronDown size={15} />
    </Button>
    <Dialog open={open} onOpenChange={setOpen} title="Chọn lớp" description="Chọn khối trước, sau đó chọn lớp cụ thể đang hoạt động." footer={<><Button variant="ghost" onClick={() => { onChange(""); setOpen(false); }}>Tất cả lớp</Button><Button variant="outline" onClick={() => setOpen(false)}>Đóng</Button></>}>
      <div className="class-picker">
        <div className="class-picker__grades" role="tablist" aria-label="Chọn khối">
          {gradeLevels.map((grade) => <button key={grade} type="button" role="tab" aria-selected={gradeLevel === grade} onClick={() => setGradeLevel(grade)}><span>Khối {grade}</span><small>{classes.filter((item) => item.gradeLevel === grade).length} lớp</small></button>)}
        </div>
        <div className="class-picker__classes" aria-label={gradeLevel ? `Lớp thuộc khối ${gradeLevel}` : "Danh sách lớp"}>
          {visibleClasses.map((item) => <button key={item.id} type="button" className={value === item.id ? "selected" : ""} onClick={() => { onChange(item.id); setOpen(false); }}><span><b>{item.classCode}</b><small>{item.name}</small><em>{item.activeEnrollmentCount}/{item.capacity} học sinh</em></span>{value === item.id ? <Check size={18} /> : null}</button>)}
          {!visibleClasses.length ? <p>Khối này chưa có lớp đang hoạt động.</p> : null}
        </div>
      </div>
    </Dialog>
  </>;
}
