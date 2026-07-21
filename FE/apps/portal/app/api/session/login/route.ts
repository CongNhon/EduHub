import { NextResponse } from "next/server";
import { backendFetch, isSameOrigin, readJson, setSessionCookies } from "@/lib/backend";

interface LoginEnvelope {
  success: boolean;
  data: { accessToken: string; accessTokenExpiresAtUtc: string; refreshToken: string; refreshTokenExpiresAtUtc: string };
}

/** Login BFF đổi email/password lấy token và giấu refresh token trong HttpOnly cookie. */
export async function POST(request: Request) {
  if (!isSameOrigin(request)) return NextResponse.json({ title: "Origin không hợp lệ" }, { status: 403 });
  const body = await request.json().catch(() => null) as { email?: string; password?: string; deviceId?: string } | null;
  if (!body?.email || !body.password) return NextResponse.json({ title: "Dữ liệu không hợp lệ", detail: "Vui lòng nhập email và mật khẩu." }, { status: 400 });
  const deviceId = body.deviceId || crypto.randomUUID();
  const upstream = await backendFetch("/api/v1/auth/login", { method: "POST", headers: { "Content-Type": "application/json", "X-Correlation-ID": crypto.randomUUID() }, body: JSON.stringify({ email: body.email, password: body.password, deviceId }) });
  const payload = await readJson(upstream);
  if (!upstream.ok) return NextResponse.json(payload, { status: upstream.status, headers: { "X-Correlation-ID": upstream.headers.get("X-Correlation-ID") || "" } });
  const token = (payload as LoginEnvelope).data;
  const meResponse = await backendFetch("/api/v1/auth/me", { headers: { Authorization: `Bearer ${token.accessToken}` } });
  const mePayload = await readJson(meResponse);
  if (!meResponse.ok) return NextResponse.json(mePayload, { status: meResponse.status });
  const response = NextResponse.json({ accessToken: token.accessToken, accessTokenExpiresAtUtc: token.accessTokenExpiresAtUtc, user: (mePayload as { data: unknown }).data });
  setSessionCookies(request, response, token.refreshToken, token.refreshTokenExpiresAtUtc, deviceId);
  return response;
}
