"use client";

import { useQuery } from "@tanstack/react-query";
import Chart, { Animation, ArgumentAxis, Label, Legend, Point, Series, Tooltip, ValueAxis } from "devextreme-react/chart";
import DataGrid, { Column, FilterRow, HeaderFilter, Pager, Paging, SearchPanel } from "devextreme-react/data-grid";
import PieChart, { Animation as PieAnimation, Label as PieLabel, Legend as PieLegend, Series as PieSeries, Tooltip as PieTooltip } from "devextreme-react/pie-chart";
import { AlertTriangle, BookOpenCheck, ShieldAlert, TrendingUp, TrendingDown, Minus } from "lucide-react";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { AdminAcademicAnalyticsDto, AdminDataQualityDto, AdminOverviewDto, AdminAdvancedSummaryDto, CommonDecimalMetricDto, Envelope } from "@/lib/domain";
import { formatDateTime, statusLabel } from "@/lib/domain";
import { AdvancedAnalyticsFilterBar } from "./components/advanced-analytics-filter-bar";
import { useAnalyticsFilters } from "@/lib/use-analytics-filters";

/** AdminAnalyticsDashboard hiển thị analytics thật của trường bằng DevExtreme charts, grids và bộ lọc nâng cao. */
export function AdminAnalyticsDashboard() {
  const { request } = useSession();
  const [filters] = useAnalyticsFilters();

  const queryParams = new URLSearchParams();
  if (filters.semesterId) queryParams.set("semesterId", filters.semesterId);
  if (filters.previousSemesterId) queryParams.set("previousSemesterId", filters.previousSemesterId);
  filters.gradeLevels.forEach(v => queryParams.append("gradeLevels", String(v)));
  filters.classIds.forEach(v => queryParams.append("classIds", v));
  filters.subjectIds.forEach(v => queryParams.append("subjectIds", v));
  filters.teacherIds.forEach(v => queryParams.append("teacherIds", v));
  const query = queryParams.toString() ? `?${queryParams.toString()}` : "";

  const overview = useQuery({ queryKey: ["admin-analytics", "overview", filters], queryFn: () => request<Envelope<AdminOverviewDto>>(`/api/v1/admin/analytics/overview${query}`), refetchInterval: 60_000 });
  const academic = useQuery({ queryKey: ["admin-analytics", "academic", filters], queryFn: () => request<Envelope<AdminAcademicAnalyticsDto>>(`/api/v1/admin/analytics/academic${query}`), refetchInterval: 60_000 });
  const quality = useQuery({ queryKey: ["admin-analytics", "data-quality", filters], queryFn: () => request<Envelope<AdminDataQualityDto>>(`/api/v1/admin/analytics/data-quality${query}`), refetchInterval: 60_000 });
  const advanced = useQuery({ queryKey: ["admin-analytics", "advanced-summary", filters], queryFn: () => request<Envelope<AdminAdvancedSummaryDto>>(`/api/v1/admin/analytics/advanced/summary${query}`), refetchInterval: 60_000 });

  if (overview.isLoading || academic.isLoading || quality.isLoading || advanced.isLoading) return <><PageHeader eyebrow="DEVEXPRESS ANALYTICS" title="Điều hành toàn trường" /><LoadingPanel rows={9} /></>;
  const error = overview.error || academic.error || quality.error || advanced.error;
  if (error) return <><PageHeader eyebrow="DEVEXPRESS ANALYTICS" title="Điều hành toàn trường" /><ErrorPanel message={sessionErrorMessage(error)} onRetry={() => refreshAll(overview.refetch, academic.refetch, quality.refetch, advanced.refetch)} /></>;

  const overviewData = overview.data?.data;
  const academicData = academic.data?.data;
  const qualityData = quality.data?.data;
  const advancedData = advanced.data?.data;
  if (!overviewData || !academicData || !qualityData || !advancedData) return null;

  const operationalQueueTotal = overviewData.pendingProfileChangeRequests + overviewData.openReportRequests + overviewData.pendingOutboxMessages + overviewData.failedExternalSyncs;
  const actionableIssues = qualityData.issues.filter((item) => item.count > 0);
  const gradeStatusTotal = academicData.gradeStatuses.reduce((total, item) => total + item.count, 0);
  const peakGradeBucket = academicData.gradeDistribution.reduce((peak, item) => item.count > peak.count ? item : peak, academicData.gradeDistribution[0] || { label: "—", count: 0 });

  return <div className="page-stack admin-analytics">
    <PageHeader
      eyebrow="DEVEXPRESS ANALYTICS"
      title="Điều hành toàn trường"
      description="Theo dõi quy mô vận hành, kết quả học tập và chất lượng dữ liệu trên cùng một màn hình."
    />

    <AdvancedAnalyticsFilterBar onRefresh={() => refreshAll(overview.refetch, academic.refetch, quality.refetch, advanced.refetch)} />

    <section className="analytics-command-bar">
      <div><span>NGỮ CẢNH BÁO CÁO</span><strong>{overviewData.semester.academicYearName} · {overviewData.semester.name}</strong><small>Cập nhật {formatDateTime(advancedData.metadata.generatedAt)}</small></div>
      <div className={`analytics-health ${qualityData.criticalFindings > 0 ? "analytics-health--warning" : ""}`}><i /> <span>{qualityData.criticalFindings > 0 ? `${qualityData.criticalFindings} lỗi nghiêm trọng` : "Dữ liệu ổn định"}</span></div>
    </section>

    <section className="metric-grid analytics-kpis">
      <ComparisonMetricCard label="Điểm trung bình" metric={advancedData.averageScore} format={formatScore} icon={<BookOpenCheck />} />
      <ComparisonMetricCard label="Tỷ lệ đạt" metric={advancedData.passRate} format={formatPercent} icon={<BookOpenCheck />} />
      <ComparisonMetricCard label="Tỷ lệ giỏi" metric={advancedData.excellentRate} format={formatPercent} icon={<BookOpenCheck />} />
      <ComparisonMetricCard label="Tỷ lệ thiếu điểm" metric={advancedData.missingGradeRate} format={formatPercent} icon={<AlertTriangle />} />
      <MetricCard label="Cần chú ý" value={advancedData.growth.declinedCount} caption="Học sinh có điểm giảm sút" icon={<ShieldAlert />} />
      <MetricCard label="Việc chờ xử lý" value={operationalQueueTotal} caption="Hồ sơ, báo cáo, đồng bộ" icon={<ShieldAlert />} />
    </section>

    <section className="analytics-work-queue" aria-label="Công việc vận hành cần xử lý">
      <div><span>Yêu cầu hồ sơ</span><strong>{overviewData.pendingProfileChangeRequests}</strong><small>Chờ học vụ xác minh</small></div>
      <div><span>Yêu cầu báo cáo</span><strong>{overviewData.openReportRequests}</strong><small>Chưa hoàn tất phản hồi</small></div>
      <div><span>Thông điệp chờ gửi</span><strong>{overviewData.pendingOutboxMessages}</strong><small>Đang chờ background worker</small></div>
      <div className={overviewData.failedExternalSyncs > 0 ? "is-warning" : ""}><span>Đồng bộ thất bại</span><strong>{overviewData.failedExternalSyncs}</strong><small>Cần kiểm tra Ministry API</small></div>
    </section>

    <section className="analytics-grid analytics-grid--overview">
      <div className="analytics-panel analytics-panel--feature">
        <header><div><span>PHÂN BỐ HỌC LỰC</span><h2>Phân bổ điểm từ 0 đến 10</h2><p>Chỉ gồm điểm đã công bố hoặc khóa.</p></div><div className="analytics-panel-stat"><b>{peakGradeBucket.label}</b><small>Dải điểm phổ biến</small></div></header>
        <Chart dataSource={academicData.gradeDistribution} height={330} palette={["#227d6b"]}>
          <Animation enabled duration={850} easing="easeOutCubic" />
          <ArgumentAxis valueMarginsEnabled={false}><Label /></ArgumentAxis>
          <ValueAxis allowDecimals={false} />
          <Series argumentField="label" valueField="count" name="Số điểm" type="bar" color="#227d6b" barPadding={0.18} />
          <Tooltip enabled customizeTooltip={(item) => ({ text: `${String(item.argument)}: ${item.valueText} bản ghi` })} />
          <Legend visible={false} />
        </Chart>
      </div>

      <div className="analytics-panel analytics-panel--donut">
        <header><div><span>TIẾN ĐỘ CÔNG BỐ</span><h2>Trạng thái điểm số</h2><p>Tỷ trọng bản ghi điểm trong học kỳ.</p></div></header>
        <div className="analytics-donut-wrap">
          <PieChart dataSource={academicData.gradeStatuses} type="doughnut" innerRadius={0.68} palette={["#94a3b8", "#3e78b2", "#227d6b", "#7b6db0"]} height={285}>
            <PieAnimation enabled duration={850} easing="easeOutCubic" />
            <PieSeries argumentField="status" valueField="count"><PieLabel visible={false} /></PieSeries>
            <PieTooltip enabled customizeTooltip={(item) => ({ text: `${statusLabel(String(item.argument))}: ${item.valueText}` })} />
            <PieLegend horizontalAlignment="center" verticalAlignment="bottom" customizeText={(item) => statusLabel(item.pointName)} />
          </PieChart>
          <div className="analytics-donut-center"><strong>{gradeStatusTotal}</strong><span>bản ghi điểm</span></div>
        </div>
      </div>
    </section>

    <section className="analytics-panel analytics-panel--grade-levels">
      <header><div><span>QUY MÔ HỌC SINH</span><h2>Học sinh theo khối</h2><p>Đối chiếu nhanh quy mô khối 10, 11 và 12 trong học kỳ.</p></div></header>
      <Chart dataSource={overviewData.studentsByGradeLevel} height={280} palette={["#3e78b2"]}>
        <Animation enabled duration={850} easing="easeOutCubic" />
        <ArgumentAxis><Label customizeText={(item) => `Khối ${item.valueText}`} /></ArgumentAxis>
        <ValueAxis allowDecimals={false} />
        <Series argumentField="gradeLevel" valueField="studentCount" name="Học sinh" type="bar" color="#3e78b2" barPadding={0.35} />
        <Tooltip enabled customizeTooltip={(item) => ({ text: `Khối ${String(item.argument)}: ${item.valueText} học sinh` })} />
        <Legend visible={false} />
      </Chart>
    </section>

    <section className="analytics-panel analytics-panel--subjects">
      <header><div><span>SO SÁNH MÔN HỌC</span><h2>Điểm trung bình và tỷ lệ đạt</h2><p>Hai thang đo độc lập, cùng một thứ tự môn học.</p></div></header>
      <Chart dataSource={academicData.subjectPerformance} height={380}>
        <Animation enabled duration={900} easing="easeOutCubic" />
        <ArgumentAxis><Label overlappingBehavior="rotate" rotationAngle={-28} /></ArgumentAxis>
        <ValueAxis name="score" visualRange={{ startValue: 0, endValue: 10 }} />
        <ValueAxis name="rate" position="right" visualRange={{ startValue: 0, endValue: 100 }} />
        <Series argumentField="subjectName" valueField="averageNormalizedScore" name="Điểm trung bình" type="splinearea" axis="score" color="#3e78b2" opacity={0.2}><Point visible size={7} /></Series>
        <Series argumentField="subjectName" valueField="passRatePercentage" name="Tỷ lệ đạt (%)" type="spline" axis="rate" color="#e0a03a"><Point visible size={7} /></Series>
        <Tooltip enabled shared customizeTooltip={(item) => ({ text: `${item.seriesName}: ${item.valueText}` })} />
        <Legend verticalAlignment="bottom" horizontalAlignment="center" />
      </Chart>
    </section>

    <section className="analytics-panel">
      <header><div><span>SO SÁNH LỚP</span><h2>Hiệu quả học tập theo lớp</h2><p>Tìm kiếm, lọc và sắp xếp trực tiếp trên bảng.</p></div></header>
      <DataGrid dataSource={academicData.classPerformance} keyExpr="classCode" showBorders={false} rowAlternationEnabled columnAutoWidth allowColumnResizing>
        <SearchPanel visible width={260} placeholder="Tìm lớp..." /><FilterRow visible /><HeaderFilter visible /><Paging defaultPageSize={10} /><Pager visible showPageSizeSelector allowedPageSizes={[10, 20, 50]} showInfo />
        <Column dataField="classCode" caption="Mã lớp" /><Column dataField="className" caption="Tên lớp" /><Column dataField="gradeLevel" caption="Khối" alignment="center" /><Column dataField="averageNormalizedScore" caption="Điểm TB" format={{ type: "fixedPoint", precision: 2 }} /><Column dataField="passRatePercentage" caption="Tỷ lệ đạt" format={{ type: "fixedPoint", precision: 2 }} /><Column dataField="publishedGradeCount" caption="Điểm công bố" />
      </DataGrid>
    </section>

    <section className="analytics-panel analytics-quality">
      <header><div><span>DATA QUALITY</span><h2>Vấn đề cần xử lý</h2><p>Ưu tiên bản ghi ảnh hưởng trực tiếp tới vận hành học vụ.</p></div><span className="analytics-alert"><AlertTriangle size={16} /> {qualityData.totalFindings} phát hiện</span></header>
      <DataGrid dataSource={actionableIssues} keyExpr="code" showBorders={false} rowAlternationEnabled columnAutoWidth noDataText="Không có vấn đề dữ liệu cần xử lý">
        <FilterRow visible /><HeaderFilter visible /><Paging enabled={false} />
        <Column dataField="severity" caption="Mức độ" cellRender={({ value }) => <span className={`quality-severity quality-severity--${String(value).toLowerCase()}`}>{severityLabel(String(value))}</span>} /><Column dataField="title" caption="Vấn đề" minWidth={320} /><Column dataField="count" caption="Số bản ghi" alignment="right" sortOrder="desc" />
      </DataGrid>
    </section>
  </div>;
}

