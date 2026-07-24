"use client";

import { Button } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { RefreshCw, Filter, X } from "lucide-react";
import SelectBox from "devextreme-react/select-box";
import TagBox from "devextreme-react/tag-box";
import { useSession } from "@/components/session-provider";
import { useAnalyticsFilters } from "@/lib/use-analytics-filters";
import type { 
  AdminOverviewDto, 
  ClassRoomDto, 
  Envelope, 
  PagedEnvelope, 
  SubjectDto, 
  UserAccountDto 
} from "@/lib/domain";
import { useState } from "react";

interface FilterBarProps {
  onRefresh: () => void;
  showGroupBy?: boolean;
  showMaxSemesters?: boolean;
  showRiskLevel?: boolean;
}

/** 
 * AdvancedAnalyticsFilterBar cung cấp bộ lọc chuyên sâu cho SystemAdmin.
 * Ghi chú: Sử dụng URL-backed state qua useAnalyticsFilters hook.
 */
export function AdvancedAnalyticsFilterBar({ 
  onRefresh, 
  showGroupBy = false, 
  showMaxSemesters = false,
  showRiskLevel = false
}: FilterBarProps) {
  const { request } = useSession();
  const [filters, setFilters] = useAnalyticsFilters();
  const [isExpanded, setIsExpanded] = useState(false);

  // Fetch filter options
  const overview = useQuery({ 
    queryKey: ["admin-analytics", "overview"], 
    queryFn: () => request<Envelope<AdminOverviewDto>>("/api/v1/admin/analytics/overview") 
  });
  
  const classes = useQuery({ 
    queryKey: ["classes", "filter"], 
    queryFn: () => request<PagedEnvelope<ClassRoomDto>>("/api/v1/classes?page=1&pageSize=200") 
  });

  const subjects = useQuery({ 
    queryKey: ["subjects", "filter"], 
    queryFn: () => request<PagedEnvelope<SubjectDto>>("/api/v1/subjects?page=1&pageSize=200") 
  });

  const teachers = useQuery({ 
    queryKey: ["users", "teachers", "filter"], 
    queryFn: () => request<PagedEnvelope<UserAccountDto>>("/api/v1/users?role=Teacher&isActive=true&page=1&pageSize=200") 
  });

  const availableSemesters = overview.data?.data.availableSemesters || [];
  const classItems = classes.data?.data.items || [];
  const subjectItems = subjects.data?.data.items || [];
  const teacherItems = teachers.data?.data.items || [];

  const semesterLabel = (item: { name: string; academicYearName: string } | null) => item ? `${item.name} · ${item.academicYearName}` : "";

  return (
    <div className="advanced-filter-bar">
      <div className="filter-bar-header">
        <div className="filter-group main-filters">
          <div className="filter-item">
            <label>Học kỳ hiện tại</label>
            <SelectBox 
              dataSource={availableSemesters} 
              valueExpr="id" 
              displayExpr={semesterLabel} 
              value={filters.semesterId || overview.data?.data.semester.id} 
              onValueChanged={(e) => setFilters({ semesterId: e.value })}
              width={240}
              placeholder="Chọn học kỳ..."
            />
          </div>

          <div className="filter-item">
            <label>Học kỳ đối chiếu</label>
            <SelectBox 
              dataSource={availableSemesters} 
              valueExpr="id" 
              displayExpr={semesterLabel} 
              value={filters.previousSemesterId} 
              onValueChanged={(e) => setFilters({ previousSemesterId: e.value })}
              width={240}
              showClearButton
              placeholder="Không đối chiếu"
            />
          </div>

          <Button 
            variant={isExpanded ? "secondary" : "outline"} 
            onClick={() => setIsExpanded(!isExpanded)}
          >
            <Filter size={16} /> 
            {isExpanded ? "Thu gọn bộ lọc" : "Lọc nâng cao"}
            {(filters.classIds.length > 0 || filters.subjectIds.length > 0 || filters.teacherIds.length > 0 || filters.gradeLevels.length > 0) && (
              <span className="filter-badge" />
            )}
          </Button>

          <Button variant="ghost" onClick={onRefresh}>
            <RefreshCw size={16} />
            Làm mới
          </Button>
        </div>
      </div>

      {isExpanded && (
        <div className="filter-bar-expanded">
          <div className="filter-grid">
            <div className="filter-item">
              <label>Khối lớp</label>
              <TagBox 
                dataSource={[10, 11, 12]} 
                value={filters.gradeLevels} 
                onValueChanged={(e) => setFilters({ gradeLevels: e.value })}
                placeholder="Tất cả khối"
              />
            </div>

            <div className="filter-item">
              <label>Lớp học</label>
              <TagBox 
                dataSource={classItems} 
                valueExpr="id" 
                displayExpr="classCode" 
                value={filters.classIds} 
                onValueChanged={(e) => setFilters({ classIds: e.value })}
                searchEnabled
                placeholder="Tất cả lớp"
              />
            </div>

            <div className="filter-item">
              <label>Môn học</label>
              <TagBox 
                dataSource={subjectItems} 
                valueExpr="id" 
                displayExpr="name" 
                value={filters.subjectIds} 
                onValueChanged={(e) => setFilters({ subjectIds: e.value })}
                searchEnabled
                placeholder="Tất cả môn"
              />
            </div>

            <div className="filter-item">
              <label>Giáo viên</label>
              <TagBox 
                dataSource={teacherItems} 
                valueExpr="id" 
                displayExpr="fullName" 
                value={filters.teacherIds} 
                onValueChanged={(e) => setFilters({ teacherIds: e.value })}
                searchEnabled
                placeholder="Tất cả giáo viên"
              />
            </div>

            {showRiskLevel && (
              <div className="filter-item">
                <label>Mức độ cảnh báo</label>
                <SelectBox 
                  dataSource={[
                    { id: "LOW", label: "Thấp" },
                    { id: "MEDIUM", label: "Trung bình" },
                    { id: "HIGH", label: "Cao" },
                    { id: "CRITICAL", label: "Nguy cấp" }
                  ]} 
                  valueExpr="id"
                  displayExpr="label"
                  value={filters.riskLevel} 
                  onValueChanged={(e) => setFilters({ riskLevel: e.value })}
                  showClearButton
                  placeholder="Tất cả"
                />
              </div>
            )}

            {showGroupBy && (
              <div className="filter-item">
                <label>Nhóm theo</label>
                <SelectBox 
                  dataSource={[
                    { id: "class", label: "Lớp học" },
                    { id: "subject", label: "Môn học" },
                    { id: "grade", label: "Khối lớp" }
                  ]} 
                  valueExpr="id"
                  displayExpr="label"
                  value={filters.groupBy} 
                  onValueChanged={(e) => setFilters({ groupBy: e.value })}
                />
              </div>
            )}

            {showMaxSemesters && (
              <div className="filter-item">
                <label>Số học kỳ tối đa</label>
                <SelectBox 
                  dataSource={[2, 3, 4, 5, 6, 8]} 
                  value={filters.maxSemesters} 
                  onValueChanged={(e) => setFilters({ maxSemesters: e.value })}
                />
              </div>
            )}
          </div>
          
          <div className="filter-actions">
            <Button 
              variant="ghost" 
              onClick={() => setFilters({
                gradeLevels: [],
                classIds: [],
                subjectIds: [],
                teacherIds: [],
                riskLevel: undefined
              })}
            >
              <X size={14} /> Xóa bộ lọc nâng cao
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
