"use client";

import { useQuery } from "@tanstack/react-query";
import Chart, { Animation, ArgumentAxis, Label, Series, Tooltip, ValueAxis, Legend } from "devextreme-react/chart";
import DataGrid, { Column, FilterRow, HeaderFilter, Paging, Pager } from "devextreme-react/data-grid";
import { BookOpen, GraduationCap, Users } from "lucide-react";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { RoleGate } from "@/components/role-gate";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import { AdvancedAnalyticsFilterBar } from "@/features/analytics/components/advanced-analytics-filter-bar";
import { useAnalyticsFilters } from "@/lib/use-analytics-filters";
import type { AcademicDistributionDto, Envelope } from "@/lib/domain";

/** 
 * AcademicAnalyticsPage hiển thị phân bổ điểm số và hiệu quả học tập theo môn/lớp. 
 * Ghi chú: Sử dụng biểu đồ histogram và bảng so sánh của DevExtreme.
 */
export default function AcademicAnalyticsPage() {
  return (
    <RoleGate allow={["SystemAdmin"]}>
      <AcademicAnalyticsContent />
    </RoleGate>
  );
}

function AcademicAnalyticsContent() {
  const { request } = useSession();
  const [filters] = useAnalyticsFilters();

  const queryParams = new URLSearchParams();
  if (filters.semesterId) queryParams.set("semesterId", filters.semesterId);
  if (filters.previousSemesterId) queryParams.set("previousSemesterId", filters.previousSemesterId);
  filters.gradeLevels.forEach(v => queryParams.append("gradeLevels", String(v)));
  filters.classIds.forEach(v => queryParams.append("classIds", v));
  filters.subjectIds.forEach(v => queryParams.append("subjectIds", v));
  filters.teacherIds.forEach(v => queryParams.append("teacherIds", v));
  queryParams.set("groupBy", filters.groupBy);

  const academic = useQuery({
    queryKey: ["admin-analytics", "academic-distribution", filters],
    queryFn: () => request<Envelope<AcademicDistributionDto>>(`/api/v1/admin/analytics/advanced/distribution?${queryParams.toString()}`)
  });

  if (academic.isLoading) return <><PageHeader eyebrow="ADVANCED ANALYTICS" title="Phân tích học tập" /><LoadingPanel rows={10} /></>;
  if (academic.error) return <><PageHeader eyebrow="ADVANCED ANALYTICS" title="Phân tích học tập" /><ErrorPanel message={sessionErrorMessage(academic.error)} onRetry={() => academic.refetch()} /></>;

  const data = academic.data?.data;
  if (!data) return null;

  return (
    <div className="page-stack">
      <PageHeader 
        eyebrow="ADVANCED ANALYTICS" 
        title="Phân tích học tập" 
        description="Theo dõi phân bổ điểm số và so sánh hiệu quả giữa các khối, lớp và môn học."
      />

      <AdvancedAnalyticsFilterBar onRefresh={() => academic.refetch()} showGroupBy />

      <section className="metric-grid">
        <MetricCard label="Số điểm tính toán" value={data.overall.sampleSize} caption="Tổng số bản ghi điểm" icon={<Users />} />
        <MetricCard label="Điểm trung bình" value={data.overall.mean?.toFixed(2) || "—"} caption={`Chênh lệch: ${data.overall.standardDeviation?.toFixed(2) || "—"}`} icon={<GraduationCap />} />
        <MetricCard label="Mức điểm ở giữa" value={data.overall.median?.toFixed(2) || "—"} caption={`Dải điểm: ${data.overall.min || 0} - ${data.overall.max || 10}`} icon={<GraduationCap />} />
        <MetricCard label="Số lớp/nhóm" value={data.grouped.length} caption={`Phân loại theo ${filters.groupBy === "class" ? "lớp" : "khối"}`} icon={<BookOpen />} />
      </section>

      <section className="analytics-grid--two">
        <div className="analytics-panel">
          <header><div><span>PHÂN BỐ ĐIỂM SỐ</span><h2>Biểu đồ phân bổ điểm</h2><p>Tần suất xuất hiện của các dải điểm trong tập dữ liệu.</p></div></header>
          <Chart dataSource={data.buckets} height={350}>
            <Series argumentField="name" valueField="count" type="bar" color="#227d6b" />
            <ArgumentAxis><Label overlappingBehavior="rotate" rotationAngle={-45} /></ArgumentAxis>
            <ValueAxis title="Số lượng" />
            <Legend visible={false} />
            <Tooltip enabled customizeTooltip={(item) => ({ text: `${item.argument}: ${item.valueText} bản ghi (${data.buckets.find(b => b.name === item.argument)?.percentage.toFixed(1)}%)` })} />
            <Animation enabled />
          </Chart>
        </div>

        <div className="analytics-panel">
          <header><div><span>THỐNG KÊ CHI TIẾT</span><h2>Chỉ số nâng cao</h2><p>Các chỉ số thống kê mô tả chuyên sâu.</p></div></header>
          <div className="stats-list">
             <div className="stats-row"><span>Điểm trung bình</span><b>{data.overall.mean?.toFixed(2) || "—"}</b></div>
             <div className="stats-row"><span>Mức điểm ở giữa</span><b>{data.overall.median?.toFixed(2) || "—"}</b></div>
             <div className="stats-row"><span>Mức chênh lệch (Std Dev)</span><b>{data.overall.standardDeviation?.toFixed(2) || "—"}</b></div>
             <div className="stats-row"><span>Khoảng tứ phân vị (IQR)</span><b>{data.overall.interquartileRange?.toFixed(2) || "—"}</b></div>
             <div className="stats-row"><span>Bách phân vị 10 (P10)</span><b>{data.overall.p10?.toFixed(2) || "—"}</b></div>
             <div className="stats-row"><span>Bách phân vị 90 (P90)</span><b>{data.overall.p90?.toFixed(2) || "—"}</b></div>
          </div>
        </div>
      </section>

      <section className="analytics-panel">
        <header><div><span>SO SÁNH HIỆU QUẢ</span><h2>Kết quả theo {filters.groupBy === "class" ? "lớp học" : filters.groupBy === "subject" ? "môn học" : "khối lớp"}</h2><p>Bảng tổng hợp chỉ số cho từng nhóm dữ liệu.</p></div></header>
        <DataGrid dataSource={data.grouped} keyExpr="groupKey" showBorders={false} rowAlternationEnabled>
          <Paging defaultPageSize={10} /><Pager visible showInfo />
          <FilterRow visible /><HeaderFilter visible />
          <Column dataField="groupName" caption={filters.groupBy === "class" ? "Tên lớp" : filters.groupBy === "subject" ? "Tên môn" : "Khối lớp"} />
          <Column dataField="metrics.sampleSize" caption="Sĩ số điểm" alignment="center" />
          <Column dataField="metrics.mean" caption="Điểm TB" format={{ type: "fixedPoint", precision: 2 }} />
          <Column dataField="metrics.median" caption="Trung vị" format={{ type: "fixedPoint", precision: 2 }} />
          <Column dataField="metrics.standardDeviation" caption="Độ lệch chuẩn" format={{ type: "fixedPoint", precision: 3 }} />
          <Column dataField="metrics.min" caption="Thấp nhất" alignment="center" />
          <Column dataField="metrics.max" caption="Cao nhất" alignment="center" />
        </DataGrid>
      </section>
    </div>
  );
}
