"use client";

import { Badge, Card, CardHeader } from "@eduhub/ui";
import { useQuery } from "@tanstack/react-query";
import { BadgeCheck, Mail, MapPin, Phone, School } from "lucide-react";
import { ErrorPanel, LoadingPanel } from "@/components/api-panel";
import { PageHeader } from "@/components/page-header";
import { sessionErrorMessage, useSession } from "@/components/session-provider";
import type { Envelope, SchoolProfileDto } from "@/lib/domain";

/** SchoolProfile hiển thị ngữ cảnh trường cố định đang vận hành toàn bộ dữ liệu EduHub. */
export function SchoolProfile() {
  const { request } = useSession();
  const school = useQuery({ queryKey: ["school-profile"], queryFn: () => request<Envelope<SchoolProfileDto>>("/api/v1/school-profile") });
  if (school.isLoading) return <><PageHeader eyebrow="HỒ SƠ NHÀ TRƯỜNG" title="Thông tin trường" /><LoadingPanel rows={5} /></>;
  if (school.error) return <><PageHeader eyebrow="HỒ SƠ NHÀ TRƯỜNG" title="Thông tin trường" /><ErrorPanel message={sessionErrorMessage(school.error)} onRetry={() => school.refetch()} /></>;
  const profile = school.data?.data;
  const completedFields = [profile?.code, profile?.name, profile?.address, profile?.email, profile?.phoneNumber, profile?.logoUrl].filter(Boolean).length;
  const completeness = Math.round(completedFields / 6 * 100);
  return <div className="page-stack"><PageHeader eyebrow="HỒ SƠ NHÀ TRƯỜNG" title="Thông tin trường" description="Thông tin định danh dùng thống nhất trên email, báo cáo và các màn hình học vụ." /><Card><CardHeader title={profile?.name || "Chưa cấu hình tên trường"} description={profile?.code ? `Mã trường ${profile.code}` : "Chưa cấu hình mã trường"} action={<Badge tone={completeness === 100 ? "success" : "warning"}>{completeness}% hoàn thiện</Badge>} /><div className="school-profile school-profile--detailed"><div className="school-profile__identity">{profile?.logoUrl ? <img src={profile.logoUrl} alt={`Logo ${profile.name}`} /> : <span className="school-profile__mark"><School size={34} /></span>}<div><BadgeCheck size={19} /><span>Thông tin nhận diện chính thức</span><small>Được dùng xuyên suốt hệ thống EduHub</small></div></div><dl><div><dt><MapPin size={15} /> Địa chỉ</dt><dd>{profile?.address || "Chưa cấu hình"}</dd></div><div><dt><Mail size={15} /> Email liên hệ</dt><dd>{profile?.email ? <a href={`mailto:${profile.email}`}>{profile.email}</a> : "Chưa cấu hình"}</dd></div><div><dt><Phone size={15} /> Điện thoại</dt><dd>{profile?.phoneNumber ? <a href={`tel:${profile.phoneNumber}`}>{profile.phoneNumber}</a> : "Chưa cấu hình"}</dd></div></dl></div><div className="school-profile__usage"><strong>Phạm vi sử dụng</strong><span>Hồ sơ học sinh</span><span>Email thông báo</span><span>Báo cáo DevExpress</span><span>Chứng từ học vụ</span></div></Card></div>;
}
