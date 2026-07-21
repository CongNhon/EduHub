"use client";

import { Badge, Button, Card, CardHeader, DataTable, Dialog, EmptyState, Field, Input, Select, type DataColumn } from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronLeft, ChevronRight, Pencil, Plus, Search, UserRoundCog, Users } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, PagedEnvelope, UserAccountDto } from "@/lib/domain";
import { useDebouncedValue } from "@/lib/use-debounced-value";

interface AccountForm { email: string; password: string; fullName: string; referenceCode: string; phoneNumber: string; role: string; isActive: boolean; }
const roleValues: Record<string, number> = { SystemAdmin: 0, AcademicAdmin: 1, Teacher: 2, Parent: 3, Student: 4 };
const roleLabels: Record<string, string> = { SystemAdmin: "Quản trị hệ thống", AcademicAdmin: "Quản trị học vụ", Teacher: "Giáo viên", Parent: "Phụ huynh", Student: "Học sinh" };

/** PeopleManagement hiển thị giáo viên/phụ huynh cho học vụ và cho SystemAdmin quản lý tài khoản. */
export function PeopleManagement() {
  const { request, user } = useSession();
  const queryClient = useQueryClient();
  const canManage = user?.role === "SystemAdmin";
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const [active, setActive] = useState("");
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<UserAccountDto | null>(null);
  const debouncedSearch = useDebouncedValue(search.trim(), 300);
  const queryString = new URLSearchParams({ page: String(page), pageSize: "10", ...(debouncedSearch ? { search: debouncedSearch } : {}), ...(role ? { role } : {}), ...(active ? { isActive: active } : {}) }).toString();
  const accounts = useQuery({ queryKey: ["users", { debouncedSearch, role, active, page }], queryFn: () => request<PagedEnvelope<UserAccountDto>>(`/api/v1/users?${queryString}`) });
  const createForm = useForm<AccountForm>({ defaultValues: { email: "", password: "", fullName: "", referenceCode: "", phoneNumber: "", role: "Teacher", isActive: true } });
  const editForm = useForm<AccountForm>({ defaultValues: { email: "", password: "", fullName: "", referenceCode: "", phoneNumber: "", role: "Teacher", isActive: true } });
  const createAccount = useMutation({ mutationFn: (value: AccountForm) => request<Envelope<UserAccountDto>>("/api/v1/users", { method: "POST", body: JSON.stringify({ email: value.email, password: value.password, fullName: value.fullName, referenceCode: value.referenceCode || null, phoneNumber: value.phoneNumber || null, role: roleValues[value.role] }) }), onSuccess: async () => { toast.success("Đã tạo tài khoản."); setCreateOpen(false); createForm.reset(); await queryClient.invalidateQueries({ queryKey: ["users"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const updateAccount = useMutation({ mutationFn: (value: AccountForm) => { if (!editing) throw new Error("Chưa chọn tài khoản."); return request<Envelope<UserAccountDto>>(`/api/v1/users/${editing.id}`, { method: "PUT", body: JSON.stringify({ fullName: value.fullName, referenceCode: value.referenceCode || null, phoneNumber: value.phoneNumber || null, role: roleValues[value.role], isActive: value.isActive }) }); }, onSuccess: async () => { toast.success("Đã cập nhật tài khoản và phân quyền."); setEditing(null); await queryClient.invalidateQueries({ queryKey: ["users"] }); }, onError: (error) => toast.error(sessionErrorMessage(error)) });
  const columns = useMemo<DataColumn<UserAccountDto>[]>(() => [
    { key: "person", header: "Người dùng", cell: (row) => <div className="entity-cell"><span><UserRoundCog size={17} /></span><div><b>{row.fullName}</b><small>{row.email}</small></div></div> },
    { key: "code", header: "Mã hồ sơ", cell: (row) => row.referenceCode || "—" },
    { key: "phone", header: "Điện thoại", cell: (row) => row.phoneNumber || "—" },
    { key: "role", header: "Vai trò", cell: (row) => roleLabels[row.role] || row.role },
    { key: "status", header: "Trạng thái", cell: (row) => <Badge tone={row.isActive ? "success" : "danger"}>{row.isActive ? "Đang hoạt động" : "Đã khóa"}</Badge> },
    { key: "action", header: "", className: "table-action", cell: (row) => canManage ? <button className="ui-icon-button" title={`Sửa ${row.fullName}`} aria-label={`Sửa tài khoản ${row.fullName}`} onClick={() => { setEditing(row); editForm.reset({ email: row.email, password: "", fullName: row.fullName, referenceCode: row.referenceCode || "", phoneNumber: row.phoneNumber || "", role: row.role, isActive: row.isActive }); }}><Pencil size={16} /></button> : null },
  ], [canManage, editForm]);
  if (accounts.isLoading) return <><PageHeader eyebrow="CON NGƯỜI & PHÂN QUYỀN" title="Giáo viên, phụ huynh và tài khoản" /><LoadingPanel rows={9} /></>;
  if (accounts.error) return <><PageHeader eyebrow="CON NGƯỜI & PHÂN QUYỀN" title="Giáo viên, phụ huynh và tài khoản" /><ErrorPanel message={sessionErrorMessage(accounts.error)} onRetry={() => accounts.refetch()} /></>;
  return <div className="page-stack"><PageHeader eyebrow="CON NGƯỜI & PHÂN QUYỀN" title="Giáo viên, phụ huynh và tài khoản" description={canManage ? "SystemAdmin quản lý danh tính, vai trò và trạng thái truy cập." : "Học vụ tra cứu người dùng để phân công giáo viên và liên kết phụ huynh."} actions={canManage ? <Button onClick={() => setCreateOpen(true)}><Plus size={17} /> Tạo tài khoản</Button> : undefined} />
    <Card><CardHeader title="Danh sách người dùng" description={`${accounts.data?.data.totalCount || 0} tài khoản phù hợp`} /><div className="table-toolbar"><div className="search-box"><Search size={17} /><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder="Tìm tên, email hoặc mã hồ sơ" aria-label="Tìm người dùng" /></div><Select value={role} onChange={(event) => { setRole(event.target.value); setPage(1); }}><option value="">Tất cả vai trò</option>{Object.entries(roleLabels).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</Select><Select value={active} onChange={(event) => { setActive(event.target.value); setPage(1); }}><option value="">Tất cả trạng thái</option><option value="true">Đang hoạt động</option><option value="false">Đã khóa</option></Select></div><DataTable columns={columns} rows={accounts.data?.data.items || []} rowKey={(row) => row.id} empty={<EmptyState icon={Users} title="Không có tài khoản phù hợp" description="Thay đổi từ khóa hoặc bộ lọc vai trò." />} /><div className="pagination"><span>Trang {accounts.data?.data.page || 1} / {Math.max(1, accounts.data?.data.totalPages || 1)}</span><div><button className="ui-icon-button" onClick={() => setPage((value) => Math.max(1, value - 1))} disabled={page <= 1} aria-label="Trang trước"><ChevronLeft size={17} /></button><button className="ui-icon-button" onClick={() => setPage((value) => value + 1)} disabled={page >= (accounts.data?.data.totalPages || 1)} aria-label="Trang sau"><ChevronRight size={17} /></button></div></div></Card>
    <Dialog open={createOpen} onOpenChange={setCreateOpen} title="Tạo tài khoản" description="Tạo danh tính đăng nhập và gán vai trò hệ thống." footer={<><Button variant="ghost" onClick={() => setCreateOpen(false)}>Hủy</Button><Button form="create-account-form" type="submit" loading={createAccount.isPending}>Tạo tài khoản</Button></>}><form id="create-account-form" className="dialog-form" onSubmit={createForm.handleSubmit((value) => createAccount.mutate(value))}><div className="form-grid-2"><Field label="Họ và tên" htmlFor="accountName" required><Input id="accountName" {...createForm.register("fullName", { required: true })} /></Field><Field label="Email đăng nhập" htmlFor="accountEmail" required><Input id="accountEmail" type="email" {...createForm.register("email", { required: true })} /></Field><Field label="Mật khẩu ban đầu" htmlFor="accountPassword" required><Input id="accountPassword" type="password" {...createForm.register("password", { required: true, minLength: 8 })} /></Field><Field label="Vai trò" htmlFor="accountRole" required><Select id="accountRole" {...createForm.register("role")}>{Object.entries(roleLabels).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</Select></Field><Field label="Mã hồ sơ" htmlFor="accountCode"><Input id="accountCode" {...createForm.register("referenceCode")} /></Field><Field label="Điện thoại" htmlFor="accountPhone"><Input id="accountPhone" {...createForm.register("phoneNumber")} /></Field></div></form></Dialog>
    <Dialog open={Boolean(editing)} onOpenChange={(open) => { if (!open) setEditing(null); }} title="Cập nhật tài khoản" description={editing?.email} footer={<><Button variant="ghost" onClick={() => setEditing(null)}>Hủy</Button><Button form="edit-account-form" type="submit" loading={updateAccount.isPending}>Lưu thay đổi</Button></>}><form id="edit-account-form" className="dialog-form" onSubmit={editForm.handleSubmit((value) => updateAccount.mutate(value))}><div className="form-grid-2"><Field label="Họ và tên" htmlFor="editAccountName" required><Input id="editAccountName" {...editForm.register("fullName", { required: true })} /></Field><Field label="Vai trò" htmlFor="editAccountRole" required><Select id="editAccountRole" {...editForm.register("role")}>{Object.entries(roleLabels).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</Select></Field><Field label="Mã hồ sơ" htmlFor="editAccountCode"><Input id="editAccountCode" {...editForm.register("referenceCode")} /></Field><Field label="Điện thoại" htmlFor="editAccountPhone"><Input id="editAccountPhone" {...editForm.register("phoneNumber")} /></Field></div><label className="toggle-row"><input type="checkbox" {...editForm.register("isActive")} /><span><b>Cho phép đăng nhập</b><small>Tắt để khóa tài khoản nhưng giữ lịch sử nghiệp vụ.</small></span></label></form></Dialog>
  </div>;
}
