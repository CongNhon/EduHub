"use client";

import { Card, CardHeader } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { Mail, MapPin, Phone, School } from "lucide-react";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, SchoolProfileDto } from "@/lib/domain";

/** SchoolProfile hiển thị ngữ cảnh trường cố định đang vận hành toàn bộ dữ liệu EduHub. */
export function SchoolProfile() {
  const { request } = useSession();
  const school = useQuery({ queryKey: ["school-profile"], queryFn: () => request<Envelope<SchoolProfileDto>>("/api/v1/school-profile") });
  if (school.isLoading) return <><PageHeader eyebrow="SINGLE-SCHOOL" title="Thông tin trường" /><LoadingPanel rows={5} /></>;
  if (school.error) return <><PageHeader eyebrow="SINGLE-SCHOOL" title="Thông tin trường" /><ErrorPanel message={sessionErrorMessage(school.error)} onRetry={() => school.refetch()} /></>;
  const profile = school.data?.data;
  return <div className="page-stack"><PageHeader eyebrow="SINGLE-SCHOOL" title="Thông tin trường" description="Một school context dùng chung cho học sinh, lớp, điểm, thông báo và báo cáo." /><Card><CardHeader title={profile?.name || "EduHub"} description={profile?.code} /><div className="school-profile"><span className="school-profile__mark"><School size={34} /></span><dl><div><dt><MapPin size={15} /> Địa chỉ</dt><dd>{profile?.address || "Chưa cấu hình"}</dd></div><div><dt><Mail size={15} /> Email</dt><dd>{profile?.email || "Chưa cấu hình"}</dd></div><div><dt><Phone size={15} /> Điện thoại</dt><dd>{profile?.phoneNumber || "Chưa cấu hình"}</dd></div></dl></div></Card></div>;
}
