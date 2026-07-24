"use client";

import { useQuery } from "@tanstack/react-query";
import Chart, { Animation, ArgumentAxis, Label, Series, Tooltip, ValueAxis, Legend, Point, CommonSeriesSettings } from "devextreme-react/chart";
import { TrendingUp, CalendarDays, BookOpenCheck } from "lucide-react";
import { ErrorPanel, LoadingPanel, MetricCard } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { RoleGate } from "@/components/role-gate";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import { AdvancedAnalyticsFilterBar } from "@/features/analytics/components/advanced-analytics-filter-bar";
import { useAnalyticsFilters } from "@/lib/use-analytics-filters";
import type { AcademicTrendDto, Envelope } from "@/lib/domain";

/** 
 * TrendsAnalyticsPage hiển thị xu hướng học tập qua nhiều học kỳ.
 * Ghi chú: Sử dụng biểu đồ đường (spline) để trực quan hóa sự thay đổi điểm số và tỷ lệ đạt.
 */
export default function TrendsAnalyticsPage() {
  return (
    <RoleGate allow={["SystemAdmin"]}>
      <TrendsAnalyticsContent />
    </RoleGate>
  );
}

function TrendsAnalyticsContent() {
  const { request } = useSession();
  const [filters] = useAnalyticsFilters();

  const queryParams = new URLSearchParams();
  if (filters.semesterId) queryParams.set("semesterId", filters.semesterId);
  filters.gradeLevels.forEach(v => queryParams.append("gradeLevels", String(v)));
  filters.classIds.forEach(v => queryParams.append("classIds", v));
  filters.subjectIds.forEach(v => queryParams.append("subjectIds", v));
  filters.teacherIds.forEach(v => queryParams.append("teacherIds", v));
  queryParams.set("maxSemesters", String(filters.maxSemesters));

  const trends = useQuery({
    queryKey: ["admin-analytics", "academic-trends", filters],
    queryFn: () => request<Envelope<AcademicTrendDto>>(`/api/v1/admin/analytics/advanced/trends?${queryParams.toString()}`)
  });

  if (trends.isLoading) return <><PageHeader eyebrow="ADVANCED ANALYTICS" title="Xu hướng học tập" /><LoadingPanel rows={10} /></>;
  if (trends.error) return <><PageHeader eyebrow="ADVANCED ANALYTICS" title="Xu hướng học tập" /><ErrorPanel message={sessionErrorMessage(trends.error)} onRetry={() => trends.refetch()} /></>;

  const data = trends.data?.data;
  if (!data || data.points.length === 0) return null;

  const newestPoint = data.points[0];
  const oldestPoint = data.points[data.points.length - 1];
  const avgChange = newestPoint.mean && oldestPoint.mean ? newestPoint.mean - oldestPoint.mean : 0;

  return (
    <div className="page-stack">
      <PageHeader 
        eyebrow="ADVANCED ANALYTICS" 
        title="Xu hướng học tập" 
        description="Theo dõi sự thay đổi của kết quả học tập qua các học kỳ để đánh giá tiến độ và hiệu quả giảng dạy."
      />

      <AdvancedAnalyticsFilterBar onRefresh={() => trends.refetch()} showMaxSemesters />

      <section className="metric-grid">
        <MetricCard label="Số học kỳ phân tích" value={data.points.length} caption={`Từ ${oldestPoint.semesterName} đến nay`} icon={<CalendarDays />} />
        <MetricCard label="Mức tăng/giảm TB" value={(avgChange >= 0 ? "+" : "") + avgChange.toFixed(2)} caption="So với học kỳ đầu tiên trong chuỗi" icon={<TrendingUp />} />
        <MetricCard label="Tỷ lệ đạt hiện tại" value={newestPoint.passRate?.toFixed(1) + "%"} caption={`Trên ${newestPoint.validScoreCount} đầu điểm`} icon={<BookOpenCheck />} />
        <MetricCard label="Quy mô học sinh" value={newestPoint.studentCount} caption="Học sinh có điểm trong hệ thống" icon={<TrendingUp />} />
      </section>

      <section className="analytics-panel">
        <header><div><span>BIỂU ĐỒ XU HƯỚNG</span><h2>Điểm trung bình và mức điểm ở giữa</h2><p>Sự biến động của các chỉ số tập trung qua các giai đoạn (Mới nhất bên trái).</p></div></header>
        <Chart dataSource={data.points} height={400}>
          <CommonSeriesSettings argumentField="semesterName" type="spline">
            <Point visible={true} size={8} />
          </CommonSeriesSettings>
          <Series valueField="mean" name="Điểm trung bình" color="#3e78b2" />
          <Series valueField="median" name="Mức điểm ở giữa" color="#7b6db0" />
          <ArgumentAxis><Label overlappingBehavior="rotate" rotationAngle={-20} /></ArgumentAxis>
          <ValueAxis visualRange={{ startValue: 0, endValue: 10 }} title="Thang điểm 10" />
          <Legend verticalAlignment="bottom" horizontalAlignment="center" />
          <Tooltip enabled shared customizeTooltip={(item) => ({ text: `${item.seriesName}: ${item.valueText}` })} />
          <Animation enabled />
        </Chart>
      </section>

      <section className="analytics-grid--two">
        <div className="analytics-panel">
          <header><div><span>TỶ LỆ ĐẠT & TRƯỢT</span><h2>Xu hướng chất lượng học tập</h2><p>Tỷ lệ phần trăm học sinh đạt yêu cầu (điểm ≥ 5).</p></div></header>
          <Chart dataSource={data.points} height={320}>
            <Series argumentField="semesterName" valueField="passRate" name="Tỷ lệ đạt (%)" type="area" color="#227d6b" opacity={0.6} />
            <Series argumentField="semesterName" valueField="failureRate" name="Tỷ lệ trượt (%)" type="line" color="#c75d55" />
            <ValueAxis visualRange={{ startValue: 0, endValue: 100 }} />
            <Legend verticalAlignment="bottom" horizontalAlignment="center" />
            <Tooltip enabled customizeTooltip={(item) => ({ text: `${item.seriesName}: ${item.valueText}%` })} />
          </Chart>
        </div>

        <div className="analytics-panel">
          <header><div><span>MỨC CHÊNH LỆCH</span><h2>Xu hướng phân hóa trình độ</h2><p>Mức chênh lệch (độ lệch chuẩn) càng cao thể hiện sự phân hóa điểm số càng lớn.</p></div></header>
          <Chart dataSource={data.points} height={320}>
            <Series argumentField="semesterName" valueField="standardDeviation" name="Mức chênh lệch" type="bar" color="#23839a" />
            <ValueAxis title="Độ lệch" />
            <Legend visible={false} />
            <Tooltip enabled />
          </Chart>
        </div>
      </section>
    </div>
  );
}
