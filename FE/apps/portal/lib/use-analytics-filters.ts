"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useCallback, useMemo } from "react";

export interface AnalyticsFilters {
  semesterId?: string;
  previousSemesterId?: string;
  gradeLevels: number[];
  classIds: string[];
  subjectIds: string[];
  teacherIds: string[];
  groupBy: string;
  maxSemesters: number;
  riskLevel?: string;
  page: number;
}

/** 
 * useAnalyticsFilters quản lý trạng thái bộ lọc analytics đồng bộ với URL search params.
 * Ghi chú: Hỗ trợ các tham số mảng (gradeLevels, classIds...) và phân trang.
 */
export function useAnalyticsFilters(): [AnalyticsFilters, (updates: Partial<AnalyticsFilters>) => void] {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  const filters = useMemo<AnalyticsFilters>(() => {
    const getArray = (key: string) => searchParams.getAll(key).filter(Boolean);
    const getIntArray = (key: string) => getArray(key).map((v) => parseInt(v, 10)).filter((v) => !isNaN(v));

    return {
      semesterId: searchParams.get("semesterId") || undefined,
      previousSemesterId: searchParams.get("previousSemesterId") || undefined,
      gradeLevels: getIntArray("gradeLevels"),
      classIds: getArray("classIds"),
      subjectIds: getArray("subjectIds"),
      teacherIds: getArray("teacherIds"),
      groupBy: searchParams.get("groupBy") || "class",
      maxSemesters: parseInt(searchParams.get("maxSemesters") || "4", 10),
      riskLevel: searchParams.get("riskLevel") || undefined,
      page: parseInt(searchParams.get("page") || "1", 10),
    };
  }, [searchParams]);

  const setFilters = useCallback((updates: Partial<AnalyticsFilters>) => {
    const params = new URLSearchParams(searchParams.toString());

    Object.entries(updates).forEach(([key, value]) => {
      params.delete(key);
      if (value === undefined || value === null || value === "") return;

      if (Array.isArray(value)) {
        value.forEach((v) => params.append(key, String(v)));
      } else {
        params.set(key, String(value));
      }
    });

    // Reset page if filters other than page change
    if (updates.page === undefined && (updates.semesterId !== undefined || updates.gradeLevels !== undefined || updates.classIds !== undefined || updates.subjectIds !== undefined || updates.teacherIds !== undefined || updates.riskLevel !== undefined)) {
      params.set("page", "1");
    }

    router.push(`${pathname}?${params.toString()}`);
  }, [pathname, router, searchParams]);

  return [filters, setFilters];
}
