import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { backendFetch, clearSessionCookies, DEVICE_COOKIE, isSameOrigin, readJson, REFRESH_COOKIE, setSessionCookies } from "@/lib/backend";

interface RefreshEnvelope {
  success: boolean;
  data: { accessToken: string; accessTokenExpiresAtUtc: string; refreshToken: string; refreshTokenExpiresAtUtc: string };
}

/** Refresh BFF xoay vòng refresh token và trả access token mới cho memory của tab. */
export async function POST(request: Request) {
  if (!isSameOrigin(request)) return NextResponse.json({ title: "Origin không hợp lệ" }, { status: 403 });
  const cookieStore = await cookies();
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;
  const deviceId = cookieStore.get(DEVICE_COOKIE)?.value;
  if (!refreshToken) return NextResponse.json({ title: "Không có phiên đăng nhập" }, { status: 401 });
  const upstream = await backendFetch("/api/v1/auth/refresh", { method: "POST", headers: { "Content-Type": "application/json", "X-Correlation-ID": crypto.randomUUID() }, body: JSON.stringify({ refreshToken, deviceId }) });
  const payload = await readJson(upstream);
  if (!upstream.ok) { const response = NextResponse.json(payload, { status: upstream.status }); clearSessionCookies(request, response); return response; }
  const token = (payload as RefreshEnvelope).data;
  const meResponse = await backendFetch("/api/v1/auth/me", { headers: { Authorization: `Bearer ${token.accessToken}` } });
  const mePayload = await readJson(meResponse);
  if (!meResponse.ok) { const response = NextResponse.json(mePayload, { status: meResponse.status }); clearSessionCookies(request, response); return response; }
  const response = NextResponse.json({ accessToken: token.accessToken, accessTokenExpiresAtUtc: token.accessTokenExpiresAtUtc, user: (mePayload as { data: unknown }).data });
  setSessionCookies(request, response, token.refreshToken, token.refreshTokenExpiresAtUtc, deviceId || crypto.randomUUID());
  return response;
}
