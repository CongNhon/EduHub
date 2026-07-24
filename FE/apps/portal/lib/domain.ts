import type { ApiResponse, PagedResponse } from "@eduhub/api-client";
import type { BadgeTone } from "@eduhub/ui";

export type Envelope<T> = ApiResponse<T>;
export type PagedEnvelope<T> = ApiResponse<PagedResponse<T>>;

export interface StudentDto { id: string; studentCode: string; fullName: string; dateOfBirth: string; status: string; version: number; currentClassId?: string | null; currentClassCode?: string | null; currentClassName?: string | null; guardianCount: number; accountEmail?: string | null; accountIsActive?: boolean | null; }
export interface StudentEnrollmentSummaryDto { id: string; classRoomId: string; classCode: string; className: string; semesterId: string; semesterName: string; status: string; enrolledAtUtc: string; endedAtUtc?: string | null; }
export interface StudentGuardianDto { linkId: string; parentUserId: string; fullName: string; email: string; phoneNumber?: string | null; relationship: string; isActive: boolean; }
export interface StudentDetailDto { student: StudentDto; enrollments: StudentEnrollmentSummaryDto[]; guardians: StudentGuardianDto[]; }
export interface ChildSummaryDto { id: string; studentCode: string; fullName: string; dateOfBirth: string; relationship: string; currentClassId?: string | null; currentClassCode?: string | null; currentClassName?: string | null; currentSemesterId?: string | null; currentSemesterName?: string | null; }
export interface UserAccountDto { id: string; email: string; fullName: string; referenceCode?: string | null; phoneNumber?: string | null; role: string; isActive: boolean; }
export interface SchoolProfileDto { code: string; name: string; logoUrl?: string | null; address?: string | null; email?: string | null; phoneNumber?: string | null; }
export interface AcademicYearDto { id: string; name: string; startDate: string; endDate: string; status: string; }
export interface SemesterDto { id: string; academicYearId: string; name: string; startDate: string; endDate: string; gradeEntryFrom: string; gradeEntryTo: string; status: string; }
export interface SubjectDto { id: string; subjectCode: string; name: string; credits: number; maxScore: number; isActive: boolean; }
export interface ClassRoomDto { id: string; classCode: string; name: string; academicYearId: string; gradeLevel: number; capacity: number; activeEnrollmentCount: number; isActive: boolean; }
export interface TeachingAssignmentSummaryDto { id: string; classRoomId: string; classCode: string; className: string; subjectId: string; subjectCode: string; subjectName: string; semesterId: string; semesterName: string; teacherId: string; teacherName: string; studentCount: number; gradebookStatus: string; isActive: boolean; }
export interface GradeComponentDto { id: string; subjectId: string; semesterId: string; name: string; weight: number; maxScore: number; displayOrder: number; isRequired: boolean; includeInGpa: boolean; version: number; isActive: boolean; }
export interface GradeConfigurationDto { subjectId: string; semesterId: string; version: number; isActive: boolean; totalWeight: number; components: GradeComponentDto[]; }
export interface NotificationDto { id: string; type: string; title: string; body: string; studentId?: string | null; assignmentId?: string | null; occurredAtUtc: string; readAtUtc?: string | null; isRead: boolean; }
export interface GradebookDto { assignmentId: string; classRoomId: string; classCode: string; className: string; subjectId: string; subjectCode: string; subjectName: string; semesterId: string; semesterName: string; teacherId: string; teacherName: string; status: string; components: { id: string; name: string; weight: number; maxScore: number; displayOrder: number; isRequired: boolean }[]; students: { studentId: string; studentCode: string; fullName: string; remark?: string | null; remarkVersion?: number | null; grades: { componentId: string; score?: number | null; status: string; version?: number | null; publicationVersion: number }[] }[]; }
export interface PublishedGradebookDto { studentId: string; studentCode: string; studentName: string; assignmentId: string; classCode: string; className: string; subjectCode: string; subjectName: string; semesterName: string; teacherName: string; publishedAtUtc?: string | null; remark?: string | null; grades: { componentId: string; componentName: string; weight: number; maxScore: number; score: number; publicationVersion: number }[]; }
export interface ReportJobDto { id: string; studentId: string; semesterId: string; status: string; checksumSha256?: string | null; policyVersion?: string | null; generatedAtUtc?: string | null; expiresAtUtc?: string | null; failureReason?: string | null; }
export interface ReportRequestDto { id: string; studentId: string; studentCode: string; studentName: string; semesterId: string; semesterName: string; requesterUserId: string; requesterName: string; reviewerUserId?: string | null; reviewerName?: string | null; reportJobId?: string | null; purpose: string; reviewNote?: string | null; status: string; jobStatus?: string | null; requestedAtUtc: string; reviewedAtUtc?: string | null; }
export interface ExternalSyncRecordDto { id: string; aggregateType: string; aggregateId: string; version: number; idempotencyKey: string; status: string; attempts: number; externalId?: string | null; externalVersion?: string | null; lastError?: string | null; nextRetryAtUtc?: string | null; succeededAtUtc?: string | null; }
export interface CurriculumSubjectQuotaDto { id: string; subjectId: string; subjectCode: string; subjectName: string; kind: string; annualPeriods: number; semester1Periods: number; semester2Periods: number; canDoublePeriod: boolean; maxPeriodsPerDay: number; includesHomeroom: boolean; preferredSession?: string | null; }
export interface CurriculumPlanDto { id: string; academicYearId: string; gradeLevel: number; name: string; totalWeeks: number; semester1Weeks: number; semester2Weeks: number; annualPeriodTotal: number; isActive: boolean; subjectQuotas: CurriculumSubjectQuotaDto[]; }
export interface TeacherCapabilityDto { id: string; teacherId: string; teacherName: string; subjectId: string; subjectCode: string; subjectName: string; priority: string; maxPeriodsPerWeek: number; isActive: boolean; }
export interface HomeroomAssignmentDto { id: string; classRoomId: string; classCode: string; className: string; teacherId: string; teacherName: string; isActive: boolean; }
export interface TimetableVersionDto { id: string; semesterId: string; semesterName: string; name: string; status: string; generatedAtUtc: string; publishedAtUtc?: string | null; entryCount: number; }
export interface TimetableWeekDto { weekNumber: number; startDate: string; endDate: string; isCurrent: boolean; }
export interface TimetableEntryDto { id: string; timetableVersionId: string; classRoomId: string; classCode: string; className: string; subjectId: string; subjectCode: string; subjectName: string; teacherId?: string | null; teacherName?: string | null; weekNumber: number; weekStartDate: string; weekEndDate: string; dayOfWeek: number; session: string; periodNumber: number; startTime: string; endTime: string; kind: string; countsTowardQuota: boolean; isLocked: boolean; note?: string | null; }
export interface GenerateTimetableDto { version: TimetableVersionDto; autoCreatedTeachingAssignments: number; autoCreatedHomeroomAssignments: number; entryCount: number; }
export interface StudentSelfProfileDto { studentId: string; studentCode: string; fullName: string; dateOfBirth: string; gender?: string | null; phoneNumber?: string | null; address?: string | null; status: string; version: number; currentClassId?: string | null; currentClassName?: string | null; currentSemesterId?: string | null; currentSemesterName?: string | null; }
export interface EvidenceUploadGrantDto { objectKey: string; uploadUrl: string; expiresAtUtc: string; usesDirectCloudUpload: boolean; }
export interface StudentProfileChangeRequestDto { id: string; studentId: string; studentCode: string; currentFullName: string; currentDateOfBirth: string; currentGender?: string | null; currentPhoneNumber?: string | null; currentAddress?: string | null; requestedFullName: string; requestedDateOfBirth: string; requestedGender?: string | null; requestedPhoneNumber?: string | null; requestedAddress?: string | null; reason: string; evidenceUrl: string; status: string; requesterName: string; reviewerName?: string | null; reviewNote?: string | null; requestedAtUtc: string; reviewedAtUtc?: string | null; }
export interface StudentImportRowDto { rowNumber: number; studentCode: string; success: boolean; errorCode?: string | null; errorMessage?: string | null; }
export interface StudentImportCredentialDto { email: string; role: string; temporaryPassword: string; }
export interface StudentImportDto { totalRows: number; successCount: number; errorCount: number; rows: StudentImportRowDto[]; temporaryCredentials: StudentImportCredentialDto[]; }
export interface AnalyticsSemesterDto { id: string; name: string; academicYearName: string; startDate: string; endDate: string; status: string; }
export interface AdminOverviewDto { semester: AnalyticsSemesterDto; availableSemesters: AnalyticsSemesterDto[]; generatedAtUtc: string; activeStudents: number; activeTeachers: number; activeParents: number; activeClasses: number; activeSubjects: number; pendingProfileChangeRequests: number; openReportRequests: number; pendingOutboxMessages: number; failedExternalSyncs: number; usersByRole: { role: string; count: number }[]; studentsByGradeLevel: { gradeLevel: number; studentCount: number }[]; }
export interface AdminAcademicAnalyticsDto { semester: AnalyticsSemesterDto; generatedAtUtc: string; averageNormalizedScore?: number | null; passRatePercentage?: number | null; publishedGradeCount: number; totalGradeCount: number; gradeDistribution: { label: string; fromInclusive: number; toExclusive?: number | null; count: number }[]; subjectPerformance: { subjectCode: string; subjectName: string; averageNormalizedScore?: number | null; passRatePercentage?: number | null; publishedGradeCount: number }[]; classPerformance: { classCode: string; className: string; gradeLevel: number; averageNormalizedScore?: number | null; passRatePercentage?: number | null; publishedGradeCount: number }[]; gradeStatuses: { status: string; count: number }[]; }
export interface AdminDataQualityDto { semester: AnalyticsSemesterDto; generatedAtUtc: string; totalFindings: number; criticalFindings: number; issues: { code: string; title: string; severity: string; count: number }[]; }
export interface SystemMonitoringDto { generatedAtUtc: string; cache: { hits: number; misses: number; failures: number; hitRatePercentage?: number | null }; hangfire: { servers: number; recurring: number; enqueued: number; scheduled: number; processing: number; succeeded: number; failed: number; deleted: number }; outbox: { pending: number; retried: number; oldestPendingAtUtc?: string | null }; externalSyncs: { status: string; count: number }[]; emailDigests: { status: string; count: number }[]; reportJobs: { status: string; count: number }[]; notificationsLast24Hours: number; }

