"use client";

import {
    Badge,
    Button,
    Card,
    CardHeader,
    DataTable,
    EmptyState,
    type DataColumn,
} from "@eduhub/ui";
import { useMutation } from "@tanstack/react-query";
import { Download, FileSpreadsheet, KeyRound, Upload } from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type {
    Envelope,
    StudentImportDto,
    StudentImportRowDto,
} from "@/lib/domain";

/** StudentImport tải template chuẩn và nhập đồng thời học sinh, phụ huynh, liên kết và ghi danh. */
export function StudentImport() {
    const { request } = useSession();
    const [file, setFile] = useState<File | null>(null);
    const [result, setResult] = useState<StudentImportDto | null>(null);
    const downloadTemplate = async () => {
        try {
            const response = await request<Response>(
                "/api/v1/imports/students/template",
                { raw: true },
            );
            const url = URL.createObjectURL(await response.blob());
            const link = document.createElement("a");
            link.href = url;
            link.download = "EduHub-Student-Import-Template.xlsx";
            link.click();
            URL.revokeObjectURL(url);
        } catch (error) {
            toast.error(sessionErrorMessage(error));
        }
    };
    const importWorkbook = useMutation({
        mutationFn: async () => {
            if (!file) throw new Error("Hãy chọn workbook XLSX.");
            const body = new FormData();
            body.append("file", file);
            return request<Envelope<StudentImportDto>>(
                "/api/v1/imports/students",
                { method: "POST", body },
            );
        },
        onSuccess: (response) => {
            setResult(response.data);
            toast.success(
                `Import hoàn tất: ${response.data.successCount}/${response.data.totalRows} dòng thành công.`,
            );
        },
        onError: (error) => toast.error(sessionErrorMessage(error)),
    });
    const columns = useMemo<DataColumn<StudentImportRowDto>[]>(
        () => [
            { key: "row", header: "Dòng", cell: (row) => row.rowNumber },
            {
                key: "code",
                header: "Mã học sinh",
                cell: (row) => row.studentCode || "—",
            },
            {
                key: "status",
                header: "Kết quả",
                cell: (row) => (
                    <Badge tone={row.success ? "success" : "danger"}>
                        {row.success ? "Thành công" : "Không nhập"}
                    </Badge>
                ),
            },
            {
                key: "error",
                header: "Chi tiết",
                cell: (row) => row.errorMessage || "Đã tạo/liên kết dữ liệu",
            },
        ],
        [],
    );
    return (
        <div className="page-stack">
            <PageHeader
                eyebrow="DỮ LIỆU HÀNG LOẠT"
                title="Import học sinh & phụ huynh"
                description="Một workbook tạo tài khoản học sinh, phụ huynh, liên kết gia đình và ghi danh vào đúng lớp/học kỳ."
                actions={
                    <Button
                        variant="outline"
                        onClick={() => void downloadTemplate()}
                    >
                        <Download size={16} /> Tải file mẫu
                    </Button>
                }
            />
            <Card className="import-workspace">
                <div className="import-drop">
                    <FileSpreadsheet size={34} />
                    <h2>Chọn workbook EduHub</h2>
                    <p>
                        Giữ nguyên tên 12 cột trong template. Mỗi dòng tương ứng
                        một học sinh và một phụ huynh.
                    </p>
                    <label className="file-drop" htmlFor="studentImportFile">
                        <Upload size={20} />
                        <span>{file?.name || "Chọn file .xlsx"}</span>
                    </label>
                    <input
                        id="studentImportFile"
                        className="file-input"
                        type="file"
                        accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        onChange={(event) => {
                            setFile(event.target.files?.[0] || null);
                            setResult(null);
                        }}
                    />
                    <Button
                        onClick={() => importWorkbook.mutate()}
                        loading={importWorkbook.isPending}
                        disabled={!file}
                    >
                        <Upload size={16} /> Kiểm tra và import
                    </Button>
                </div>
                <div className="import-columns">
                    <b>Workbook yêu cầu</b>
                    <ol>
                        <li>
                            StudentCode, FullName, DateOfBirth, Gender, Address,
                            StudentEmail
                        </li>
                        <li>
                            ParentFullName, ParentEmail, ParentPhone,
                            Relationship
                        </li>
                        <li>ClassCode, SemesterName</li>
                    </ol>
                    <small>Mật khẩu tạm chỉ trả về một lần sau import.</small>
                </div>
            </Card>
            {result ? (
                <>
                    <div className="metric-grid import-metrics">
                        <Card className="metric-card">
                            <span>Tổng dòng</span>
                            <strong>{result.totalRows}</strong>
                            <small>Workbook đã xử lý</small>
                        </Card>
                        <Card className="metric-card">
                            <span>Thành công</span>
                            <strong>{result.successCount}</strong>
                            <small>Dữ liệu đã lưu</small>
                        </Card>
                        <Card className="metric-card">
                            <span>Có lỗi</span>
                            <strong>{result.errorCount}</strong>
                            <small>Cần sửa và import lại</small>
                        </Card>
                        <Card className="metric-card">
                            <span>Tài khoản mới</span>
                            <strong>
                                {result.temporaryCredentials.length}
                            </strong>
                            <small>Mật khẩu tạm một lần</small>
                        </Card>
                    </div>
                    <Card>
                        <CardHeader
                            title="Kết quả từng dòng"
                            description="Dòng lỗi không làm dừng các dòng hợp lệ khác."
                        />
                        <DataTable
                            columns={columns}
                            rows={result.rows}
                            rowKey={(row) =>
                                `${row.rowNumber}-${row.studentCode}`
                            }
                            empty={
                                <EmptyState
                                    icon={FileSpreadsheet}
                                    title="Không có dòng dữ liệu"
                                    description="Kiểm tra lại workbook đã chọn."
                                />
                            }
                        />
                    </Card>
                    <Card>
                        <CardHeader
                            title="Thông tin đăng nhập mới"
                            description="Bàn giao an toàn; danh sách này không thể tải lại từ hệ thống."
                        />
                        <div className="credential-list">
                            {result.temporaryCredentials.length ? (
                                result.temporaryCredentials.map((item) => (
                                    <div key={`${item.role}-${item.email}`}>
                                        <KeyRound size={17} />
                                        <span>
                                            <b>{item.email}</b>
                                            <small>{item.role}</small>
                                        </span>
                                        <code>{item.temporaryPassword}</code>
                                    </div>
                                ))
                            ) : (
                                <p>
                                    Không tạo tài khoản mới trong lần import
                                    này.
                                </p>
                            )}
                        </div>
                    </Card>
                </>
            ) : null}
        </div>
    );
}
