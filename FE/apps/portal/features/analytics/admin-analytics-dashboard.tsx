"use client";

import { Button } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import Chart, { Animation, ArgumentAxis, Label, Legend, Point, Series, Tooltip, ValueAxis } from "devextreme-react/chart";
import DataGrid, { Column, FilterRow, HeaderFilter, Pager, Paging, SearchPanel } from "devextreme-react/data-grid";
import PieChart, { Animation as PieAnimation, Label as PieLabel, Legend as PieLegend, Series as PieSeries, Tooltip as PieTooltip } from "devextreme-react/pie-chart";
import SelectBox from "devextreme-react/select-box";
import { AlertTriangle, BookOpenCheck, Boxes, RefreshCw, ShieldAlert, Users, UserRoundCheck } from "lucide-react";
import { useState } from "react";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { AdminAcademicAnalyticsDto, AdminDataQualityDto, AdminOverviewDto, Envelope } from "@/lib/domain";
import { formatDateTime, statusLabel } from "@/lib/domain";

/** AdminAnalyticsDashboard hiển thị analytics thật của trường bằng DevExtreme charts, grids và semester filter. */
export function AdminAnalyticsDashboard() {
  const { request } = useSession();
  const [semesterId, setSemesterId] = useState<string>();
  const query = semesterId ? `?semesterId=${semesterId}` : "";
  const overview = useQuery({ queryKey: ["admin-analytics", "overview", semesterId], queryFn: () => request<Envelope<AdminOverviewDto>>(`/api/v1/admin/analytics/overview${query}`), refetchInterval: 60_000 });
  const academic = useQuery({ queryKey: ["admin-analytics", "academic", semesterId], queryFn: () => request<Envelope<AdminAcademicAnalyticsDto>>(`/api/v1/admin/analytics/academic${query}`), refetchInterval: 60_000 });
  const quality = useQuery({ queryKey: ["admin-analytics", "data-quality", semesterId], queryFn: () => request<Envelope<AdminDataQualityDto>>(`/api/v1/admin/analytics/data-quality${query}`), refetchInterval: 60_000 });

  if (overview.isLoading || academic.isLoading || quality.isLoading) return <><PageHeader eyebrow="DEVEXPRESS ANALYTICS" title="Điều hành toàn trường" /><LoadingPanel rows={9} /></>;
  const error = overview.error || academic.error || quality.error;
  if (error) return <><PageHeader eyebrow="DEVEXPRESS ANALYTICS" title="Điều hành toàn trường" /><ErrorPanel message={sessionErrorMessage(error)} onRetry={() => refreshAll(overview.refetch, academic.refetch, quality.refetch)} /></>;

  const overviewData = overview.data?.data;
  const academicData = academic.data?.data;
  const qualityData = quality.data?.data;
  if (!overviewData || !academicData || !qualityData) return null;
  const selectedSemesterId = semesterId || overviewData.semester.id;
  const operationalQueueTotal = overviewData.pendingProfileChangeRequests + overviewData.openReportRequests + overviewData.pendingOutboxMessages + overviewData.failedExternalSyncs;
  const actionableIssues = qualityData.issues.filter((item) => item.count > 0);
  const gradeStatusTotal = academicData.gradeStatuses.reduce((total, item) => total + item.count, 0);
  const peakGradeBucket = academicData.gradeDistribution.reduce((peak, item) => item.count > peak.count ? item : peak, academicData.gradeDistribution[0] || { label: "—", count: 0 });

  return <div className="page-stack admin-analytics">
    <PageHeader
      eyebrow="DEVEXPRESS ANALYTICS"
      title="Điều hành toàn trường"
      description="Theo dõi quy mô vận hành, kết quả học tập và chất lượng dữ liệu trên cùng một màn hình."
      actions={<div className="analytics-actions">
        <SelectBox dataSource={overviewData.availableSemesters} valueExpr="id" displayExpr={semesterLabel} value={selectedSemesterId} width={270} inputAttr={{ "aria-label": "Chọn học kỳ báo cáo" }} onValueChanged={(event) => setSemesterId(event.value as string)} />
        <Button variant="outline" onClick={() => refreshAll(overview.refetch, academic.refetch, quality.refetch)}><RefreshCw size={16} /> Làm mới</Button>
      </div>}
    />

    <section className="analytics-command-bar">
      <div><span>NGỮ CẢNH BÁO CÁO</span><strong>{overviewData.semester.academicYearName} · {overviewData.semester.name}</strong><small>Cập nhật {formatDateTime(overviewData.generatedAtUtc)}</small></div>
      <div className={`analytics-health ${qualityData.criticalFindings > 0 ? "analytics-health--warning" : ""}`}><i /> <span>{qualityData.criticalFindings > 0 ? `${qualityData.criticalFindings} lỗi nghiêm trọng` : "Dữ liệu ổn định"}</span></div>
    </section>

    <section className="metric-grid analytics-kpis">
      <MetricCard label="Học sinh đang học" value={overviewData.activeStudents} caption="Đã xếp lớp trong học kỳ" icon={<Users />} />
      <MetricCard label="Giáo viên active" value={overviewData.activeTeachers} caption="Đủ quyền truy cập giảng dạy" icon={<UserRoundCheck />} />
      <MetricCard label="Lớp đang hoạt động" value={overviewData.activeClasses} caption={`${overviewData.activeSubjects} môn học đang mở`} icon={<Boxes />} />
      <MetricCard label="Điểm trung bình" value={formatScore(academicData.averageNormalizedScore)} caption={`${academicData.publishedGradeCount}/${academicData.totalGradeCount} điểm đã công bố`} icon={<BookOpenCheck />} />
      <MetricCard label="Tỷ lệ đạt" value={formatPercent(academicData.passRatePercentage)} caption="Điểm Published/Locked từ 5" icon={<BookOpenCheck />} />
      <MetricCard label="Việc đang chờ xử lý" value={operationalQueueTotal} caption="Hồ sơ, báo cáo, outbox và đồng bộ" icon={<ShieldAlert />} />
    </section>

    <section className="analytics-work-queue" aria-label="Công việc vận hành cần xử lý">
      <div><span>Yêu cầu hồ sơ</span><strong>{overviewData.pendingProfileChangeRequests}</strong><small>Chờ học vụ xác minh</small></div>
      <div><span>Yêu cầu báo cáo</span><strong>{overviewData.openReportRequests}</strong><small>Chưa hoàn tất phản hồi</small></div>
      <div><span>Thông điệp chờ gửi</span><strong>{overviewData.pendingOutboxMessages}</strong><small>Đang chờ background worker</small></div>
      <div className={overviewData.failedExternalSyncs > 0 ? "is-warning" : ""}><span>Đồng bộ thất bại</span><strong>{overviewData.failedExternalSyncs}</strong><small>Cần kiểm tra Ministry API</small></div>
    </section>

    <section className="analytics-grid analytics-grid--overview">
      <div className="analytics-panel analytics-panel--feature">
        <header><div><span>PHÂN BỐ HỌC LỰC</span><h2>Đường cong điểm thang 10</h2><p>Chỉ gồm điểm đã công bố hoặc khóa.</p></div><div className="analytics-panel-stat"><b>{peakGradeBucket.label}</b><small>Dải điểm phổ biến</small></div></header>
        <Chart dataSource={academicData.gradeDistribution} height={330} palette={["#227d6b"]}>
          <Animation enabled duration={850} easing="easeOutCubic" />
          <ArgumentAxis valueMarginsEnabled={false}><Label /></ArgumentAxis>
          <ValueAxis allowDecimals={false} />
          <Series argumentField="label" valueField="count" name="Số điểm" type="bar" color="#227d6b" barPadding={0.18} />
          <Tooltip enabled customizeTooltip={(item) => ({ text: `${String(item.argument)}: ${item.valueText} điểm` })} />
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

/** refreshAll làm mới đồng thời ba dataset dashboard sau thao tác của SystemAdmin. */
function refreshAll(...refetchers: Array<() => Promise<unknown>>) { void Promise.all(refetchers.map((refetch) => refetch())); }

/** semesterLabel tạo nhãn học kỳ dễ nhận biết trong SelectBox. */
function semesterLabel(item?: { name: string; academicYearName: string }) { return item ? `${item.name} · ${item.academicYearName}` : ""; }

/** formatScore định dạng điểm thang 10 hoặc dấu gạch khi chưa có điểm công bố. */
function formatScore(value?: number | null) { return value == null ? "—" : value.toFixed(2); }

/** formatPercent định dạng tỷ lệ analytics theo phần trăm. */
function formatPercent(value?: number | null) { return value == null ? "—" : `${value.toFixed(1)}%`; }

/** severityLabel đổi severity kỹ thuật thành nhãn tiếng Việt. */
function severityLabel(value: string) { return value === "Critical" ? "Nghiêm trọng" : value === "Warning" ? "Cảnh báo" : value; }
