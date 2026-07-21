"use client";

import {
    Badge,
    Button,
    Card,
    CardHeader,
    DataTable,
    Dialog,
    EmptyState,
    Field,
    Input,
    Select,
    Textarea,
    type DataColumn,
} from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { CircleUserRound, FileImage, History, Send } from "lucide-react";
import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type {
    Envelope,
    EvidenceUploadGrantDto,
    StudentProfileChangeRequestDto,
    StudentSelfProfileDto,
} from "@/lib/domain";
import {
    formatDate,
    formatDateTime,
    statusLabel,
    statusTone,
} from "@/lib/domain";

interface ProfileRequestForm {
    fullName: string;
    dateOfBirth: string;
    gender: string;
    phoneNumber: string;
    address: string;
    reason: string;
}

/** StudentProfile hiển thị hồ sơ chính thức và gửi thay đổi kèm ảnh bằng chứng để học vụ duyệt. */
export function StudentProfile() {
    const { request } = useSession();
    const queryClient = useQueryClient();
    const [dialogOpen, setDialogOpen] = useState(false);
    const [evidence, setEvidence] = useState<File | null>(null);
    const profile = useQuery({
        queryKey: ["student-profile", "me"],
        queryFn: () =>
            request<Envelope<StudentSelfProfileDto>>(
                "/api/v1/student-profile/me",
            ),
    });
    const requests = useQuery({
        queryKey: ["student-profile-requests", "me"],
        queryFn: () =>
            request<Envelope<StudentProfileChangeRequestDto[]>>(
                "/api/v1/student-profile/requests/me",
            ),
    });
    const form = useForm<ProfileRequestForm>({
        defaultValues: {
            fullName: "",
            dateOfBirth: "",
            gender: "",
            phoneNumber: "",
            address: "",
            reason: "",
        },
    });

    const submit = useMutation({
        mutationFn: async (value: ProfileRequestForm) => {
            if (!evidence)
                throw new Error("Hãy chọn ảnh bằng chứng trước khi gửi.");
            const grant = await request<Envelope<EvidenceUploadGrantDto>>(
                "/api/v1/student-profile/evidence/upload-grant",
                {
                    method: "POST",
                    body: JSON.stringify({
                        fileName: evidence.name,
                        contentType: evidence.type,
                        fileSize: evidence.size,
                    }),
                },
            );
            if (grant.data.usesDirectCloudUpload) {
                const upload = await fetch(grant.data.uploadUrl, {
                    method: "PUT",
                    headers: { "Content-Type": evidence.type },
                    body: evidence,
                });
                if (!upload.ok)
                    throw new Error("Không thể tải ảnh bằng chứng lên cloud.");
            } else {
                await request(grant.data.uploadUrl, {
                    method: "PUT",
                    headers: { "Content-Type": evidence.type },
                    body: evidence,
                });
            }
            return request<Envelope<StudentProfileChangeRequestDto>>(
                "/api/v1/student-profile/requests",
                {
                    method: "POST",
                    body: JSON.stringify({
                        ...value,
                        gender: value.gender || null,
                        phoneNumber: value.phoneNumber || null,
                        address: value.address || null,
                        evidenceObjectKey: grant.data.objectKey,
                    }),
                },
            );
        },
        onSuccess: async () => {
            toast.success("Đã gửi yêu cầu chỉnh sửa hồ sơ tới học vụ.");
            setDialogOpen(false);
            setEvidence(null);
            await queryClient.invalidateQueries({
                queryKey: ["student-profile-requests"],
            });
        },
        onError: (error) => toast.error(sessionErrorMessage(error)),
    });

    const openRequest = () => {
        const item = profile.data?.data;
        if (!item) return;
        form.reset({
            fullName: item.fullName,
            dateOfBirth: item.dateOfBirth,
            gender: item.gender || "",
            phoneNumber: item.phoneNumber || "",
            address: item.address || "",
            reason: "",
        });
        setEvidence(null);
        setDialogOpen(true);
    };
    const columns = useMemo<DataColumn<StudentProfileChangeRequestDto>[]>(
        () => [
            {
                key: "requested",
                header: "Gửi lúc",
                cell: (row) => formatDateTime(row.requestedAtUtc),
            },
            {
                key: "content",
                header: "Nội dung đề nghị",
                cell: (row) => (
                    <div>
                        <b>{row.requestedFullName}</b>
                        <small className="table-subtext">
                            {formatDate(row.requestedDateOfBirth)} ·{" "}
                            {row.requestedPhoneNumber || "Không có SĐT"}
                        </small>
                    </div>
                ),
            },
            {
                key: "reason",
                header: "Lý do",
                cell: (row) => (
                    <span className="profile-reason">{row.reason}</span>
                ),
            },
            {
                key: "status",
                header: "Trạng thái",
                cell: (row) => (
                    <div>
                        <Badge tone={statusTone(row.status)}>
                            {statusLabel(row.status)}
                        </Badge>
                        {row.reviewNote ? (
                            <small className="table-subtext">
                                {row.reviewNote}
                            </small>
                        ) : null}
                    </div>
                ),
            },
        ],
        [],
    );

    if (profile.isLoading || requests.isLoading)
        return (
            <>
                <PageHeader eyebrow="HỒ SƠ HỌC SINH" title="Hồ sơ của tôi" />
                <LoadingPanel rows={7} />
            </>
        );
    const error = profile.error || requests.error;
    if (error)
        return (
            <>
                <PageHeader eyebrow="HỒ SƠ HỌC SINH" title="Hồ sơ của tôi" />
                <ErrorPanel
                    message={sessionErrorMessage(error)}
                    onRetry={() => {
                        void profile.refetch();
                        void requests.refetch();
                    }}
                />
            </>
        );
    const item = profile.data!.data;
    return (
        <div className="page-stack">
            <PageHeader
                eyebrow="HỒ SƠ HỌC SINH"
                title="Hồ sơ của tôi"
                description="Thông tin chính thức do nhà trường quản lý. Mọi chỉnh sửa cần ảnh bằng chứng và được học vụ phê duyệt."
                actions={
                    <Button onClick={openRequest}>
                        <Send size={16} /> Yêu cầu chỉnh sửa
                    </Button>
                }
            />
            <Card className="profile-summary">
                <div className="profile-summary__identity">
                    <span>
                        <CircleUserRound size={28} />
                    </span>
                    <div>
                        <small>{item.studentCode}</small>
                        <h2>{item.fullName}</h2>
                        <Badge tone={statusTone(item.status)}>
                            {statusLabel(item.status)}
                        </Badge>
                    </div>
                </div>
                <dl className="detail-grid">
                    <div>
                        <dt>Ngày sinh</dt>
                        <dd>{formatDate(item.dateOfBirth)}</dd>
                    </div>
                    <div>
                        <dt>Giới tính</dt>
                        <dd>{item.gender || "Chưa cập nhật"}</dd>
                    </div>
                    <div>
                        <dt>Điện thoại</dt>
                        <dd>{item.phoneNumber || "Chưa cập nhật"}</dd>
                    </div>
                    <div>
                        <dt>Lớp hiện tại</dt>
                        <dd>{item.currentClassName || "Chưa xếp lớp"}</dd>
                    </div>
                    <div className="detail-span-2">
                        <dt>Địa chỉ</dt>
                        <dd>{item.address || "Chưa cập nhật"}</dd>
                    </div>
                </dl>
            </Card>
            <Card>
                <CardHeader
                    title="Lịch sử yêu cầu"
                    description={`${requests.data?.data.length || 0} yêu cầu đã gửi`}
                />
                <DataTable
                    columns={columns}
                    rows={requests.data?.data || []}
                    rowKey={(row) => row.id}
                    empty={
                        <EmptyState
                            icon={History}
                            title="Chưa có yêu cầu chỉnh sửa"
                            description="Chỉ gửi yêu cầu khi thông tin chính thức không còn chính xác."
                        />
                    }
                />
            </Card>
            <Dialog
                open={dialogOpen}
                onOpenChange={setDialogOpen}
                title="Yêu cầu chỉnh sửa hồ sơ"
                description="Nhập toàn bộ thông tin mong muốn sau chỉnh sửa và đính kèm ảnh giấy tờ phù hợp."
                footer={
                    <>
                        <Button
                            variant="ghost"
                            onClick={() => setDialogOpen(false)}
                        >
                            Hủy
                        </Button>
                        <Button
                            type="submit"
                            form="profile-request-form"
                            loading={submit.isPending}
                        >
                            Gửi học vụ
                        </Button>
                    </>
                }
            >
                <form
                    id="profile-request-form"
                    className="dialog-form"
                    onSubmit={form.handleSubmit((value) =>
                        submit.mutate(value),
                    )}
                >
                    <div className="form-grid-2">
                        <Field label="Họ và tên" htmlFor="profileName" required>
                            <Input
                                id="profileName"
                                {...form.register("fullName", {
                                    required: true,
                                })}
                            />
                        </Field>
                        <Field label="Ngày sinh" htmlFor="profileDob" required>
                            <Input
                                id="profileDob"
                                type="date"
                                {...form.register("dateOfBirth", {
                                    required: true,
                                })}
                            />
                        </Field>
                        <Field label="Giới tính" htmlFor="profileGender">
                            <Select
                                id="profileGender"
                                {...form.register("gender")}
                            >
                                <option value="">Chưa xác định</option>
                                <option value="Nam">Nam</option>
                                <option value="Nữ">Nữ</option>
                                <option value="Khác">Khác</option>
                            </Select>
                        </Field>
                        <Field label="Điện thoại" htmlFor="profilePhone">
                            <Input
                                id="profilePhone"
                                {...form.register("phoneNumber")}
                            />
                        </Field>
                    </div>
                    <Field label="Địa chỉ" htmlFor="profileAddress">
                        <Input
                            id="profileAddress"
                            {...form.register("address")}
                        />
                    </Field>
                    <Field
                        label="Lý do chỉnh sửa"
                        htmlFor="profileReason"
                        required
                    >
                        <Textarea
                            id="profileReason"
                            {...form.register("reason", {
                                required: true,
                                minLength: 10,
                            })}
                        />
                    </Field>
                    <Field
                        label="Ảnh bằng chứng"
                        htmlFor="profileEvidence"
                        required
                        hint="JPG, PNG hoặc WEBP; tối đa 5 MB."
                    >
                        <label className="file-drop" htmlFor="profileEvidence">
                            <FileImage size={22} />
                            <span>{evidence?.name || "Chọn ảnh giấy tờ"}</span>
                        </label>
                        <Input
                            id="profileEvidence"
                            className="file-input"
                            type="file"
                            accept="image/jpeg,image/png,image/webp"
                            onChange={(event) =>
                                setEvidence(event.target.files?.[0] || null)
                            }
                        />
                    </Field>
                </form>
            </Dialog>
        </div>
    );
}