export interface AdvancedMetricMetadataDto { metricVersion: string; riskModelVersion: string; qualityModelVersion: string; generatedAt: string; }
export interface CommonDecimalMetricDto { value?: number | null; previousValue?: number | null; absoluteChange?: number | null; percentageChange?: number | null; trend: string; }
export interface GrowthSummaryDto { totalCount: number; improvedCount: number; stableCount: number; declinedCount: number; meanGrowth?: number | null; medianGrowth?: number | null; }
export interface DataQualityScoreSummaryDto { overallScore: number; completeness: number; validity: number; consistency: number; integrity: number; uniqueness: number; freshness: number; }
export interface AdminAdvancedSummaryDto { metadata: AdvancedMetricMetadataDto; averageScore: CommonDecimalMetricDto; passRate: CommonDecimalMetricDto; excellentRate: CommonDecimalMetricDto; missingGradeRate: CommonDecimalMetricDto; growth: GrowthSummaryDto; dataQuality: DataQualityScoreSummaryDto; }

export interface ScoreDistributionMetricsDto { sampleSize: number; mean?: number | null; median?: number | null; min?: number | null; max?: number | null; standardDeviation?: number | null; variance?: number | null; p10?: number | null; q1?: number | null; q3?: number | null; p90?: number | null; interquartileRange?: number | null; }
export interface ScoreBucketMetricDto { code: string; name: string; count: number; percentage: number; }
export interface GroupedDistributionItemDto { groupKey: string; groupName: string; metrics: ScoreDistributionMetricsDto; }
export interface AcademicDistributionDto { metadata: AdvancedMetricMetadataDto; overall: ScoreDistributionMetricsDto; buckets: ScoreBucketMetricDto[]; grouped: GroupedDistributionItemDto[]; }

