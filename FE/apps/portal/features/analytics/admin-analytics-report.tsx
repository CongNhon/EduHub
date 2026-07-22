"use client";

import { Button } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import dynamic from "next/dynamic";
import Link from "next/link";
import { Download, FileChartColumn, Layers3, RefreshCw } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { fetchSetup } from "@devexpress/analytics-core/analytics-utils";
import { toast } from "sonner";
import { ExportSettings, RequestOptions, SearchSettings, TabPanelSettings } from "devexpress-reporting-react/dx-report-viewer";
import SelectBox from "devextreme-react/select-box";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { AdminOverviewDto, Envelope } from "@/lib/domain";

const ReportViewer = dynamic(() => import("devexpress-reporting-react/dx-report-viewer"), { ssr: false });

type ReportOption = {
  value: string;
  viewerName: string;
  label: string;
  description: string;
  grouped: boolean;
};

const REPORT_OPTIONS: ReportOption[] = [
  { value: "executive-summary", viewerName: "admin-system-analytics", label: "Tổng quan điều hành", description: "KPI toàn trường, học lực, lớp và chất lượng dữ liệu.", grouped: false },
  { value: "academic-by-grade", viewerName: "admin-academic-by-grade", label: "Kết quả theo khối", description: "Nhóm lớp theo khối 10, 11, 12 và đối chiếu điểm công bố.", grouped: true },
  { value: "data-quality", viewerName: "admin-data-quality", label: "Chất lượng dữ liệu", description: "Nhóm vấn đề theo Critical, Warning và mức độ còn lại.", grouped: true },
];

/** AdminAnalyticsReport hiển thị XtraReport bằng Web Document Viewer và cho phép export file chuẩn. */
export function AdminAnalyticsReport() {
  const { request, accessToken } = useSession();
  const [host, setHost] = useState<string>();
  const [semesterId, setSemesterId] = useState<string>();
  const [reportType, setReportType] = useState(REPORT_OPTIONS[0].value);
  const [exporting, setExporting] = useState<string>();
  const overview = useQuery({ queryKey: ["admin-analytics", "report-semesters"], queryFn: () => request<Envelope<AdminOverviewDto>>("/api/v1/admin/analytics/overview") });

  useEffect(() => setHost(`${window.location.origin}/api/backend/`), []);
  useEffect(() => {
    const authorization = accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
    fetchSetup.fetchSettings.headers = { ...fetchSetup.fetchSettings.headers, ...authorization };
  }, [accessToken]);
  const selectedSemesterId = semesterId || overview.data?.data.semester.id;
  const selectedReport = REPORT_OPTIONS.find((option) => option.value === reportType) ?? REPORT_OPTIONS[0];
  const reportUrl = useMemo(
    () => selectedSemesterId ? `${selectedReport.viewerName}--${selectedSemesterId}` : selectedReport.viewerName,
    [selectedReport.viewerName, selectedSemesterId],
  );

  if (overview.isLoading) return <><PageHeader eyebrow="DEVEXPRESS REPORTING" title="Báo cáo System Analytics" /><LoadingPanel rows={7} /></>;
  if (overview.error) return <><PageHeader eyebrow="DEVEXPRESS REPORTING" title="Báo cáo System Analytics" /><ErrorPanel message={sessionErrorMessage(overview.error)} onRetry={() => overview.refetch()} /></>;

  return <div className="page-stack admin-report-page">
    <PageHeader eyebrow="DEVEXPRESS REPORTING" title={selectedReport.label} description={selectedReport.description} actions={<div className="analytics-actions"><SelectBox dataSource={REPORT_OPTIONS} valueExpr="value" displayExpr="label" value={reportType} width={240} inputAttr={{ "aria-label": "Chọn mẫu báo cáo" }} onValueChanged={(event) => setReportType(event.value as string)} /><SelectBox dataSource={overview.data?.data.availableSemesters || []} valueExpr="id" displayExpr={semesterLabel} value={selectedSemesterId} width={260} inputAttr={{ "aria-label": "Chọn học kỳ báo cáo" }} onValueChanged={(event) => setSemesterId(event.value as string)} /><Link className="ui-button ui-button--outline" href="/admin"><FileChartColumn size={16} /> Dashboard</Link></div>} />
    <section className="report-context" aria-label="Thông tin báo cáo"><div><span>PHẠM VI</span><strong>{semesterLabel(overview.data?.data.availableSemesters.find((item) => item.id === selectedSemesterId))}</strong></div><div><span>CẤU TRÚC</span><strong>{selectedReport.grouped ? "Có phân nhóm dữ liệu" : "Tổng hợp điều hành"}</strong></div><div><span>MỤC ĐÍCH</span><strong>{selectedReport.grouped ? "Đối chiếu và xử lý chi tiết" : "Theo dõi toàn trường"}</strong></div></section>
    <div className="report-command-bar"><div><Layers3 size={18} /><span><strong>{selectedReport.label}</strong><small>DevExpress XtraReport · xem trước tương tác</small></span></div><div className="report-export-bar"><span>Tải file</span>{["pdf", "xlsx", "csv"].map((format) => <Button key={format} variant="outline" disabled={Boolean(exporting)} onClick={() => exportReport(format, selectedSemesterId, reportType, accessToken, setExporting)}>{exporting === format ? <RefreshCw className="spin" size={15} /> : <Download size={15} />} {format.toUpperCase()}</Button>)}</div></div>
    <section className="report-viewer-shell">{host ? <ReportViewer key={reportUrl} reportUrl={reportUrl} width="100%" height="820px" developmentMode={process.env.NODE_ENV === "development"}><RequestOptions host={host} invokeAction="DXXRDV" /><TabPanelSettings position="Left" width={320} /><SearchSettings searchEnabled useAsyncSearch /><ExportSettings useSameTab={false} useAsynchronousExport /></ReportViewer> : <LoadingPanel rows={6} />}</section>
  </div>;
}

/** exportReport tải file từ API BFF và giữ access token ngoài JavaScript runtime. */
async function exportReport(format: string, semesterId: string | undefined, reportType: string, accessToken: string | null, setExporting: (format?: string) => void) {
  setExporting(format);
  try {
    const query = new URLSearchParams({ format, reportType });
    if (semesterId) query.set("semesterId", semesterId);
    const response = await fetch(`/api/backend/api/v1/admin/analytics/report/export?${query}`, { credentials: "same-origin", headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined });
    if (!response.ok) throw new Error(`Export failed (${response.status}).`);
    const blob = await response.blob();
    const disposition = response.headers.get("content-disposition") || "";
    const fileName = disposition.match(/filename\*?=(?:UTF-8''|\")?([^";]+)/i)?.[1] || `eduhub-${reportType}.${format}`;
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = decodeURIComponent(fileName);
    link.click();
    URL.revokeObjectURL(link.href);
    toast.success(`Đã xuất báo cáo ${format.toUpperCase()}.`);
  } catch (error) {
    toast.error(error instanceof Error ? error.message : "Không thể xuất báo cáo.");
  } finally {
    setExporting(undefined);
  }
}

/** semesterLabel tạo nhãn học kỳ cho bộ lọc báo cáo. */
function semesterLabel(item?: { name: string; academicYearName: string }) { return item ? `${item.name} · ${item.academicYearName}` : ""; }
