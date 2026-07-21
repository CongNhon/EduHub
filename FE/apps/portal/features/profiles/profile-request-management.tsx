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
    Select,
    Textarea,
    type DataColumn,
} from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, ExternalLink, FileSearch, ShieldCheck, X } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, StudentProfileChangeRequestDto } from "@/lib/domain";
import {
    formatDate,
    formatDateTime,
    statusLabel,
    statusTone,
} from "@/lib/domain";

/** ProfileRequestManagement giúp học vụ đối chiếu thông tin hiện tại, đề nghị và ảnh bằng chứng trước khi duyệt. */
export function ProfileRequestManagement() {
    const { request } = useSession();
    const queryClient = useQueryClient();
    const [status, setStatus] = useState("Pending");
    const [reviewing, setReviewing] =
        useState<StudentProfileChangeRequestDto | null>(null);
    const [reviewNote, setReviewNote] = useState("");
    const requests = useQuery({
        queryKey: ["student-profile-requests", status],
        queryFn: () =>
            request<Envelope<StudentProfileChangeRequestDto[]>>(
                `/api/v1/student-profile/requests${status ? `?status=${status}` : ""}`,
            ),
    });
    const review = useMutation({
        mutationFn: (approve: boolean) => {
            if (!reviewing) throw new Error("Chưa chọn yêu cầu.");
            return request<Envelope<StudentProfileChangeRequestDto>>(
                `/api/v1/student-profile/requests/${reviewing.id}/review`,
                {
                    method: "PUT",
                    body: JSON.stringify({
                        approve,
                        reviewNote: reviewNote || null,
                    }),
                },
            );
        },
        onSuccess: async (result) => {
            toast.success(
                result.data.status === "Approved"
                    ? "Đã duyệt và cập nhật hồ sơ học sinh."
                    : "Đã từ chối yêu cầu.",
            );
            setReviewing(null);
            setReviewNote("");
            await queryClient.invalidateQueries({
                queryKey: ["student-profile-requests"],
            });
        },
        onError: (error) => toast.error(sessionErrorMessage(error)),
    });
    const openEvidence = useCallback(
        async (url: string) => {
            try {
                const response = url.startsWith("http")
                    ? await fetch(url)
                    : await request<Response>(url, { raw: true });
                if (!response.ok)
                    throw new Error("Không thể đọc ảnh bằng chứng.");
                const objectUrl = URL.createObjectURL(await response.blob());
                window.open(objectUrl, "_blank", "noopener,noreferrer");
                window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
            } catch (error) {
                toast.error(sessionErrorMessage(error));
            }
        },
        [request],
    );
    const columns = useMemo<DataColumn<StudentProfileChangeRequestDto>[]>(
        () => [
            {
                key: "student",
                header: "Học sinh",
                cell: (row) => (
                    <div className="entity-cell">
                        <span>
                            <FileSearch size={17} />
                        </span>
                        <div>
                            <b>{row.currentFullName}</b>
                            <small>
                                {row.studentCode} · {row.requesterName}
                            </small>
                        </div>
                    </div>
                ),
            },
            {
                key: "changes",
                header: "Thay đổi chính",
                cell: (row) => (
                    <div className="profile-change-summary">
                        <b>
                            {row.currentFullName} → {row.requestedFullName}
                        </b>
                        <small>
                            {formatDate(row.currentDateOfBirth)} →{" "}
                            {formatDate(row.requestedDateOfBirth)}
                        </small>
                    </div>
                ),
            },
            {
                key: "requested",
                header: "Thời điểm",
                cell: (row) => formatDateTime(row.requestedAtUtc),
            },
            {
                key: "status",
                header: "Trạng thái",
                cell: (row) => (
                    <Badge tone={statusTone(row.status)}>
                        {statusLabel(row.status)}
                    </Badge>
                ),
            },
            {
                key: "action",
                header: "",
                className: "table-action",
                cell: (row) => (
                    <Button
                        variant="outline"
                        onClick={() => {
                            setReviewing(row);
                            setReviewNote("");
                        }}
                    >
                        <ShieldCheck size={15} />{" "}
                        {row.status === "Pending" ? "Duyệt" : "Chi tiết"}
                    </Button>
                ),
            },
        ],
        [],
    );
    if (requests.isLoading)
        return (
            <>
                <PageHeader
                    eyebrow="HỒ SƠ HỌC SINH"
                    title="Duyệt chỉnh sửa hồ sơ"
                />
                <LoadingPanel rows={8} />
            </>
        );
    if (requests.error)
        return (
            <>
                <PageHeader
                    eyebrow="HỒ SƠ HỌC SINH"
                    title="Duyệt chỉnh sửa hồ sơ"
                />
                <ErrorPanel
                    message={sessionErrorMessage(requests.error)}
                    onRetry={() => requests.refetch()}
                />
            </>
        );
    return (
        <div className="page-stack">
            <PageHeader
                eyebrow="INBOX HỌC VỤ"
                title="Duyệt chỉnh sửa hồ sơ"
                description="Chỉ cập nhật hồ sơ chính thức sau khi đối chiếu dữ liệu cũ, dữ liệu đề nghị và ảnh bằng chứng."
            />
            <Card>
                <CardHeader
                    title="Yêu cầu chỉnh sửa"
                    description={`${requests.data?.data.length || 0} yêu cầu phù hợp`}
                    action={
                        <Select
                            value={status}
                            onChange={(event) => setStatus(event.target.value)}
                            aria-label="Lọc trạng thái"
                        >
                            <option value="">Tất cả</option>
                            <option value="Pending">Chờ duyệt</option>
                            <option value="Approved">Đã duyệt</option>
                            <option value="Rejected">Đã từ chối</option>
                        </Select>
                    }
                />
                <DataTable
                    columns={columns}
                    rows={requests.data?.data || []}
                    rowKey={(row) => row.id}
                    empty={
                        <EmptyState
                            icon={FileSearch}
                            title="Không có yêu cầu phù hợp"
                            description="Yêu cầu mới từ học sinh sẽ xuất hiện tại đây."
                        />
                    }
                />
            </Card>
            <Dialog
                open={Boolean(reviewing)}
                onOpenChange={(open) => {
                    if (!open) setReviewing(null);
                }}
                title="Đối chiếu yêu cầu hồ sơ"
                description={
                    reviewing
                        ? `${reviewing.studentCode} · gửi ${formatDateTime(reviewing.requestedAtUtc)}`
                        : undefined
                }
                footer={
                    reviewing?.status === "Pending" ? (
                        <>
                            <Button
                                variant="danger"
                                onClick={() => review.mutate(false)}
                                loading={review.isPending}
                                disabled={!reviewNote.trim()}
                            >
                                <X size={16} /> Từ chối
                            </Button>
                            <Button
                                onClick={() => review.mutate(true)}
                                loading={review.isPending}
                            >
                                <Check size={16} /> Duyệt cập nhật
                            </Button>
                        </>
                    ) : (
                        <Button
                            variant="outline"
                            onClick={() => setReviewing(null)}
                        >
                            Đóng
                        </Button>
                    )
                }
            >
                <div className="profile-review">
                    <div className="profile-compare">
                        <section>
                            <span>THÔNG TIN HIỆN TẠI</span>
                            <b>{reviewing?.currentFullName}</b>
                            <p>
                                {formatDate(reviewing?.currentDateOfBirth)} ·{" "}
                                {reviewing?.currentGender ||
                                    "Chưa có giới tính"}
                            </p>
                            <p>
                                {reviewing?.currentPhoneNumber || "Chưa có SĐT"}
                            </p>
                            <p>
                                {reviewing?.currentAddress || "Chưa có địa chỉ"}
                            </p>
                        </section>
                        <section>
                            <span>THÔNG TIN ĐỀ NGHỊ</span>
                            <b>{reviewing?.requestedFullName}</b>
                            <p>
                                {formatDate(reviewing?.requestedDateOfBirth)} ·{" "}
                                {reviewing?.requestedGender ||
                                    "Chưa có giới tính"}
                            </p>
                            <p>
                                {reviewing?.requestedPhoneNumber ||
                                    "Chưa có SĐT"}
                            </p>
                            <p>
                                {reviewing?.requestedAddress ||
                                    "Chưa có địa chỉ"}
                            </p>
                        </section>
                    </div>
                    <div className="review-reason">
                        <b>Lý do</b>
                        <p>{reviewing?.reason}</p>
                    </div>
                    <Button
                        variant="outline"
                        onClick={() =>
                            reviewing &&
                            void openEvidence(reviewing.evidenceUrl)
                        }
                    >
                        <ExternalLink size={16} /> Mở ảnh bằng chứng
                    </Button>
                    {reviewing?.status === "Pending" ? (
                        <Field
                            label="Ghi chú duyệt / lý do từ chối"
                            htmlFor="profileReviewNote"
                        >
                            <Textarea
                                id="profileReviewNote"
                                value={reviewNote}
                                onChange={(event) =>
                                    setReviewNote(event.target.value)
                                }
                            />
                        </Field>
                    ) : (
                        <div className="review-reason">
                            <b>Kết quả xử lý</b>
                            <p>
                                {reviewing?.reviewNote || "Không có ghi chú."}
                            </p>
                        </div>
                    )}
                </div>
            </Dialog>
        </div>
    );
}
