"use client";

import { Badge, Button, Card, CardHeader } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import Chart, { ArgumentAxis, Label, Legend, Series, Tooltip, ValueAxis } from "devextreme-react/chart";
import DataGrid, { Column, FilterRow, Paging } from "devextreme-react/data-grid";
import PieChart, { Connector, Label as PieLabel, Legend as PieLegend, Series as PieSeries, Tooltip as PieTooltip } from "devextreme-react/pie-chart";
import { Activity, Bell, Database, Gauge, RefreshCw, Server, Wifi } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, SystemMonitoringDto } from "@/lib/domain";
import { formatDateTime, statusLabel } from "@/lib/domain";

interface HealthResponse { status: string; totalDurationMs: number; checks: Record<string, { status: string; description: string; durationMs: number; error?: string | null }> }

/** HealthHistoryPoint lưu latency của từng dependency tại một lần health check trong phiên quản trị. */
interface HealthHistoryPoint { time: string; postgres?: number; redis?: number; mongo?: number; ministry?: number; }

/** SystemHealth hiển thị dependency health thật và trạng thái degraded rõ nguyên nhân. */
export function SystemHealth() {
  const { request } = useSession();
  const health = useQuery({ queryKey: ["health", "ready", "detail"], queryFn: () => request<HealthResponse>("/health/ready"), refetchInterval: 30_000 });
  const monitoring = useQuery({ queryKey: ["admin", "monitoring"], queryFn: () => request<Envelope<SystemMonitoringDto>>("/api/v1/admin/monitoring"), refetchInterval: 30_000 });
  const [healthHistory, setHealthHistory] = useState<HealthHistoryPoint[]>([]);
  const lastHistoryAt = useRef(0);
  useEffect(() => {
    if (!health.data || !health.dataUpdatedAt || health.dataUpdatedAt === lastHistoryAt.current) return;
    lastHistoryAt.current = health.dataUpdatedAt;
    const checks = health.data.checks;
    const point: HealthHistoryPoint = {
      time: new Date(health.dataUpdatedAt).toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit", second: "2-digit" }),
      postgres: checks.postgres?.durationMs,
      redis: checks.redis?.durationMs,
      mongo: checks.mongo?.durationMs,
      ministry: checks.ministry?.durationMs,
    };
    setHealthHistory((current) => [...current.slice(-19), point]);
  }, [health.data, health.dataUpdatedAt]);
  if (health.isLoading || monitoring.isLoading) return <><PageHeader eyebrow="VẬN HÀNH HỆ THỐNG" title="Sức khỏe dependency" /><LoadingPanel rows={8} /></>;
  const error = health.error || monitoring.error;
  if (error) return <><PageHeader eyebrow="VẬN HÀNH HỆ THỐNG" title="Sức khỏe dependency" /><ErrorPanel message={sessionErrorMessage(error)} onRetry={() => refresh(health.refetch, monitoring.refetch)} /></>;
  const data = health.data;
  const operations = monitoring.data?.data;
  const hangfireData = operations ? Object.entries(operations.hangfire).filter(([name]) => !["servers", "recurring"].includes(name)).map(([status, count]) => ({ status: statusLabel(capitalize(status)), count })) : [];
  const cacheData = operations ? [{ status: "Hit", count: operations.cache.hits }, { status: "Miss", count: operations.cache.misses }, { status: "Failure", count: operations.cache.failures }] : [];
  const queueRows = operations ? [{ queue: "Đồng bộ Ministry", items: operations.externalSyncs }, { queue: "Email tổng hợp", items: operations.emailDigests }, { queue: "Xuất báo cáo", items: operations.reportJobs }].flatMap((group) => group.items.filter((item) => item.count > 0).map((item) => ({ key: `${group.queue}-${item.status}`, queue: group.queue, status: statusLabel(item.status), count: item.count }))) : [];
  return <div className="page-stack admin-analytics"><PageHeader eyebrow="VẬN HÀNH HỆ THỐNG" title="Sức khỏe dependency" description="Dependency health và operational queues được tự động làm mới mỗi 30 giây." actions={<Button variant="outline" onClick={() => refresh(health.refetch, monitoring.refetch)}><RefreshCw size={16} /> Kiểm tra lại</Button>} />
    <div className="health-summary"><div><span>MỨC SẴN SÀNG</span><h2>{healthStatus(data?.status)}</h2><p>Kiểm tra {formatDateTime(new Date(health.dataUpdatedAt).toISOString())} · tổng thời gian {Math.round(data?.totalDurationMs || 0)} ms</p></div><Activity size={36} /></div>
    <section className="health-grid">{Object.entries(data?.checks || {}).map(([name, check]) => { const healthy = check.status === "Healthy"; const Icon = name === "postgres" || name === "mongo" ? Database : name === "redis" ? Server : Wifi; return <Card key={name} className="health-card"><CardHeader title={dependencyName(name)} action={<Badge tone={healthy ? "success" : check.status === "Degraded" ? "warning" : "danger"}>{healthStatus(check.status)}</Badge>} /><div><span className={`health-icon ${healthy ? "healthy" : "degraded"}`}><Icon size={22} /></span><p>{healthDescription(name, check.status)}</p><dl><div><dt>Phản hồi</dt><dd>{Math.round(check.durationMs)} ms</dd></div>{check.error ? <div><dt>Chẩn đoán</dt><dd>{healthError(name, check.error)}</dd></div> : null}</dl></div></Card>; })}</section>
    <section className="analytics-panel health-history"><header><div><span>LATENCY HISTORY</span><h2>Độ trễ dependency trong phiên</h2><p>Tối đa 20 lần kiểm tra gần nhất, tự cập nhật mỗi 30 giây.</p></div></header><Chart dataSource={healthHistory} height={300}><ArgumentAxis argumentType="string"><Label /></ArgumentAxis><ValueAxis title="Milliseconds" /><Series argumentField="time" valueField="postgres" name="PostgreSQL" type="spline" color="#227d6b" /><Series argumentField="time" valueField="redis" name="Redis" type="spline" color="#3e78b2" /><Series argumentField="time" valueField="mongo" name="MongoDB" type="spline" color="#7b6db0" /><Series argumentField="time" valueField="ministry" name="Ministry API" type="spline" color="#d7655b" /><Tooltip enabled shared /><Legend verticalAlignment="bottom" horizontalAlignment="center" /></Chart></section>
    {operations ? <>
      <section className="metric-grid analytics-kpis"><MetricCard label="Redis hit rate" value={operations.cache.hitRatePercentage == null ? "—" : `${operations.cache.hitRatePercentage}%`} caption={`${operations.cache.failures} lần fallback lỗi`} icon={<Gauge />} /><MetricCard label="Outbox pending" value={operations.outbox.pending} caption={operations.outbox.oldestPendingAtUtc ? `Cũ nhất ${formatDateTime(operations.outbox.oldestPendingAtUtc)}` : "Không có backlog"} icon={<Server />} /><MetricCard label="Hangfire failed" value={operations.hangfire.failed} caption={`${operations.hangfire.processing} đang chạy · ${operations.hangfire.servers} server`} icon={<Activity />} /><MetricCard label="Thông báo 24 giờ" value={operations.notificationsLast24Hours} caption={`Snapshot ${formatDateTime(operations.generatedAtUtc)}`} icon={<Bell />} /></section>
      <section className="analytics-grid analytics-grid--two"><div className="analytics-panel"><header><div><span>HANGFIRE</span><h2>Job theo trạng thái</h2></div></header><Chart dataSource={hangfireData} height={300}><ArgumentAxis><Label overlappingBehavior="rotate" rotationAngle={-25} /></ArgumentAxis><ValueAxis allowDecimals={false} /><Series argumentField="status" valueField="count" type="bar" color="#236f9f" /><Tooltip enabled /><Legend visible={false} /></Chart></div><div className="analytics-panel"><header><div><span>REDIS CACHE</span><h2>Hit, miss và fallback</h2></div></header><PieChart dataSource={cacheData} height={300} palette={["#167a64", "#d89b35", "#c64f4f"]}><PieSeries argumentField="status" valueField="count"><PieLabel visible><Connector visible /></PieLabel></PieSeries><PieTooltip enabled /><PieLegend horizontalAlignment="center" verticalAlignment="bottom" /></PieChart></div></section>
      <section className="analytics-panel"><header><div><span>HÀNG ĐỢI VẬN HÀNH</span><h2>Công việc đang tồn đọng</h2><p>Chỉ hiển thị trạng thái có số lượng lớn hơn 0.</p></div></header><DataGrid dataSource={queueRows} keyExpr="key" showBorders={false} rowAlternationEnabled noDataText="Không có công việc tồn đọng"><FilterRow visible /><Paging enabled={false} /><Column dataField="queue" caption="Hàng đợi" /><Column dataField="status" caption="Trạng thái" /><Column dataField="count" caption="Số lượng" alignment="right" sortOrder="desc" /></DataGrid></section>
    </> : null}
    <p className="security-note">Chi tiết dependency chỉ dành cho SystemAdmin; public liveness chỉ trả trạng thái process.</p>
  </div>;
}

