"use client";

import { Badge, Button, Card, CardHeader } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { Activity, Database, RefreshCw, Server, Wifi } from "lucide-react";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";

interface HealthResponse { status: string; totalDurationMs: number; checks: Record<string, { status: string; description: string; durationMs: number; error?: string | null }> }

/** SystemHealth hiển thị dependency health thật và trạng thái degraded rõ nguyên nhân. */
export function SystemHealth() {
  const { request } = useSession();
  const health = useQuery({ queryKey: ["health", "ready", "detail"], queryFn: () => request<HealthResponse>("/health/ready"), refetchInterval: 30_000 });
  if (health.isLoading) return <><PageHeader eyebrow="VẬN HÀNH HỆ THỐNG" title="Sức khỏe dependency" /><LoadingPanel rows={6} /></>;
  if (health.error) return <><PageHeader eyebrow="VẬN HÀNH HỆ THỐNG" title="Sức khỏe dependency" /><ErrorPanel message={sessionErrorMessage(health.error)} onRetry={() => health.refetch()} /></>;
  const data = health.data;
  return <div className="page-stack"><PageHeader eyebrow="VẬN HÀNH HỆ THỐNG" title="Sức khỏe dependency" description="Postgres, Redis, MongoDB và Ministry được kiểm tra trực tiếp từ backend." actions={<Button variant="outline" onClick={() => health.refetch()}><RefreshCw size={16} /> Kiểm tra lại</Button>} />
    <div className="health-summary"><div><span>READINESS</span><h2>{data?.status || "Unknown"}</h2><p>Tổng thời gian {Math.round(data?.totalDurationMs || 0)} ms</p></div><Activity size={36} /></div>
    <section className="health-grid">{Object.entries(data?.checks || {}).map(([name, check]) => { const healthy = check.status === "Healthy"; const Icon = name === "postgres" || name === "mongo" ? Database : name === "redis" ? Server : Wifi; return <Card key={name} className="health-card"><CardHeader title={dependencyName(name)} action={<Badge tone={healthy ? "success" : check.status === "Degraded" ? "warning" : "danger"}>{check.status}</Badge>} /><div><span className={`health-icon ${healthy ? "healthy" : "degraded"}`}><Icon size={22} /></span><p>{check.description}</p><dl><div><dt>Thời gian</dt><dd>{Math.round(check.durationMs)} ms</dd></div>{check.error ? <div><dt>Lỗi</dt><dd>{check.error}</dd></div> : null}</dl></div></Card>; })}</section>
    <p className="security-note">Chi tiết dependency chỉ dành cho SystemAdmin; public liveness chỉ trả trạng thái process.</p>
  </div>;
}

/** dependencyName đổi tên dependency kỹ thuật thành nhãn dễ quét. */
function dependencyName(name: string) { return ({ postgres: "PostgreSQL", redis: "Redis Cache", mongo: "MongoDB Audit", ministry: "Ministry API" } as Record<string, string>)[name] || name; }
