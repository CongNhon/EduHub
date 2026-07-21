"use client";

import { Badge, Button, Card, EmptyState, Select } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { CalendarClock } from "lucide-react";
import { useMemo, useState } from "react";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type {
    ChildSummaryDto,
    Envelope,
    StudentSelfProfileDto,
    TeachingAssignmentSummaryDto,
    TimetableEntryDto,
    TimetableVersionDto,
    TimetableWeekDto,
} from "@/lib/domain";

type TimetableRole = "Student" | "Parent" | "Teacher";
interface TimetableContext {
    key: string;
    classRoomId: string;
    className: string;
    semesterId: string;
    semesterName: string;
}

/** RoleTimetable hiển thị thời khóa biểu đã công bố theo đúng lớp mà Student, Parent hoặc Teacher được phép xem. */
export function RoleTimetable({ role }: { role: TimetableRole }) {
    const { request } = useSession();
    const [selectedKey, setSelectedKey] = useState("");
    const [weekNumbers, setWeekNumbers] = useState<Record<string, number>>({});
    const student = useQuery({
        queryKey: ["student-profile", "timetable"],
        queryFn: () =>
            request<Envelope<StudentSelfProfileDto>>(
                "/api/v1/student-profile/me",
            ),
        enabled: role === "Student",
    });
    const children = useQuery({
        queryKey: ["children", "timetable"],
        queryFn: () =>
            request<Envelope<ChildSummaryDto[]>>("/api/v1/me/children"),
        enabled: role === "Parent",
    });
    const assignments = useQuery({
        queryKey: ["teaching-assignments", "timetable"],
        queryFn: () =>
            request<Envelope<TeachingAssignmentSummaryDto[]>>(
                "/api/v1/me/teaching-assignments",
            ),
        enabled: role === "Teacher",
    });
    const contexts = useMemo<TimetableContext[]>(() => {
        if (role === "Student") {
            const item = student.data?.data;
            return item?.currentClassId && item.currentSemesterId
                ? [
                      {
                          key: `${item.currentClassId}:${item.currentSemesterId}`,
                          classRoomId: item.currentClassId,
                          className: item.currentClassName || "Lớp hiện tại",
                          semesterId: item.currentSemesterId,
                          semesterName:
                              item.currentSemesterName || "Học kỳ hiện tại",
                      },
                  ]
                : [];
        }
        if (role === "Parent")
            return (children.data?.data || [])
                .filter((item) => item.currentClassId && item.currentSemesterId)
                .map((item) => ({
                    key: `${item.id}:${item.currentSemesterId}`,
                    classRoomId: item.currentClassId!,
                    className: `${item.fullName} · ${item.currentClassName || "Lớp"}`,
                    semesterId: item.currentSemesterId!,
                    semesterName: item.currentSemesterName || "Học kỳ",
                }));
        const seen = new Set<string>();
        return (assignments.data?.data || []).flatMap((item) => {
            const key = `${item.classRoomId}:${item.semesterId}`;
            if (seen.has(key)) return [];
            seen.add(key);
            return [
                {
                    key,
                    classRoomId: item.classRoomId,
                    className: item.className,
                    semesterId: item.semesterId,
                    semesterName: item.semesterName,
                },
            ];
        });
    }, [assignments.data, children.data, role, student.data]);
    const activeSelectedKey = contexts.some((item) => item.key === selectedKey)
        ? selectedKey
        : contexts[0]?.key || "";
    const selected =
        contexts.find((item) => item.key === activeSelectedKey) || contexts[0];
    const published = useQuery({
        queryKey: ["published-timetable", selected?.semesterId],
        queryFn: () =>
            request<Envelope<TimetableVersionDto>>(
                `/api/v1/timetables/published?semesterId=${selected!.semesterId}`,
            ),
        enabled: Boolean(selected),
    });
    const weeks = useQuery({
        queryKey: ["timetable-weeks", selected?.semesterId],
        queryFn: () =>
            request<Envelope<TimetableWeekDto[]>>(
                `/api/v1/timetables/weeks?semesterId=${selected!.semesterId}`,
            ),
        enabled: Boolean(selected),
    });
    const weekItems = weeks.data?.data || [];
    const defaultWeekNumber =
        weekItems.find((item) => item.isCurrent)?.weekNumber ||
        weekItems[0]?.weekNumber ||
        1;
    const storedWeekNumber = selected ? weekNumbers[selected.key] : undefined;
    const weekNumber = weekItems.some((item) => item.weekNumber === storedWeekNumber)
        ? storedWeekNumber!
        : defaultWeekNumber;
    const setWeekNumber = (value: number) => {
        if (!selected) return;
        setWeekNumbers((current) => ({ ...current, [selected.key]: value }));
    };
    const entries = useQuery({
        queryKey: [
            "timetable-entries",
            published.data?.data.id,
            selected?.classRoomId,
            weekNumber,
        ],
        queryFn: () =>
            request<Envelope<TimetableEntryDto[]>>(
                `/api/v1/timetables/${published.data!.data.id}/entries?classRoomId=${selected!.classRoomId}&weekNumber=${weekNumber}`,
            ),
        enabled: Boolean(published.data?.data.id && selected),
    });

    const loading =
        student.isLoading ||
        children.isLoading ||
        assignments.isLoading ||
        published.isLoading ||
        weeks.isLoading ||
        entries.isLoading;
    if (loading)
        return (
            <>
                <PageHeader eyebrow="LỊCH HỌC" title="Thời khóa biểu" />
                <LoadingPanel rows={8} />
            </>
        );
    const error =
        student.error ||
        children.error ||
        assignments.error ||
        weeks.error ||
        entries.error;
    if (error)
        return (
            <>
                <PageHeader eyebrow="LỊCH HỌC" title="Thời khóa biểu" />
                <ErrorPanel message={sessionErrorMessage(error)} />
            </>
        );
    if (!contexts.length)
        return (
            <>
                <PageHeader eyebrow="LỊCH HỌC" title="Thời khóa biểu" />
                <Card>
                    <EmptyState
                        icon={CalendarClock}
                        title="Chưa xác định lớp học"
                        description="Học vụ cần hoàn tất ghi danh hoặc phân công trước khi xem thời khóa biểu."
                    />
                </Card>
            </>
        );
    if (published.error)
        return (
            <>
                <PageHeader eyebrow="LỊCH HỌC" title="Thời khóa biểu" />
                <Card>
                    <EmptyState
                        icon={CalendarClock}
                        title="Chưa công bố thời khóa biểu"
                        description="Bản nháp chỉ hiển thị cho quản trị học vụ cho tới khi được công bố."
                    />
                </Card>
            </>
        );

    const selectedWeek = weekItems.find(
        (item) => item.weekNumber === weekNumber,
    );
    const currentWeek = weekItems.find((item) => item.isCurrent);
    const formatShortDate = (value: string) =>
        new Intl.DateTimeFormat("vi-VN", {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
        }).format(new Date(`${value}T00:00:00`));
    return (
        <div className="page-stack">
            <PageHeader
                eyebrow="LỊCH HỌC"
                title="Thời khóa biểu"
                description={`${selected?.className} · ${selected?.semesterName}`}
            />
            <div className="timetable-toolbar">
                <Select
                    value={activeSelectedKey}
                    onChange={(event) => setSelectedKey(event.target.value)}
                >
                    {contexts.map((item) => (
                        <option key={item.key} value={item.key}>
                            {item.className} · {item.semesterName}
                        </option>
                    ))}
                </Select>
                <Select
                    value={weekNumber}
                    onChange={(event) => setWeekNumber(Number(event.target.value))}
                >
                    {weekItems.map((item) => (
                        <option key={item.weekNumber} value={item.weekNumber}>
                            Tuần {String(item.weekNumber).padStart(2, "0")} · {formatShortDate(item.startDate)} - {formatShortDate(item.endDate)}{item.isCurrent ? " · Hiện tại" : ""}
                        </option>
                    ))}
                </Select>
                {currentWeek ? (
                    <Button
                        variant="secondary"
                        onClick={() => setWeekNumber(currentWeek.weekNumber)}
                    >
                        Về tuần hiện tại
                    </Button>
                ) : null}
                {selectedWeek?.isCurrent ? (
                    <Badge tone="success">Tuần hiện tại</Badge>
                ) : null}
                <Badge tone="success">Đã công bố</Badge>
            </div>
            <TimetableGrid entries={entries.data?.data || []} />
        </div>
    );
}

