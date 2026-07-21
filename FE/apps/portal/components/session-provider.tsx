"use client";

import { normalizeApiError, type ApiError } from "@eduhub/api-client";
import type { AuthSession, CurrentUser } from "@eduhub/auth";
import { isUserRole } from "@eduhub/auth";
import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from "react";

type SessionStatus = "bootstrapping" | "authenticated" | "anonymous";

interface LoginInput { email: string; password: string; }
interface RequestOptions extends RequestInit { retryAuth?: boolean; raw?: boolean; }

interface SessionContextValue {
  status: SessionStatus;
  user: CurrentUser | null;
  accessToken: string | null;
  login: (input: LoginInput) => Promise<CurrentUser>;
  logout: () => Promise<void>;
  request: <T>(path: string, options?: RequestOptions) => Promise<T>;
}

const SessionContext = createContext<SessionContextValue | null>(null);

/** SessionProvider giữ access token trong memory, refresh một lần và cung cấp API request có auth. */
export function SessionProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<SessionStatus>("bootstrapping");
  const [session, setSession] = useState<AuthSession | null>(null);
  const refreshPromise = useRef<Promise<AuthSession | null> | null>(null);

  const acceptSession = useCallback((payload: AuthSession | null) => {
    if (!payload || !isUserRole(payload.user.role)) { setSession(null); setStatus("anonymous"); return null; }
    setSession(payload); setStatus("authenticated"); return payload;
  }, []);

  const refresh = useCallback(async () => {
    if (!refreshPromise.current) {
      refreshPromise.current = fetch("/api/session/refresh", { method: "POST", credentials: "same-origin", cache: "no-store" })
        .then(async (response) => response.ok ? acceptSession(await response.json() as AuthSession) : acceptSession(null))
        .catch(() => acceptSession(null))
        .finally(() => { refreshPromise.current = null; });
    }
    return refreshPromise.current;
  }, [acceptSession]);

  useEffect(() => { void refresh(); }, [refresh]);

  const login = useCallback(async (input: LoginInput) => {
    const response = await fetch("/api/session/login", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ ...input, deviceId: crypto.randomUUID() }) });
    const payload = await response.json().catch(() => null);
    if (!response.ok) throw normalizeApiError(response.status, payload, response.headers.get("X-Correlation-ID"));
    const accepted = acceptSession(payload as AuthSession);
    if (!accepted) throw new Error("Role tài khoản không được portal hỗ trợ.");
    return accepted.user;
  }, [acceptSession]);

  const logout = useCallback(async () => {
    setSession(null); setStatus("anonymous");
    await fetch("/api/session/logout", { method: "POST" }).catch(() => null);
  }, []);

  const request = useCallback(async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const { retryAuth = true, raw = false, ...init } = options;
    const browserManagedContentType = init.body instanceof FormData || init.body instanceof Blob || init.body instanceof ArrayBuffer;
    const execute = async (activeSession: AuthSession | null) => fetch(`/api/backend${path}`, {
      ...init,
      headers: { Accept: "application/json", ...(init.body && !browserManagedContentType ? { "Content-Type": "application/json" } : {}), ...(init.headers || {}), ...(activeSession?.accessToken ? { Authorization: `Bearer ${activeSession.accessToken}` } : {}), "X-Correlation-ID": crypto.randomUUID() },
      cache: "no-store",
    });
    let response = await execute(session);
    if (response.status === 401 && retryAuth) { const nextSession = await refresh(); if (nextSession) response = await execute(nextSession); }
    if (raw) {
      if (!response.ok) throw normalizeApiError(response.status, await response.json().catch(() => null), response.headers.get("X-Correlation-ID"));
      return response as T;
    }
    const payload = await response.json().catch(() => null);
    if (!response.ok) throw normalizeApiError(response.status, payload, response.headers.get("X-Correlation-ID"));
    return payload as T;
  }, [refresh, session]);

  const value = useMemo(() => ({ status, user: session?.user || null, accessToken: session?.accessToken || null, login, logout, request }), [status, session, login, logout, request]);
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

/** useSession đọc trạng thái đăng nhập và API requester của portal. */
export function useSession() {
  const context = useContext(SessionContext);
  if (!context) throw new Error("useSession phải nằm trong SessionProvider.");
  return context;
}

/** Trích message ngắn từ ApiError cho form và toast. */
export function sessionErrorMessage(error: unknown) {
  return (error as ApiError)?.message || (error instanceof Error ? error.message : "Không thể hoàn tất thao tác.");
}
