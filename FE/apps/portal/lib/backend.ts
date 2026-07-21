import type { NextResponse } from "next/server";

export const REFRESH_COOKIE = "eduhub_refresh";
export const DEVICE_COOKIE = "eduhub_device";

function usesSecureCookies(request: Request) {
  const forwardedProtocol = request.headers.get("x-forwarded-proto")?.split(",", 1)[0]?.trim();
  return (forwardedProtocol || new URL(request.url).protocol.replace(":", "")) === "https";
}

/** Kiểm tra Origin của session mutation để chặn cross-site request ngoài portal. */
export function isSameOrigin(request: Request) {
  const origin = request.headers.get("origin");
  if (!origin) return true;
  return origin === new URL(request.url).origin;
}

/** Gọi backend ASP.NET từ BFF bằng base URL cố định phía server. */
export async function backendFetch(path: string, init?: RequestInit) {
  const baseUrl = process.env.EDUHUB_API_URL || "http://localhost:8080";
  return fetch(`${baseUrl}${path}`, { ...init, cache: "no-store" });
}

/** Ghi refresh token và device id vào HttpOnly cookie của portal. */
export function setSessionCookies(request: Request, response: NextResponse, refreshToken: string, refreshTokenExpiresAtUtc: string, deviceId: string) {
  const secure = usesSecureCookies(request);
  response.cookies.set(REFRESH_COOKIE, refreshToken, { httpOnly: true, sameSite: "lax", secure, path: "/", expires: new Date(refreshTokenExpiresAtUtc) });
  response.cookies.set(DEVICE_COOKIE, deviceId, { httpOnly: true, sameSite: "lax", secure, path: "/", maxAge: 60 * 60 * 24 * 365 });
}

/** Xóa cookie phiên kể cả khi backend logout không thành công. */
export function clearSessionCookies(request: Request, response: NextResponse) {
  const secure = usesSecureCookies(request);
  response.cookies.set(REFRESH_COOKIE, "", { httpOnly: true, sameSite: "lax", secure, path: "/", maxAge: 0 });
  response.cookies.set(DEVICE_COOKIE, "", { httpOnly: true, sameSite: "lax", secure, path: "/", maxAge: 0 });
}

/** Đọc JSON response an toàn khi backend trả body rỗng hoặc payload lỗi. */
export async function readJson(response: Response): Promise<unknown> {
  const text = await response.text();
  if (!text) return null;
  try { return JSON.parse(text); } catch { return { title: "Backend response không hợp lệ", detail: text }; }
}