/** dependencyName đổi tên dependency kỹ thuật thành nhãn dễ quét. */
function dependencyName(name: string) { return ({ postgres: "PostgreSQL", redis: "Redis Cache", mongo: "MongoDB Audit", ministry: "Ministry API" } as Record<string, string>)[name] || name; }

/** healthStatus đổi trạng thái health check thành nhãn tiếng Việt dành cho quản trị viên. */
function healthStatus(status?: string) { return status === "Healthy" ? "Sẵn sàng" : status === "Degraded" ? "Suy giảm" : status === "Unhealthy" ? "Gián đoạn" : "Chưa xác định"; }

/** healthDescription mô tả vai trò dependency và kết quả kiểm tra thay cho thông báo kỹ thuật thô. */
function healthDescription(name: string, status: string) {
  if (status !== "Healthy") return `${dependencyName(name)} đang phản hồi không ổn định và cần được kiểm tra.`;
  return ({ postgres: "Cơ sở dữ liệu nghiệp vụ đang truy cập được.", redis: "Cache và dữ liệu phiên đang truy cập được.", mongo: "Kho audit log đang truy cập được.", ministry: "Dịch vụ tích hợp bên ngoài đang phản hồi." } as Record<string, string>)[name] || "Dependency đang phản hồi bình thường.";
}

/** healthError rút gọn lỗi dependency thành hướng chẩn đoán không làm lộ thông tin kết nối. */
function healthError(name: string, error: string) {
  if (/timeout|timed out|canceled/i.test(error)) return `${dependencyName(name)} vượt quá thời gian phản hồi cho phép.`;
  if (/refused|unreachable|connect/i.test(error)) return `Không thể kết nối tới ${dependencyName(name)}.`;
  return `${dependencyName(name)} trả về lỗi trong lần kiểm tra gần nhất.`;
}

/** refresh làm mới đồng thời health checks và operational metrics. */
function refresh(...refetchers: Array<() => Promise<unknown>>) { void Promise.all(refetchers.map((refetch) => refetch())); }

/** capitalize chuẩn hóa tên property Hangfire trước khi dịch trạng thái. */
function capitalize(value: string) { return value.charAt(0).toUpperCase() + value.slice(1); }