export interface AcademicTrendPointDto { semesterId: string; semesterName: string; academicYearStart: number; academicYearEnd: number; mean?: number | null; median?: number | null; standardDeviation?: number | null; passRate?: number | null; failureRate?: number | null; missingRate?: number | null; validScoreCount: number; studentCount: number; }
export interface AcademicTrendDto { metadata: AdvancedMetricMetadataDto; points: AcademicTrendPointDto[]; }

export interface StudentRiskSummaryDto { total: number; low: number; medium: number; high: number; critical: number; }
export interface StudentRiskReasonDto { code: string; message: string; }
export interface StudentRiskItemDto { studentId: string; studentCode: string; studentName: string; classId: string; classCode: string; className: string; gradeLevel: number; riskScore: number; riskLevel: string; currentAverage?: number | null; previousAverage?: number | null; growth?: number | null; failedSubjectCount: number; totalSubjectCount: number; missingGradeRate?: number | null; currentPercentileInGrade?: number | null; reasons: StudentRiskReasonDto[]; }
export interface StudentRiskDto { metadata: AdvancedMetricMetadataDto; summary: StudentRiskSummaryDto; items: StudentRiskItemDto[]; }

/** unwrapData lấy phần data từ ApiResponse chuẩn của backend. */
export function unwrapData<T>(response: Envelope<T>) { return response.data; }

