"use client";

import { Badge } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import DataGrid, { Column, FilterRow, HeaderFilter, Paging } from "devextreme-react/data-grid";
import PieChart, { Series as PieSeries, Label as PieLabel, Legend as PieLegend, Tooltip as PieTooltip } from "devextreme-react/pie-chart";
import { ShieldAlert, Search } from "lucide-react";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { RoleGate } from "@/components/role-gate";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import { AdvancedAnalyticsFilterBar } from "@/features/analytics/components/advanced-analytics-filter-bar";
import { useAnalyticsFilters } from "@/lib/use-analytics-filters";
import type { StudentRiskDto, Envelope } from "@/lib/domain";

/** 
 * RiskAnalyticsPage hiển thị danh sách học sinh có rủi ro học tập cao.
 * Ghi chú: Sử dụng mô hình dự báo rủi ro từ backend để cảnh báo sớm.
 */
export default function RiskAnalyticsPage() {
  return (
    <RoleGate allow={["SystemAdmin"]}>
      <RiskAnalyticsContent />
    </RoleGate>
  );
}

function RiskAnalyticsContent() {
  const { request } = useSession();
  const [filters, setFilters] = useAnalyticsFilters();

  const queryParams = new URLSearchParams();
  if (filters.semesterId) queryParams.set("semesterId", filters.semesterId);
  if (filters.previousSemesterId) queryParams.set("previousSemesterId", filters.previousSemesterId);
  filters.gradeLevels.forEach(v => queryParams.append("gradeLevels", String(v)));
  filters.classIds.forEach(v => queryParams.append("classIds", v));
  filters.subjectIds.forEach(v => queryParams.append("subjectIds", v));
  filters.teacherIds.forEach(v => queryParams.append("teacherIds", v));
  if (filters.riskLevel) queryParams.set("riskLevel", filters.riskLevel);
  queryParams.set("skip", String((filters.page - 1) * 20));
  queryParams.set("take", "20");

  const risk = useQuery({
    queryKey: ["admin-analytics", "student-risk", filters],
    queryFn: () => request<Envelope<StudentRiskDto>>(`/api/v1/admin/analytics/advanced/student-risk?${queryParams.toString()}`)
  });

  if (risk.isLoading) return <><PageHeader eyebrow="ADVANCED ANALYTICS" title="Rủi ro học tập" /><LoadingPanel rows={10} /></>;
  if (risk.error) return <><PageHeader eyebrow="ADVANCED ANALYTICS" title="Rủi ro học tập" /><ErrorPanel message={sessionErrorMessage(risk.error)} onRetry={() => risk.refetch()} /></>;

  const data = risk.data?.data;
  if (!data) return null;

  const riskLevelData = [
    { level: "Low", count: data.summary.low, color: "#94a3b8" },
    { level: "Medium", count: data.summary.medium, color: "#eab308" },
    { level: "High", count: data.summary.high, color: "#f97316" },
    { level: "Critical", count: data.summary.critical, color: "#ef4444" },
  ].filter(item => item.count > 0);

  const riskLevelTone = (level: string): "success" | "warning" | "danger" | "neutral" => {
    switch (level.toUpperCase()) {
      case "CRITICAL": return "danger";
      case "HIGH": return "danger";
      case "MEDIUM": return "warning";
      case "LOW": return "success";
      default: return "neutral";
    }
  };

  const riskLevelLabel = (level: string) => {
    switch (level.toUpperCase()) {
      case "CRITICAL": return "Nguy cấp";
      case "HIGH": return "Cao";
      case "MEDIUM": return "Trung bình";
      case "LOW": return "Thấp";
      default: return level;
    }
  };

  return (
    <div className="page-stack">
      <PageHeader 
        eyebrow="ADVANCED ANALYTICS" 
        title="Rủi ro học tập" 
        description="Danh sách học sinh cần được quan tâm đặc biệt dựa trên kết quả học tập và tiến độ hoàn thành điểm số."
      />

      <AdvancedAnalyticsFilterBar onRefresh={() => risk.refetch()} showRiskLevel />

      <section className="analytics-grid--overview">
        <div className="analytics-panel">
          <header><div><span>TỔNG QUAN RỦI RO</span><h2>Phân lớp mức độ rủi ro</h2><p>Tỷ lệ học sinh theo các ngưỡng cảnh báo của hệ thống.</p></div></header>
          <div className="risk-summary-content">
             <PieChart dataSource={riskLevelData} type="doughnut" innerRadius={0.65} height={260}>
                <PieSeries argumentField="level" valueField="count">
                   <PieLabel visible={false} />
                </PieSeries>
                <PieLegend visible={true} horizontalAlignment="center" verticalAlignment="bottom" />
                <PieTooltip enabled customizeTooltip={(item) => ({ text: `${riskLevelLabel(String(item.argument))}: ${item.valueText} học sinh` })} />
             </PieChart>
          </div>
        </div>

        <div className="metric-grid" style={{ gridTemplateColumns: "1fr 1fr" }}>
          <MetricCard label="Tổng học sinh" value={data.summary.total} caption="Trong phạm vi lọc hiện tại" icon={<ShieldAlert />} />
          <MetricCard label="Cần chú ý" value={data.summary.critical + data.summary.high} caption="Mức độ Nguy cấp và Cao" icon={<ShieldAlert />} />
        </div>
      </section>

      <section className="analytics-panel">
        <header><div><span>CHI TIẾT RỦI RO</span><h2>Danh sách học sinh cảnh báo</h2><p>Sử dụng dữ liệu điểm số hiện tại và lịch sử để đánh giá.</p></div></header>
        <DataGrid dataSource={data.items} keyExpr="studentId" showBorders={false} rowAlternationEnabled columnAutoWidth>
          <Paging enabled={false} />
          <FilterRow visible />
          <HeaderFilter visible />
          <Column dataField="studentCode" caption="Mã HS" width={110} />
          <Column dataField="studentName" caption="Họ và tên" minWidth={180} />
          <Column dataField="classCode" caption="Lớp" width={100} />
          <Column dataField="riskScore" caption="Điểm cảnh báo" alignment="center" width={110} cellRender={({ value }) => <b className={value > 70 ? "text-danger" : value > 40 ? "text-warning" : ""}>{value.toFixed(0)}</b>} />
          <Column dataField="riskLevel" caption="Mức độ" width={120} cellRender={({ value }) => <Badge tone={riskLevelTone(value)}>{riskLevelLabel(value)}</Badge>} />
          <Column dataField="currentAverage" caption="Điểm TB" format={{ type: "fixedPoint", precision: 2 }} width={100} />
          <Column dataField="growth" caption="Mức tăng/giảm" cellRender={({ value }) => <span className={value < 0 ? "text-danger" : "text-success"}>{value ? (value > 0 ? "+" : "") + value.toFixed(2) : "—"}</span>} width={120} />
          <Column dataField="failedSubjectCount" caption="Môn < TB" alignment="center" width={100} />
          <Column dataField="missingGradeRate" caption="Tỷ lệ thiếu điểm" cellRender={({ value }) => value ? `${value.toFixed(1)}%` : "0%"} width={130} />
          <Column dataField="reasons" caption="Lý do chính" minWidth={240} cellRender={({ value }) => <ul className="risk-reasons-list">{(value as { code: string; message: string }[]).map((r) => <li key={r.code}>{r.message}</li>)}</ul>} />
        </DataGrid>
        
        <div className="pagination" style={{ borderTop: "1px solid var(--border)", marginTop: "16px", paddingTop: "16px" }}>
           <span>Trang {filters.page}</span>
           <div>
              <button className="ui-icon-button" onClick={() => setFilters({ page: Math.max(1, filters.page - 1) })} disabled={filters.page <= 1}><Search size={16} /></button>
              <button className="ui-icon-button" onClick={() => setFilters({ page: filters.page + 1 })} disabled={data.items.length < 20}><Search size={16} /></button>
           </div>
        </div>
      </section>
    </div>
  );
}