/** refreshAll làm mới đồng thời các dataset dashboard sau thao tác của SystemAdmin. */
function refreshAll(...refetchers: Array<() => Promise<unknown>>) { void Promise.all(refetchers.map((refetch) => refetch())); }

/** formatScore định dạng điểm thang 10 hoặc dấu gạch khi chưa có điểm công bố. */
function formatScore(value?: number | null) { return value == null ? "—" : value.toFixed(2); }

/** formatPercent định dạng tỷ lệ analytics theo phần trăm. */
function formatPercent(value?: number | null) { return value == null ? "—" : `${value.toFixed(1)}%`; }

/** severityLabel đổi severity kỹ thuật thành nhãn tiếng Việt. */
function severityLabel(value: string) { return value === "Critical" ? "Nghiêm trọng" : value === "Warning" ? "Cảnh báo" : value; }

/** ComparisonMetricCard hiển thị metric có so sánh xu hướng với học kỳ trước. */
function ComparisonMetricCard({ label, metric, format, icon }: { label: string; metric: CommonDecimalMetricDto; format: (v?: number | null) => string; icon: React.ReactNode }) {
  const trendIcon = metric.trend === "UP" ? <TrendingUp size={14} className="text-success" /> : metric.trend === "DOWN" ? <TrendingDown size={14} className="text-danger" /> : <Minus size={14} className="text-muted" />;
  const deltaColor = metric.trend === "UP" ? "text-success" : metric.trend === "DOWN" ? "text-danger" : "text-muted";

  return (
    <div className="metric-card analytics-metric-card">
      <div className="metric-card__icon">{icon}</div>
      <span>{label}</span>
      <strong>{format(metric.value)}</strong>
      <small>
        {metric.previousValue !== null ? (
          <span className="comparison-row">
            {trendIcon}
            <span className={deltaColor}>
              {metric.absoluteChange && metric.absoluteChange > 0 ? "+" : ""}{metric.absoluteChange?.toFixed(2)}
              ({metric.percentageChange && metric.percentageChange > 0 ? "+" : ""}{metric.percentageChange?.toFixed(1)}%)
            </span>
          </span>
        ) : (
          "Không có dữ liệu đối chiếu"
        )}
      </small>
    </div>
  );
}
