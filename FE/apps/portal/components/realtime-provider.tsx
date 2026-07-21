"use client";

import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { createContext, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { toast } from "sonner";
import { useSession } from "./session-provider";

export type RealtimeState = "disconnected" | "connecting" | "connected" | "reconnecting";
interface RealtimeContextValue { state: RealtimeState; }
const RealtimeContext = createContext<RealtimeContextValue>({ state: "disconnected" });

/** RealtimeProvider kết nối SignalR theo phiên và dùng event để invalidate dữ liệu REST. */
export function RealtimeProvider({ children }: { children: ReactNode }) {
  const { status, accessToken } = useSession();
  const queryClient = useQueryClient();
  const [connectionState, setConnectionState] = useState<RealtimeState>("connecting");
  const seen = useRef(new Set<string>());

  useEffect(() => {
    if (status !== "authenticated" || !accessToken) return;
    const connection = new HubConnectionBuilder().withUrl(process.env.NEXT_PUBLIC_SIGNALR_URL || "/hubs/notifications", { accessTokenFactory: () => accessToken }).withAutomaticReconnect([0, 1500, 5000, 10_000]).configureLogging(LogLevel.Warning).build();
    const accept = (event: Record<string, unknown>) => { const id = String(event.eventId || event.id || crypto.randomUUID()); if (seen.current.has(id)) return false; seen.current.add(id); if (seen.current.size > 200) seen.current.delete(seen.current.values().next().value as string); return true; };
    connection.on("NotificationCreated", (event) => { if (!accept(event || {})) return; void queryClient.invalidateQueries({ queryKey: ["notifications"] }); });
    connection.on("NotificationReceived", (event) => { if (!accept(event || {})) return; void queryClient.invalidateQueries({ queryKey: ["notifications"] }); });
    connection.on("GradePublished", (event) => { if (!accept(event || {})) return; void queryClient.invalidateQueries({ queryKey: ["published-grades", event.studentId] }); toast.success("Có kết quả học tập mới được công bố."); });
    connection.on("GradeReopened", (event) => { if (!accept(event || {})) return; void queryClient.invalidateQueries({ queryKey: ["published-grades", event.studentId] }); });
    connection.on("ReportReady", (event) => { if (!accept(event || {})) return; void queryClient.invalidateQueries({ queryKey: ["report-job", event.reportJobId] }); toast.success("Báo cáo đã sẵn sàng tải xuống."); });
    connection.on("SyncStatusChanged", (event) => { if (!accept(event || {})) return; void queryClient.invalidateQueries({ queryKey: ["sync-record", event.syncRecordId] }); });
    connection.onreconnecting(() => setConnectionState("reconnecting"));
    connection.onreconnected(() => { setConnectionState("connected"); void queryClient.invalidateQueries({ queryKey: ["notifications"] }); void queryClient.invalidateQueries({ queryKey: ["report-job"] }); });
    connection.onclose(() => setConnectionState("disconnected"));
    void connection.start().then(() => setConnectionState("connected")).catch(() => setConnectionState("disconnected"));
    return () => { connection.off("NotificationCreated"); connection.off("NotificationReceived"); connection.off("GradePublished"); connection.off("GradeReopened"); connection.off("ReportReady"); connection.off("SyncStatusChanged"); if (connection.state !== HubConnectionState.Disconnected) void connection.stop(); };
  }, [accessToken, queryClient, status]);

  const state = status === "authenticated" && accessToken ? connectionState : "disconnected";
  const value = useMemo(() => ({ state }), [state]);
  return <RealtimeContext.Provider value={value}>{children}</RealtimeContext.Provider>;
}

/** useRealtimeState trả trạng thái SignalR để chỉ cảnh báo khi degraded. */
export function useRealtimeState() { return useContext(RealtimeContext); }
