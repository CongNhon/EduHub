"use client";

import {
    Badge,
    Button,
    Card,
    CardHeader,
    DataTable,
    Dialog,
    EmptyState,
    Field,
    Input,
    Select,
    type DataColumn,
} from "@eduhub/ui";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    BookOpen,
    CalendarClock,
    Check,
    GraduationCap,
    Lock,
    Plus,
    Send,
    Sparkles,
    UserRoundCheck,
    X,
} from "lucide-react";
import { useMemo, useState } from "react";
import { toast } from "sonner";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type {
    AcademicYearDto,
    ClassRoomDto,
    CurriculumPlanDto,
    Envelope,
    GenerateTimetableDto,
    HomeroomAssignmentDto,
    PagedEnvelope,
    SemesterDto,
    SubjectDto,
    TeacherCapabilityDto,
    TimetableEntryDto,
    TimetableVersionDto,
    TimetableWeekDto,
    UserAccountDto,
} from "@/lib/domain";
import { formatDateTime, statusLabel, statusTone } from "@/lib/domain";
import { TimetableGrid } from "./role-timetable";

type SchedulingTab = "timetable" | "curriculum" | "capabilities" | "homerooms";
interface QuotaDraft {
    subjectId: string;
    kind: number;
    annualPeriods: number;
    semester1Periods: number;
    semester2Periods: number;
    canDoublePeriod: boolean;
    maxPeriodsPerDay: number;
    includesHomeroom: boolean;
    preferredSession: number | null;
}
interface CapabilityGroupRow {
    id: string;
    label: string;
    reference: string;
    items: TeacherCapabilityDto[];
}
const emptyQuota: QuotaDraft = {
    subjectId: "",
    kind: 1,
    annualPeriods: 70,
    semester1Periods: 36,
    semester2Periods: 34,
    canDoublePeriod: true,
    maxPeriodsPerDay: 2,
    includesHomeroom: false,
    preferredSession: null,
};