/** TimetableGrid dựng lưới sáng/chiều theo giờ học thật và giữ kích thước ô ổn định trên desktop/mobile. */
export function TimetableGrid({
    entries,
    onEntryClick,
}: {
    entries: TimetableEntryDto[];
    onEntryClick?: (entry: TimetableEntryDto) => void;
}) {
    const days = [
        "Thứ Hai",
        "Thứ Ba",
        "Thứ Tư",
        "Thứ Năm",
        "Thứ Sáu",
        "Thứ Bảy",
    ];
    const bySlot = new Map(
        entries.map((entry) => [
            `${entry.session}:${entry.dayOfWeek}:${entry.periodNumber}`,
            entry,
        ]),
    );
    const activeAfternoonDays = new Set(
        entries
            .filter((entry) => entry.session === "Afternoon")
            .map((entry) => entry.dayOfWeek),
    );
    const periodTime = (
        session: "Morning" | "Afternoon",
        periodNumber: number,
    ) => {
        const startMinutes =
            (session === "Morning" ? 7 * 60 + 15 : 13 * 60 + 15) +
            (periodNumber - 1) * 50;
        const format = (minutes: number) =>
            `${String(Math.floor(minutes / 60)).padStart(2, "0")}:${String(minutes % 60).padStart(2, "0")}`;
        return `${format(startMinutes)} - ${format(startMinutes + 45)}`;
    };
    const rows = [
        ...Array.from({ length: 5 }, (_, index) => ({
            session: "Morning" as const,
            periodNumber: index + 1,
        })),
        ...(entries.some((entry) => entry.session === "Afternoon")
            ? Array.from({ length: 5 }, (_, index) => ({
                  session: "Afternoon" as const,
                  periodNumber: index + 1,
              }))
            : []),
    ];
    return (
        <div className="timetable-scroll">
            <div className="timetable-grid">
                <div className="timetable-corner">Tiết</div>
                {days.map((day) => (
                    <div className="timetable-day" key={day}>
                        {day}
                    </div>
                ))}
                {rows.flatMap(
                    ({ session, periodNumber }) => [
                        <div
                            className="timetable-period"
                            key={`period-${session}-${periodNumber}`}
                        >
                            <b>Tiết {periodNumber}</b>
                            <small>
                                {session === "Morning" ? "Sáng" : "Chiều"}
                            </small>
                            <small>{periodTime(session, periodNumber)}</small>
                        </div>,
                        ...days.map((_, dayIndex) => {
                            const entry = bySlot.get(
                                `${session}:${dayIndex + 1}:${periodNumber}`,
                            );
                            const unavailable =
                                (session === "Morning" &&
                                    dayIndex === 2 &&
                                    periodNumber === 5) ||
                                (session === "Afternoon" &&
                                    !activeAfternoonDays.has(dayIndex + 1));
                            return (
                                <div
                                    className={`timetable-cell ${unavailable ? "unavailable" : ""}`}
                                    key={`${session}-${dayIndex}-${periodNumber}`}
                                >
                                    {entry ? (
                                        <button
                                            className={`timetable-lesson ${entry.note ? "homeroom" : ""}`}
                                            onClick={() =>
                                                onEntryClick?.(entry)
                                            }
                                            disabled={!onEntryClick}
                                        >
                                            <b>{entry.subjectName}</b>
                                            <span>
                                                {entry.teacherName ||
                                                    "Chưa phân công"}
                                            </span>
                                            <small>
                                                {entry.startTime} - {entry.endTime}
                                            </small>
                                            {entry.note ? (
                                                <small>{entry.note}</small>
                                            ) : null}
                                        </button>
                                    ) : null}
                                </div>
                            );
                        }),
                    ],
                )}
            </div>
        </div>
    );
}
