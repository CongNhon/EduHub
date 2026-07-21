import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { backendFetch, clearSessionCookies, isSameOrigin, REFRESH_COOKIE } from "@/lib/backend";

/** Logout BFF thu hồi refresh token ở backend và luôn xóa cookie local. */
export async function POST(request: Request) {
  if (!isSameOrigin(request)) return NextResponse.json({ title: "Origin không hợp lệ" }, { status: 403 });
  const refreshToken = (await cookies()).get(REFRESH_COOKIE)?.value;
  if (refreshToken) await backendFetch("/api/v1/auth/logout", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ refreshToken }) }).catch(() => null);
  const response = NextResponse.json({ success: true });
  clearSessionCookies(request, response);
  return response;
}