/** SchedulingManagement vận hành chương trình khối, năng lực giáo viên, GVCN và phiên bản thời khóa biểu. */
export function SchedulingManagement() {
    const { request } = useSession();
    const queryClient = useQueryClient();
    const [tab, setTab] = useState<SchedulingTab>("timetable");
    const [semesterId, setSemesterId] = useState("");
    const [versionId, setVersionId] = useState("");
    const [classRoomId, setClassRoomId] = useState("");
    const [weekNumbers, setWeekNumbers] = useState<Record<string, number>>({});
    const [generateOpen, setGenerateOpen] = useState(false);
    const [versionName, setVersionName] = useState("");
    const [moving, setMoving] = useState<TimetableEntryDto | null>(null);
    const [moveSlot, setMoveSlot] = useState({
        weekNumber: 1,
        dayOfWeek: 1,
        session: 1,
        periodNumber: 1,
    });
    const [selectedTeacherId, setSelectedTeacherId] = useState("");
    const [capabilityView, setCapabilityView] = useState<"teacher" | "subject">("teacher");
    const [capabilityFilterId, setCapabilityFilterId] = useState("");
    const [planOpen, setPlanOpen] = useState(false);
    const [planMeta, setPlanMeta] = useState({
        academicYearId: "",
        gradeLevel: 10,
        name: "Chương trình khối 10",
        totalWeeks: 35,
        semester1Weeks: 18,
        semester2Weeks: 17,
    });
    const [quotas, setQuotas] = useState<QuotaDraft[]>([]);
    const [quotaDraft, setQuotaDraft] = useState<QuotaDraft>(emptyQuota);
    const [capabilityOpen, setCapabilityOpen] = useState(false);
    const [capability, setCapability] = useState({
        teacherId: "",
        subjectId: "",
        priority: 1,
        maxPeriodsPerWeek: 20,
    });
    const [homeroomClass, setHomeroomClass] = useState<ClassRoomDto | null>(
        null,
    );
    const [homeroomTeacherId, setHomeroomTeacherId] = useState("");

    const years = useQuery({
        queryKey: ["academic-years", "scheduling"],
        queryFn: () =>
            request<PagedEnvelope<AcademicYearDto>>(
                "/api/v1/academic-years?page=1&pageSize=100",
            ),
    });
    const semesters = useQuery({
        queryKey: ["semesters", "scheduling"],
        queryFn: () =>
            request<PagedEnvelope<SemesterDto>>(
                "/api/v1/semesters?page=1&pageSize=100",
            ),
    });
    const subjects = useQuery({
        queryKey: ["subjects", "scheduling"],
        queryFn: () =>
            request<PagedEnvelope<SubjectDto>>(
                "/api/v1/subjects?isActive=true&page=1&pageSize=100",
            ),
    });
    const classRooms = useQuery({
        queryKey: ["classes", "scheduling"],
        queryFn: () =>
            request<PagedEnvelope<ClassRoomDto>>(
                "/api/v1/classes?page=1&pageSize=100",
            ),
    });
    const teachers = useQuery({
        queryKey: ["users", "scheduling-teachers"],
        queryFn: () =>
            request<PagedEnvelope<UserAccountDto>>(
                "/api/v1/users?role=Teacher&isActive=true&page=1&pageSize=100",
            ),
    });
    const plans = useQuery({
        queryKey: ["curriculum-plans"],
        queryFn: () =>
            request<Envelope<CurriculumPlanDto[]>>("/api/v1/curriculum-plans"),
    });
    const capabilities = useQuery({
        queryKey: ["teacher-capabilities"],
        queryFn: () =>
            request<Envelope<TeacherCapabilityDto[]>>(
                "/api/v1/teacher-capabilities",
            ),
    });
    const homerooms = useQuery({
        queryKey: ["homeroom-assignments"],
        queryFn: () =>
            request<Envelope<HomeroomAssignmentDto[]>>(
                "/api/v1/homeroom-assignments",
            ),
    });
    const semesterItems = semesters.data?.data.items || [];
    const classItems = classRooms.data?.data.items || [];
    const activeSemesterId = semesterItems.some((item) => item.id === semesterId)
        ? semesterId
        : semesterItems[0]?.id || "";
    const activeClassRoomId = classItems.some((item) => item.id === classRoomId)
        ? classRoomId
        : classItems[0]?.id || "";
    const versions = useQuery({
        queryKey: ["timetable-versions", activeSemesterId],
        queryFn: () =>
            request<Envelope<TimetableVersionDto[]>>(
                `/api/v1/timetables/versions?semesterId=${activeSemesterId}`,
            ),
        enabled: Boolean(activeSemesterId),
    });
    const versionItems = versions.data?.data || [];
    const activeVersionId = versionItems.some((item) => item.id === versionId)
        ? versionId
        : versionItems[0]?.id || "";
    const weeks = useQuery({
        queryKey: ["timetable-weeks", activeSemesterId],
        queryFn: () =>
            request<Envelope<TimetableWeekDto[]>>(
                `/api/v1/timetables/weeks?semesterId=${activeSemesterId}`,
            ),
        enabled: Boolean(activeSemesterId),
    });
    const weekItems = weeks.data?.data || [];
    const currentWeek = weekItems.find((item) => item.isCurrent);
    const storedWeekNumber = weekNumbers[activeSemesterId];
    const weekNumber = weekItems.some((item) => item.weekNumber === storedWeekNumber)
        ? storedWeekNumber!
        : currentWeek?.weekNumber ?? weekItems[0]?.weekNumber ?? 1;
    const setWeekNumber = (value: number) => {
        if (!activeSemesterId) return;
        setWeekNumbers((current) => ({ ...current, [activeSemesterId]: value }));
    };
    const entries = useQuery({
        queryKey: ["timetable-entries", activeVersionId, activeClassRoomId, weekNumber],
        queryFn: () =>
            request<Envelope<TimetableEntryDto[]>>(
                `/api/v1/timetables/${activeVersionId}/entries?classRoomId=${activeClassRoomId}&weekNumber=${weekNumber}`,
            ),
        enabled: Boolean(activeVersionId && activeClassRoomId),
    });

    const fail = (error: unknown) => toast.error(sessionErrorMessage(error));
    const createPlan = useMutation({
        mutationFn: () => {
            if (!quotas.length)
                throw new Error("Chương trình cần ít nhất một môn học.");
            return request<Envelope<CurriculumPlanDto>>(
                "/api/v1/curriculum-plans",
                {
                    method: "POST",
                    body: JSON.stringify({
                        ...planMeta,
                        subjectQuotas: quotas,
                    }),
                },
            );
        },
        onSuccess: async () => {
            toast.success("Đã tạo chương trình học theo khối.");
            setPlanOpen(false);
            setQuotas([]);
            await queryClient.invalidateQueries({
                queryKey: ["curriculum-plans"],
            });
        },
        onError: fail,
    });
    const createCapability = useMutation({
        mutationFn: () =>
            request<Envelope<TeacherCapabilityDto>>(
                "/api/v1/teacher-capabilities",
                { method: "POST", body: JSON.stringify(capability) },
            ),
        onSuccess: async () => {
            toast.success("Đã khai báo năng lực giảng dạy.");
            setCapabilityOpen(false);
            await queryClient.invalidateQueries({
                queryKey: ["teacher-capabilities"],
            });
        },
        onError: fail,
    });
    const assignHomeroom = useMutation({
        mutationFn: () => {
            if (!homeroomClass) throw new Error("Chưa chọn lớp.");
            return request<Envelope<HomeroomAssignmentDto>>(
                `/api/v1/homeroom-assignments/classes/${homeroomClass.id}`,
                {
                    method: "POST",
                    body: JSON.stringify({ teacherId: homeroomTeacherId }),
                },
            );
        },
        onSuccess: async () => {
            toast.success("Đã phân công giáo viên chủ nhiệm.");
            setHomeroomClass(null);
            setHomeroomTeacherId("");
            await queryClient.invalidateQueries({
                queryKey: ["homeroom-assignments"],
            });
        },
        onError: fail,
    });
    const generate = useMutation({
        mutationFn: () =>
            request<Envelope<GenerateTimetableDto>>(
                "/api/v1/timetables/generate",
                {
                    method: "POST",
                    body: JSON.stringify({ semesterId: activeSemesterId, name: versionName }),
                },
            ),
        onSuccess: async (result) => {
            toast.success(
                `Đã sinh ${result.data.entryCount} tiết và tạo ${result.data.autoCreatedTeachingAssignments} phân công.`,
            );
            setGenerateOpen(false);
            setVersionId(result.data.version.id);
            await queryClient.invalidateQueries({
                queryKey: ["timetable-versions", activeSemesterId],
            });
        },
        onError: fail,
    });
    const publish = useMutation({
        mutationFn: (id: string) =>
            request<Envelope<TimetableVersionDto>>(
                `/api/v1/timetables/${id}/publish`,
                { method: "POST" },
            ),
        onSuccess: async () => {
            toast.success("Đã công bố thời khóa biểu cho các role liên quan.");
            await queryClient.invalidateQueries({
                queryKey: ["timetable-versions", activeSemesterId],
            });
        },
        onError: fail,
    });
    const move = useMutation({
        mutationFn: () => {
            if (!moving) throw new Error("Chưa chọn tiết học.");
            return request<Envelope<TimetableEntryDto>>(
                `/api/v1/timetables/entries/${moving.id}/slot`,
                { method: "PUT", body: JSON.stringify(moveSlot) },
            );
        },
        onSuccess: async () => {
            toast.success("Đã hoán đổi hai tiết học và giữ lịch lớp đầy đủ.");
            setMoving(null);
            await queryClient.invalidateQueries({
                queryKey: ["timetable-entries", activeVersionId, activeClassRoomId],
            });
        },
        onError: fail,
    });
    const assignClassSubjectTeacher = useMutation({
        mutationFn: () => {
            if (!moving || !selectedTeacherId)
                throw new Error("Chưa chọn giáo viên.");
            return request<Envelope<TimetableEntryDto>>(
                `/api/v1/timetables/entries/${moving.id}/subject-teacher`,
                {
                    method: "PUT",
                    body: JSON.stringify({ teacherId: selectedTeacherId }),
                },
            );
        },
        onSuccess: async (result) => {
            toast.success("Đã đổi giáo viên cho toàn bộ môn học của lớp trong học kỳ.");
            setMoving(result.data);
            await queryClient.invalidateQueries({
                queryKey: ["timetable-entries", activeVersionId, activeClassRoomId],
            });
        },
        onError: fail,
    });
    const toggleLock = useMutation({
        mutationFn: (entry: TimetableEntryDto) =>
            request<Envelope<TimetableEntryDto>>(
                `/api/v1/timetables/entries/${entry.id}/lock`,
                {
                    method: "PUT",
                    body: JSON.stringify({ isLocked: !entry.isLocked }),
                },
            ),
        onSuccess: async (result) => {
            toast.success(
                result.data.isLocked
                    ? "Đã khóa tiết học."
                    : "Đã mở khóa tiết học.",
            );
            setMoving(result.data);
            await queryClient.invalidateQueries({
                queryKey: ["timetable-entries", activeVersionId, activeClassRoomId],
            });
        },
        onError: fail,
    });

    const yearItems = years.data?.data.items || [];
    const subjectItems = subjects.data?.data.items || [];
    const teacherItems = teachers.data?.data.items || [];
    const selectedVersion = versionItems.find((item) => item.id === activeVersionId);
    const selectedWeek = weekItems.find((item) => item.weekNumber === weekNumber);
    const formatShortDate = (value: string) =>
        new Intl.DateTimeFormat("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric" })
            .format(new Date(`${value}T00:00:00`));
    const eligibleTeacherIds = new Set(
        (capabilities.data?.data || [])
            .filter((item) => item.isActive && item.subjectId === moving?.subjectId)
            .map((item) => item.teacherId),
    );
    const currentClassHomeroomTeacherId = homerooms.data?.data.find(
        (item) => item.classRoomId === moving?.classRoomId && item.isActive,
    )?.teacherId;
    const eligibleTeachers = teacherItems.filter(
        (item) => eligibleTeacherIds.has(item.id) && item.id !== currentClassHomeroomTeacherId,
    );
    const planColumns = useMemo<DataColumn<CurriculumPlanDto>[]>(
        () => [
            {
                key: "plan",
                header: "Chương trình",
                cell: (row) => (
                    <div className="entity-cell">
                        <span>
                            <BookOpen size={17} />
                        </span>
                        <div>
                            <b>{row.name}</b>
                            <small>
                                Khối {row.gradeLevel} ·{" "}
                                {row.subjectQuotas.length} môn
                            </small>
                        </div>
                    </div>
                ),
            },
            {
                key: "weeks",
                header: "Thời lượng",
                cell: (row) =>
                    `${row.totalWeeks} tuần (${row.semester1Weeks} + ${row.semester2Weeks})`,
            },
            {
                key: "periods",
                header: "Tổng tiết/năm",
                cell: (row) => row.annualPeriodTotal,
            },
            {
                key: "status",
                header: "Trạng thái",
                cell: (row) => (
                    <Badge tone={row.isActive ? "success" : "neutral"}>
                        {row.isActive ? "Đang áp dụng" : "Ngừng áp dụng"}
                    </Badge>
                ),
            },
        ],
        [],
    );
    const capabilityGroups = useMemo<CapabilityGroupRow[]>(() => {
        const source = (capabilities.data?.data || []).filter((item) => item.isActive);
        const groups = new Map<string, CapabilityGroupRow>();
        for (const item of source) {
            const id = capabilityView === "teacher" ? item.teacherId : item.subjectId;
            const label = capabilityView === "teacher" ? item.teacherName : item.subjectName;
            const reference = capabilityView === "teacher" ? "Hồ sơ năng lực" : item.subjectCode;
            const group = groups.get(id) || { id, label, reference, items: [] };
            group.items.push(item);
            groups.set(id, group);
        }
        return [...groups.values()]
            .filter((group) => !capabilityFilterId || group.id === capabilityFilterId)
            .sort((left, right) => left.label.localeCompare(right.label, "vi"));
    }, [capabilities.data, capabilityFilterId, capabilityView]);
    const capabilityFilterOptions = useMemo(() => {
        const options = new Map<string, string>();
        for (const item of (capabilities.data?.data || []).filter((entry) => entry.isActive)) {
            options.set(
                capabilityView === "teacher" ? item.teacherId : item.subjectId,
                capabilityView === "teacher" ? item.teacherName : item.subjectName,
            );
        }
        return [...options.entries()].sort((left, right) =>
            left[1].localeCompare(right[1], "vi"),
        );
    }, [capabilities.data, capabilityView]);
    const capabilityColumns = useMemo<DataColumn<CapabilityGroupRow>[]>(
        () => [
            {
                key: "group",
                header: capabilityView === "teacher" ? "Giáo viên" : "Môn học",
                cell: (row) => (
                    <div className="entity-cell">
                        <span><GraduationCap size={17} /></span>
                        <div><b>{row.label}</b><small>{row.reference}</small></div>
                    </div>
                ),
            },
            {
                key: "capabilities",
                header: capabilityView === "teacher" ? "Các môn có thể dạy" : "Giáo viên có thể dạy",
                cell: (row) => (
                    <div className="capability-chip-list">
                        {row.items.map((item) => (
                            <span className="capability-chip" key={item.id}>
                                <b>{capabilityView === "teacher" ? item.subjectName : item.teacherName}</b>
                                <small>{item.priority === "Primary" ? "Chính" : "Phụ"} · {item.maxPeriodsPerWeek} tiết/tuần</small>
                            </span>
                        ))}
                    </div>
                ),
            },
            {
                key: "count",
                header: "Tổng",
                cell: (row) => `${row.items.length} ${capabilityView === "teacher" ? "môn" : "giáo viên"}`,
            },
        ],
        [capabilityView],
    );
    const homeroomColumns = useMemo<DataColumn<ClassRoomDto>[]>(
        () => [
            {
                key: "class",
                header: "Lớp",
                cell: (row) => (
                    <div className="entity-cell">
                        <span>
                            <UserRoundCheck size={17} />
                        </span>
                        <div>
                            <b>{row.name}</b>
                            <small>
                                Khối {row.gradeLevel} ·{" "}
                                {row.activeEnrollmentCount} học sinh
                            </small>
                        </div>
                    </div>
                ),
            },
            {
                key: "teacher",
                header: "Giáo viên chủ nhiệm",
                cell: (row) =>
                    homerooms.data?.data.find(
                        (item) => item.classRoomId === row.id,
                    )?.teacherName || "Chưa phân công",
            },
            {
                key: "action",
                header: "",
                className: "table-action",
                cell: (row) => (
                    <Button
                        variant="outline"
                        onClick={() => {
                            setHomeroomClass(row);
                            setHomeroomTeacherId(
                                homerooms.data?.data.find(
                                    (item) => item.classRoomId === row.id,
                                )?.teacherId || "",
                            );
                        }}
                    >
                        Chọn GVCN
                    </Button>
                ),
            },
        ],
        [homerooms.data],
    );

    const loading =
        years.isLoading ||
        semesters.isLoading ||
        subjects.isLoading ||
        classRooms.isLoading ||
        teachers.isLoading ||
        plans.isLoading ||
        capabilities.isLoading ||
        homerooms.isLoading ||
        versions.isLoading ||
        weeks.isLoading ||
        entries.isLoading;
    const error =
        years.error ||
        semesters.error ||
        subjects.error ||
        classRooms.error ||
        teachers.error ||
        plans.error ||
        capabilities.error ||
        homerooms.error ||
        versions.error ||
        weeks.error ||
        entries.error;
    if (loading)
        return (
            <>
                <PageHeader
                    eyebrow="ĐIỀU PHỐI HỌC VỤ"
                    title="Chương trình & Thời khóa biểu"
                />
                <LoadingPanel rows={9} />
            </>
        );
    if (error)
        return (
            <>
                <PageHeader
                    eyebrow="ĐIỀU PHỐI HỌC VỤ"
                    title="Chương trình & Thời khóa biểu"
                />
                <ErrorPanel message={sessionErrorMessage(error)} />
            </>
        );

    const addQuota = () => {
        if (!quotaDraft.subjectId) return toast.error("Hãy chọn môn học.");
        if (quotas.some((item) => item.subjectId === quotaDraft.subjectId))
            return toast.error("Môn học đã có trong chương trình.");
        setQuotas((items) => [...items, quotaDraft]);
        setQuotaDraft(emptyQuota);
    };
    return (
        <div className="page-stack">
            <PageHeader
                eyebrow="ĐIỀU PHỐI HỌC VỤ"
                title="Chương trình & Thời khóa biểu"
                description="Quản lý quota môn theo khối, năng lực giáo viên, chủ nhiệm và lịch từng tuần thực học."
            />
            <div className="segmented-tabs" role="tablist">
                <button
                    role="tab"
                    aria-selected={tab === "timetable"}
                    onClick={() => setTab("timetable")}
                >
                    <CalendarClock size={16} /> Thời khóa biểu
                </button>
                <button
                    role="tab"
                    aria-selected={tab === "curriculum"}
                    onClick={() => setTab("curriculum")}
                >
                    <BookOpen size={16} /> Chương trình khối
                </button>
                <button
                    role="tab"
                    aria-selected={tab === "capabilities"}
                    onClick={() => setTab("capabilities")}
                >
                    <GraduationCap size={16} /> Năng lực giáo viên
                </button>
                <button
                    role="tab"
                    aria-selected={tab === "homerooms"}
                    onClick={() => setTab("homerooms")}
                >
                    <UserRoundCheck size={16} /> Giáo viên chủ nhiệm
                </button>
            </div>
            {tab === "timetable" ? (
                <>
                    <Card>
                        <CardHeader
                            title="Phiên bản thời khóa biểu"
                            description="Sinh bản nháp, chỉnh thủ công rồi công bố."
                            action={
                                <Button
                                    onClick={() => {
                                        setVersionName(
                                            `TKB ${semesterItems.find((item) => item.id === activeSemesterId)?.name || "học kỳ"} · ${new Date().toLocaleDateString("vi-VN")}`,
                                        );
                                        setGenerateOpen(true);
                                    }}
                                    disabled={!activeSemesterId}
                                >
                                    <Sparkles size={16} /> Sinh tự động
                                </Button>
                            }
                        />
                        <div className="timetable-toolbar">
                            <Select
                                value={activeSemesterId}
                                onChange={(event) => {
                                    setSemesterId(event.target.value);
                                    setVersionId("");
                                }}
                            >
                                <option value="">Chọn học kỳ</option>
                                {semesterItems.map((item) => (
                                    <option value={item.id} key={item.id}>
                                        {item.name}
                                    </option>
                                ))}
                            </Select>
                            <Select
                                value={activeVersionId}
                                onChange={(event) =>
                                    setVersionId(event.target.value)
                                }
                            >
                                <option value="">Chọn phiên bản</option>
                                {versionItems.map((item) => (
                                    <option value={item.id} key={item.id}>
                                        {item.name} · {statusLabel(item.status)}
                                    </option>
                                ))}
                            </Select>
                            <Select
                                value={activeClassRoomId}
                                onChange={(event) =>
                                    setClassRoomId(event.target.value)
                                }
                            >
                                <option value="">Chọn lớp</option>
                                {classItems.map((item) => (
                                    <option value={item.id} key={item.id}>
                                        {item.name}
                                    </option>
                                ))}
                            </Select>
                            {selectedVersion ? (
                                <Badge
                                    tone={statusTone(selectedVersion.status)}
                                >
                                    {statusLabel(selectedVersion.status)}
                                </Badge>
                            ) : null}
                            {selectedVersion?.status === "Draft" ? (
                                <Button
                                    onClick={() =>
                                        publish.mutate(selectedVersion.id)
                                    }
                                    loading={publish.isPending}
                                >
                                    <Send size={16} /> Công bố
                                </Button>
                            ) : null}
                        </div>
                    </Card>
                    {activeVersionId && activeClassRoomId ? (
                        <>
                            <div className="timetable-heading">
                                <div className="timetable-week-controls">
                                    <Select
                                        value={weekNumber}
                                        onChange={(event) =>
                                            setWeekNumber(Number(event.target.value))
                                        }
                                        aria-label="Tuần học"
                                    >
                                        {weekItems.map((item) => (
                                            <option value={item.weekNumber} key={item.weekNumber}>
                                                Tuần {item.weekNumber} · {formatShortDate(item.startDate)} - {formatShortDate(item.endDate)}
                                                {item.isCurrent ? " · Hiện tại" : ""}
                                            </option>
                                        ))}
                                    </Select>
                                    {currentWeek && currentWeek.weekNumber !== weekNumber ? (
                                        <Button
                                            variant="outline"
                                            onClick={() => setWeekNumber(currentWeek.weekNumber)}
                                        >
                                            Tuần hiện tại
                                        </Button>
                                    ) : null}
                                </div>
                                <span>
                                    Tuần {selectedWeek?.weekNumber || weekNumber}
                                    {selectedWeek
                                        ? ` · ${formatShortDate(selectedWeek.startDate)} - ${formatShortDate(selectedWeek.endDate)}`
                                        : ""}
                                    {selectedWeek?.isCurrent ? " · hiện tại" : ""} · tạo{" "}
                                    {formatDateTime(
                                        selectedVersion?.generatedAtUtc,
                                    )}
                                </span>
                            </div>
                            <TimetableGrid
                                entries={entries.data?.data || []}
                                onEntryClick={
                                    selectedVersion?.status === "Draft"
                                        ? (entry) => {
                                              setMoving(entry);
                                              setSelectedTeacherId(entry.teacherId || "");
                                              setMoveSlot({
                                                  weekNumber: entry.weekNumber,
                                                  dayOfWeek: entry.dayOfWeek,
                                                  session:
                                                      entry.session ===
                                                      "Morning"
                                                          ? 1
                                                          : 2,
                                                  periodNumber:
                                                      entry.periodNumber,
                                              });
                                          }
                                        : undefined
                                }
                            />
                        </>
                    ) : (
                        <Card>
                            <EmptyState
                                icon={CalendarClock}
                                title="Chọn đủ học kỳ, phiên bản và lớp"
                                description="Sau khi sinh tự động, chọn một lớp và tuần cụ thể để kiểm tra lịch."
                            />
                        </Card>
                    )}
                </>
            ) : null}
            {tab === "curriculum" ? (
                <Card>
                    <CardHeader
                        title="Chương trình theo khối"
                        description="Quota năm được tách theo học kỳ và phân bổ đều vào từng tuần thực học."
                        action={
                            <Button
                                onClick={() => {
                                    setPlanMeta({
                                        academicYearId: yearItems[0]?.id || "",
                                        gradeLevel: 10,
                                        name: "Chương trình khối 10",
                                        totalWeeks: 35,
                                        semester1Weeks: 18,
                                        semester2Weeks: 17,
                                    });
                                    const homeroomSubject = subjectItems.find(
                                        (subject) =>
                                            subject.subjectCode === "HOMEROOM",
                                    );
                                    setQuotas(
                                        homeroomSubject
                                            ? [
                                                  {
                                                      ...emptyQuota,
                                                      subjectId:
                                                          homeroomSubject.id,
                                                       annualPeriods: 35,
                                                       semester1Periods: 18,
                                                       semester2Periods: 17,
                                                       canDoublePeriod: false,
                                                      maxPeriodsPerDay: 1,
                                                      includesHomeroom: true,
                                                      preferredSession: 1,
                                                  },
                                              ]
                                            : [],
                                    );
                                    setPlanOpen(true);
                                }}
                            >
                                <Plus size={16} /> Tạo chương trình
                            </Button>
                        }
                    />
                    <DataTable
                        columns={planColumns}
                        rows={plans.data?.data || []}
                        rowKey={(row) => row.id}
                        empty={
                            <EmptyState
                                icon={BookOpen}
                                title="Chưa có chương trình"
                                description="Tạo quota môn cho từng khối trước khi sinh lịch."
                            />
                        }
                    />
                </Card>
            ) : null}
            {tab === "capabilities" ? (
                <Card>
                    <CardHeader
                        title="Năng lực giảng dạy"
                        description="Mỗi giáo viên có ít nhất một môn chính và có thể có môn phụ."
                        action={
                            <Button onClick={() => setCapabilityOpen(true)}>
                                <Plus size={16} /> Thêm năng lực
                            </Button>
                        }
                    />
                    <div className="capability-toolbar">
                        <div className="segmented-tabs" role="tablist">
                            <button
                                role="tab"
                                aria-selected={capabilityView === "teacher"}
                                onClick={() => {
                                    setCapabilityView("teacher");
                                    setCapabilityFilterId("");
                                }}
                            >
                                Theo giáo viên
                            </button>
                            <button
                                role="tab"
                                aria-selected={capabilityView === "subject"}
                                onClick={() => {
                                    setCapabilityView("subject");
                                    setCapabilityFilterId("");
                                }}
                            >
                                Theo môn học
                            </button>
                        </div>
                        <Select
                            value={capabilityFilterId}
                            onChange={(event) => setCapabilityFilterId(event.target.value)}
                            aria-label="Lọc năng lực giảng dạy"
                        >
                            <option value="">Tất cả {capabilityView === "teacher" ? "giáo viên" : "môn học"}</option>
                            {capabilityFilterOptions.map(([id, label]) => (
                                <option value={id} key={id}>{label}</option>
                            ))}
                        </Select>
                    </div>
                    <DataTable
                        columns={capabilityColumns}
                        rows={capabilityGroups}
                        rowKey={(row) => row.id}
                        empty={
                            <EmptyState
                                icon={GraduationCap}
                                title="Chưa có năng lực giảng dạy"
                                description="Khai báo môn giáo viên có thể dạy trước khi tự động phân công."
                            />
                        }
                    />
                </Card>
            ) : null}
            {tab === "homerooms" ? (
                <Card>
                    <CardHeader
                        title="Chủ nhiệm theo lớp"
                        description="Hệ thống có thể tự xếp phần còn thiếu khi sinh lịch; học vụ luôn có thể chọn thủ công."
                    />
                    <DataTable
                        columns={homeroomColumns}
                        rows={classItems}
                        rowKey={(row) => row.id}
                        empty={
                            <EmptyState
                                icon={UserRoundCheck}
                                title="Chưa có lớp"
                                description="Tạo lớp trước khi phân công giáo viên chủ nhiệm."
                            />
                        }
                    />
                </Card>
            ) : null}

            <Dialog
                open={generateOpen}
                onOpenChange={setGenerateOpen}
                title="Sinh thời khóa biểu tự động"
                description="Hệ thống tự bù giáo viên/GVCN còn thiếu và tối ưu lịch cho toàn bộ tuần trong học kỳ."
                footer={
                    <>
                        <Button
                            variant="ghost"
                            onClick={() => setGenerateOpen(false)}
                        >
                            Hủy
                        </Button>
                        <Button
                            onClick={() => generate.mutate()}
                            loading={generate.isPending}
                            disabled={!activeSemesterId || !versionName.trim()}
                        >
                            <Sparkles size={16} /> Bắt đầu xếp lịch
                        </Button>
                    </>
                }
            >
                <div className="dialog-form">
                    <Field label="Học kỳ" htmlFor="generateSemester">
                        <Select
                            id="generateSemester"
                            value={activeSemesterId}
                            onChange={(event) =>
                                setSemesterId(event.target.value)
                            }
                        >
                            {semesterItems.map((item) => (
                                <option key={item.id} value={item.id}>
                                    {item.name}
                                </option>
                            ))}
                        </Select>
                    </Field>
                    <Field label="Tên phiên bản" htmlFor="versionName">
                        <Input
                            id="versionName"
                            value={versionName}
                            onChange={(event) =>
                                setVersionName(event.target.value)
                            }
                        />
                    </Field>
                    <p className="security-note">
                        Bản mới luôn ở trạng thái nháp. Học sinh, phụ huynh và
                        giáo viên chỉ thấy lịch sau khi học vụ công bố.
                    </p>
                </div>
            </Dialog>
            <Dialog
                open={Boolean(moving)}
                onOpenChange={(open) => {
                    if (!open) setMoving(null);
                }}
                title="Điều chỉnh tiết học"
                description={
                    moving
                        ? `${moving.className} · ${moving.subjectName} · ${moving.teacherName || "Chưa phân công"}`
                        : undefined
                }
                footer={
                    <>
                        <Button
                            variant="outline"
                            onClick={() => moving && toggleLock.mutate(moving)}
                            loading={toggleLock.isPending}
                        >
                            {moving?.isLocked ? (
                                <X size={16} />
                            ) : (
                                <Lock size={16} />
                            )}{" "}
                            {moving?.isLocked ? "Mở khóa" : "Khóa tiết"}
                        </Button>
                        <Button
                            variant="outline"
                            onClick={() => assignClassSubjectTeacher.mutate()}
                            loading={assignClassSubjectTeacher.isPending}
                            disabled={moving?.isLocked || !selectedTeacherId}
                        >
                            Lưu giáo viên
                        </Button>
                        <Button
                            onClick={() => move.mutate()}
                            loading={move.isPending}
                            disabled={moving?.isLocked}
                        >
                            Hoán đổi vị trí
                        </Button>
                    </>
                }
            >
                <div className="form-grid-2">
                    <Field label="Tuần học" htmlFor="moveWeek">
                        <Select
                            id="moveWeek"
                            value={moveSlot.weekNumber}
                            onChange={(event) =>
                                setMoveSlot((item) => ({
                                    ...item,
                                    weekNumber: Number(event.target.value),
                                }))
                            }
                        >
                            {weekItems.map((item) => (
                                <option value={item.weekNumber} key={item.weekNumber}>
                                    Tuần {item.weekNumber} · {formatShortDate(item.startDate)} - {formatShortDate(item.endDate)}
                                    {item.isCurrent ? " · Hiện tại" : ""}
                                </option>
                            ))}
                        </Select>
                    </Field>
                    <Field label="Ngày" htmlFor="moveDay">
                        <Select
                            id="moveDay"
                            value={moveSlot.dayOfWeek}
                            onChange={(event) =>
                                setMoveSlot((item) => ({
                                    ...item,
                                    dayOfWeek: Number(event.target.value),
                                }))
                            }
                        >
                            {[
                                "Thứ Hai",
                                "Thứ Ba",
                                "Thứ Tư",
                                "Thứ Năm",
                                "Thứ Sáu",
                                "Thứ Bảy",
                            ].map((day, index) => (
                                <option value={index + 1} key={day}>
                                    {day}
                                </option>
                            ))}
                        </Select>
                    </Field>
                    <Field label="Buổi" htmlFor="moveSession">
                        <Select
                            id="moveSession"
                            value={moveSlot.session}
                            onChange={(event) =>
                                setMoveSlot((item) => ({
                                    ...item,
                                    session: Number(event.target.value),
                                }))
                            }
                        >
                            <option value="1">Sáng</option>
                            <option value="2">Chiều</option>
                        </Select>
                    </Field>
                    <Field label="Tiết" htmlFor="movePeriod">
                        <Input
                            id="movePeriod"
                            type="number"
                            min="1"
                            max="5"
                            value={moveSlot.periodNumber}
                            onChange={(event) =>
                                setMoveSlot((item) => ({
                                    ...item,
                                    periodNumber: Number(event.target.value),
                                }))
                            }
                        />
                    </Field>
                    <Field label="Giáo viên môn học của lớp" htmlFor="moveTeacher">
                        <Select
                            id="moveTeacher"
                            value={selectedTeacherId}
                            onChange={(event) => setSelectedTeacherId(event.target.value)}
                        >
                            <option value="">Chọn giáo viên đủ năng lực</option>
                            {eligibleTeachers.map((teacher) => (
                                <option value={teacher.id} key={teacher.id}>
                                    {teacher.fullName}
                                    {teacher.id === moving?.teacherId ? " · Hiện tại" : ""}
                                </option>
                            ))}
                        </Select>
                    </Field>
                </div>
            </Dialog>
            <Dialog
                open={capabilityOpen}
                onOpenChange={setCapabilityOpen}
                title="Thêm năng lực giảng dạy"
                description="Môn chính được ưu tiên trước môn phụ khi tự động phân giáo viên."
                footer={
                    <>
                        <Button
                            variant="ghost"
                            onClick={() => setCapabilityOpen(false)}
                        >
                            Hủy
                        </Button>
                        <Button
                            onClick={() => createCapability.mutate()}
                            loading={createCapability.isPending}
                            disabled={
                                !capability.teacherId || !capability.subjectId
                            }
                        >
                            Lưu năng lực
                        </Button>
                    </>
                }
            >
                <div className="dialog-form">
                    <Field label="Giáo viên" htmlFor="capTeacher">
                        <Select
                            id="capTeacher"
                            value={capability.teacherId}
                            onChange={(event) =>
                                setCapability((item) => ({
                                    ...item,
                                    teacherId: event.target.value,
                                }))
                            }
                        >
                            <option value="">Chọn giáo viên</option>
                            {teacherItems.map((item) => (
                                <option value={item.id} key={item.id}>
                                    {item.fullName} ·{" "}
                                    {item.referenceCode || item.email}
                                </option>
                            ))}
                        </Select>
                    </Field>
                    <Field label="Môn học" htmlFor="capSubject">
                        <Select
                            id="capSubject"
                            value={capability.subjectId}
                            onChange={(event) =>
                                setCapability((item) => ({
                                    ...item,
                                    subjectId: event.target.value,
                                }))
                            }
                        >
                            <option value="">Chọn môn</option>
                            {subjectItems.map((item) => (
                                <option value={item.id} key={item.id}>
                                    {item.subjectCode} · {item.name}
                                </option>
                            ))}
                        </Select>
                    </Field>
                    <div className="form-grid-2">
                        <Field label="Mức ưu tiên" htmlFor="capPriority">
                            <Select
                                id="capPriority"
                                value={capability.priority}
                                onChange={(event) =>
                                    setCapability((item) => ({
                                        ...item,
                                        priority: Number(event.target.value),
                                    }))
                                }
                            >
                                <option value="1">Môn chính</option>
                                <option value="2">Môn phụ</option>
                            </Select>
                        </Field>
                        <Field label="Tối đa tiết/tuần" htmlFor="capLoad">
                            <Input
                                id="capLoad"
                                type="number"
                                min="1"
                                max="40"
                                value={capability.maxPeriodsPerWeek}
                                onChange={(event) =>
                                    setCapability((item) => ({
                                        ...item,
                                        maxPeriodsPerWeek: Number(
                                            event.target.value,
                                        ),
                                    }))
                                }
                            />
                        </Field>
                    </div>
                </div>
            </Dialog>
            <Dialog
                open={Boolean(homeroomClass)}
                onOpenChange={(open) => {
                    if (!open) setHomeroomClass(null);
                }}
                title="Phân công giáo viên chủ nhiệm"
                description={homeroomClass?.name}
                footer={
                    <>
                        <Button
                            variant="ghost"
                            onClick={() => setHomeroomClass(null)}
                        >
                            Hủy
                        </Button>
                        <Button
                            onClick={() => assignHomeroom.mutate()}
                            loading={assignHomeroom.isPending}
                            disabled={!homeroomTeacherId}
                        >
                            <Check size={16} /> Lưu GVCN
                        </Button>
                    </>
                }
            >
                <Field label="Giáo viên" htmlFor="homeroomTeacher">
                    <Select
                        id="homeroomTeacher"
                        value={homeroomTeacherId}
                        onChange={(event) =>
                            setHomeroomTeacherId(event.target.value)
                        }
                    >
                        <option value="">Chọn giáo viên</option>
                        {teacherItems.map((item) => (
                            <option value={item.id} key={item.id}>
                                {item.fullName} ·{" "}
                                {item.referenceCode || item.email}
                            </option>
                        ))}
                    </Select>
                </Field>
            </Dialog>
            <Dialog
                open={planOpen}
                onOpenChange={setPlanOpen}
                title="Tạo chương trình theo khối"
                description="Cấu hình 35 tuần và quota từng môn trước khi sinh lịch."
                footer={
                    <>
                        <Button
                            variant="ghost"
                            onClick={() => setPlanOpen(false)}
                        >
                            Hủy
                        </Button>
                        <Button
                            onClick={() => createPlan.mutate()}
                            loading={createPlan.isPending}
                            disabled={
                                !planMeta.academicYearId || !quotas.length
                            }
                        >
                            Tạo chương trình
                        </Button>
                    </>
                }
            >
                <div className="curriculum-form">
                    <div className="form-grid-2">
                        <Field label="Năm học" htmlFor="planYear">
                            <Select
                                id="planYear"
                                value={planMeta.academicYearId}
                                onChange={(event) =>
                                    setPlanMeta((item) => ({
                                        ...item,
                                        academicYearId: event.target.value,
                                    }))
                                }
                            >
                                <option value="">Chọn năm học</option>
                                {yearItems.map((item) => (
                                    <option value={item.id} key={item.id}>
                                        {item.name}
                                    </option>
                                ))}
                            </Select>
                        </Field>
                        <Field label="Khối" htmlFor="planGrade">
                            <Select
                                id="planGrade"
                                value={planMeta.gradeLevel}
                                onChange={(event) => {
                                    const gradeLevel = Number(
                                        event.target.value,
                                    );
                                    setPlanMeta((item) => ({
                                        ...item,
                                        gradeLevel,
                                        name: `Chương trình khối ${gradeLevel}`,
                                    }));
                                }}
                            >
                                <option value="10">Khối 10</option>
                                <option value="11">Khối 11</option>
                                <option value="12">Khối 12</option>
                            </Select>
                        </Field>
                    </div>
                    <Field label="Tên chương trình" htmlFor="planName">
                        <Input
                            id="planName"
                            value={planMeta.name}
                            onChange={(event) =>
                                setPlanMeta((item) => ({
                                    ...item,
                                    name: event.target.value,
                                }))
                            }
                        />
                    </Field>
                    <div className="form-grid-2">
                        <Field label="Môn học" htmlFor="quotaSubject">
                            <Select
                                id="quotaSubject"
                                value={quotaDraft.subjectId}
                                onChange={(event) =>
                                    setQuotaDraft((item) => ({
                                        ...item,
                                        subjectId: event.target.value,
                                    }))
                                }
                            >
                                <option value="">Chọn môn</option>
                                {subjectItems.map((item) => (
                                    <option value={item.id} key={item.id}>
                                        {item.subjectCode} · {item.name}
                                    </option>
                                ))}
                            </Select>
                        </Field>
                        <Field label="Loại môn" htmlFor="quotaKind">
                            <Select
                                id="quotaKind"
                                value={quotaDraft.kind}
                                onChange={(event) =>
                                    setQuotaDraft((item) => ({
                                        ...item,
                                        kind: Number(event.target.value),
                                    }))
                                }
                            >
                                <option value="1">Bắt buộc</option>
                                <option value="2">Lựa chọn</option>
                                <option value="3">Chuyên đề</option>
                            </Select>
                        </Field>
                        <Field label="Tiết cả năm" htmlFor="quotaAnnual">
                            <Input
                                id="quotaAnnual"
                                type="number"
                                min="1"
                                value={quotaDraft.annualPeriods}
                                onChange={(event) =>
                                    setQuotaDraft((item) => ({
                                        ...item,
                                        annualPeriods: Number(
                                            event.target.value,
                                        ),
                                    }))
                                }
                            />
                        </Field>
                        <Field label="HK I / HK II" htmlFor="quotaS1">
                            <div className="inline-inputs">
                                <Input
                                    id="quotaS1"
                                    type="number"
                                    min="0"
                                    value={quotaDraft.semester1Periods}
                                    onChange={(event) =>
                                        setQuotaDraft((item) => ({
                                            ...item,
                                            semester1Periods: Number(
                                                event.target.value,
                                            ),
                                        }))
                                    }
                                />
                                <Input
                                    aria-label="Số tiết học kỳ II"
                                    type="number"
                                    min="0"
                                    value={quotaDraft.semester2Periods}
                                    onChange={(event) =>
                                        setQuotaDraft((item) => ({
                                            ...item,
                                            semester2Periods: Number(
                                                event.target.value,
                                            ),
                                        }))
                                    }
                                />
                            </div>
                        </Field>
                        <Field label="Buổi ưu tiên" htmlFor="quotaSession">
                            <Select
                                id="quotaSession"
                                value={quotaDraft.preferredSession ?? ""}
                                onChange={(event) =>
                                    setQuotaDraft((item) => ({
                                        ...item,
                                        preferredSession: event.target.value
                                            ? Number(event.target.value)
                                            : null,
                                    }))
                                }
                            >
                                <option value="">Không ưu tiên</option>
                                <option value="1">Buổi sáng</option>
                                <option value="2">Buổi chiều</option>
                            </Select>
                        </Field>
                    </div>
                    <div className="quota-options">
                        <label>
                            <input
                                type="checkbox"
                                checked={quotaDraft.canDoublePeriod}
                                onChange={(event) =>
                                    setQuotaDraft((item) => ({
                                        ...item,
                                        canDoublePeriod: event.target.checked,
                                    }))
                                }
                            />{" "}
                            Cho phép tiết đôi
                        </label>
                        <label>
                            <input
                                type="checkbox"
                                checked={quotaDraft.includesHomeroom}
                                onChange={(event) => {
                                    const includesHomeroom =
                                        event.target.checked;
                                    const homeroomSubject = subjectItems.find(
                                        (subject) =>
                                            subject.subjectCode === "HOMEROOM",
                                    );
                                    setQuotaDraft((item) =>
                                        includesHomeroom
                                            ? {
                                                  ...item,
                                                  subjectId:
                                                      homeroomSubject?.id ||
                                                      item.subjectId,
                                                  includesHomeroom: true,
                                                  annualPeriods: 35,
                                                  semester1Periods: 18,
                                                  semester2Periods: 17,
                                                  canDoublePeriod: false,
                                                  maxPeriodsPerDay: 1,
                                                  preferredSession: 1,
                                              }
                                            : {
                                                  ...item,
                                                  includesHomeroom: false,
                                              },
                                    );
                                }}
                            />{" "}
                            Sinh hoạt lớp Thứ Bảy
                        </label>
                        <Button variant="outline" onClick={addQuota}>
                            <Plus size={15} /> Thêm môn vào chương trình
                        </Button>
                    </div>
                    <div className="quota-list">
                        {quotas.map((quota) => {
                            const subject = subjectItems.find(
                                (item) => item.id === quota.subjectId,
                            );
                            return (
                                <div key={quota.subjectId}>
                                    <span>
                                        <b>{subject?.name}</b>
                                        <small>
                                            {quota.annualPeriods} tiết/năm ·{" "}
                                            HK I {quota.semester1Periods} · HK II {quota.semester2Periods}
                                        </small>
                                    </span>
                                    <button
                                        className="ui-icon-button danger-icon"
                                        onClick={() =>
                                            setQuotas((items) =>
                                                items.filter(
                                                    (item) =>
                                                        item.subjectId !==
                                                        quota.subjectId,
                                                ),
                                            )
                                        }
                                        aria-label={`Xóa ${subject?.name}`}
                                    >
                                        <X size={15} />
                                    </button>
                                </div>
                            );
                        })}
                    </div>
                </div>
            </Dialog>
        </div>
    );
}