/** formatDateTime chuyển UTC thành ngày giờ Việt Nam dễ đọc. */
export function formatDateTime(value?: string | null) { return value ? new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short", timeZone: "Asia/Ho_Chi_Minh" }).format(new Date(value)) : "—"; }

/** formatDate chuyển ISO date thành định dạng ngày Việt Nam. */
export function formatDate(value?: string | null) { return value ? new Intl.DateTimeFormat("vi-VN", { dateStyle: "short" }).format(new Date(`${value}T00:00:00`)) : "—"; }

/** statusTone ánh xạ trạng thái domain sang màu semantic. */
export function statusTone(status: string): BadgeTone {
  const normalized = status.toLowerCase();
  if (["active", "approved", "published", "locked", "completed", "succeeded"].includes(normalized)) return "success";
  if (["pending", "reviewing", "submitted", "generating", "processing", "queued", "planned", "reopened", "retryscheduled"].includes(normalized)) return "warning";
  if (["rejected", "failed", "failedpermanent", "withdrawn", "suspended"].includes(normalized)) return "danger";
  return "neutral";
}

/** statusLabel đổi enum backend thành nội dung tiếng Việt. */
export function statusLabel(status: string) {
  const labels: Record<string, string> = { Active: "Đang hoạt động", Planned: "Dự kiến", Pending: "Chờ duyệt", Reviewing: "Đang duyệt", Approved: "Đã duyệt", Rejected: "Đã từ chối", Generating: "Đang tạo PDF", Completed: "Hoàn tất", Archived: "Đã lưu trữ", Suspended: "Tạm dừng", Graduated: "Đã tốt nghiệp", Withdrawn: "Đã thôi học", Draft: "Bản nháp", Submitted: "Đã nộp", Published: "Đã công bố", Locked: "Đã khóa", Reopened: "Đang điều chỉnh", Queued: "Đang xếp hàng", Processing: "Đang xử lý", Succeeded: "Thành công", Failed: "Thất bại", RetryScheduled: "Sẽ thử lại", FailedPermanent: "Cần xử lý", Expired: "Đã hết hạn" };
  return labels[status] || status;
}
