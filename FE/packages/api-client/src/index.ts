import createClient from "openapi-fetch";
import type { paths } from "./generated/schema";

export interface ApiError {
  status: number;
  code: string;
  title: string;
  message: string;
  fieldErrors?: Record<string, string[]>;
  correlationId?: string;
  retryAfterSeconds?: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/** Tạo OpenAPI client bám schema sinh từ Swagger và đi qua same-origin BFF. */
export function createEduHubOpenApiClient(accessToken?: string) {
  return createClient<paths>({
    baseUrl: "/api/backend",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

/** Chuẩn hóa ProblemDetails và lỗi mạng thành AppError dùng thống nhất trên UI. */
export function normalizeApiError(status: number, payload: unknown, correlationId?: string | null): ApiError {
  const data = payload && typeof payload === "object" ? payload as Record<string, unknown> : {};
  const errors = data.errors && typeof data.errors === "object" ? data.errors as Record<string, string[]> : undefined;
  const code = typeof data.code === "string" ? data.code : `http.${status || 0}`;
  const messages: Record<string, string> = {
    "grade.out_of_range": "Điểm phải nằm trong thang điểm của thành phần này.",
    "grade.entry_window_closed": "Thời gian nhập điểm đã kết thúc.",
    "grade.invalid_state": "Sổ điểm đã thay đổi trạng thái. Hãy tải lại để tiếp tục.",
    "grade.concurrency_conflict": "Điểm vừa được người khác cập nhật.",
    "class.capacity_exceeded": "Lớp đã đủ sĩ số.",
    "student.code_conflict": "Mã học sinh này đã tồn tại.",
  };
  return {
    status,
    code,
    title: typeof data.title === "string" ? data.title : "Không thể hoàn tất thao tác",
    message: messages[code] || (typeof data.detail === "string" ? data.detail : status === 403 ? "Bạn không có quyền thực hiện thao tác này." : "Đã có lỗi xảy ra. Vui lòng thử lại."),
    fieldErrors: errors,
    correlationId: correlationId || (typeof data.correlationId === "string" ? data.correlationId : undefined),
  };
}
