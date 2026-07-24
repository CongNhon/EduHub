"use client";

import { homeForRole, roleLabel, type UserRole } from "@eduhub/auth";
import { ThemeToggle } from "@eduhub/ui";
import { Bell, BookOpenCheck, Boxes, CalendarClock, CalendarRange, ChevronLeft, ChevronRight, CircleUserRound, FileChartColumn, FileSpreadsheet, FileText, HeartHandshake, LayoutDashboard, LogOut, Menu, School, Settings2, UserRoundCheck, UserRoundCog, Users, X, TrendingUp, BarChart3, ShieldAlert } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useMemo, useState, type ReactNode } from "react";
import { PortalLogo } from "./portal-logo";
import { useRealtimeState } from "./realtime-provider";
import { useSession } from "./session-provider";

interface NavItem { href: string; label: string; icon: typeof LayoutDashboard; }
const common = [{ href: "/notifications", label: "Thông báo", icon: Bell }];
const navigation: Record<UserRole, NavItem[]> = {
  Parent: [{ href: "/parent", label: "Con của tôi", icon: LayoutDashboard }, { href: "/parent/timetable", label: "Thời khóa biểu", icon: CalendarClock }, { href: "/reports", label: "Báo cáo học tập", icon: FileText }, ...common],
  Student: [{ href: "/student", label: "Tổng quan", icon: LayoutDashboard }, { href: "/student/timetable", label: "Thời khóa biểu", icon: CalendarClock }, { href: "/student/profile", label: "Hồ sơ của tôi", icon: CircleUserRound }, ...common],
  Teacher: [{ href: "/teacher", label: "Tổng quan", icon: LayoutDashboard }, { href: "/teacher/classes", label: "Lớp & Sổ điểm", icon: BookOpenCheck }, { href: "/teacher/timetable", label: "Thời khóa biểu", icon: CalendarClock }, ...common],
  AcademicAdmin: [{ href: "/academic", label: "Tổng quan", icon: LayoutDashboard }, { href: "/academic/students", label: "Học sinh & Phụ huynh", icon: Users }, { href: "/academic/people", label: "Giáo viên & Phụ huynh", icon: UserRoundCog }, { href: "/academic/academics", label: "Năm học & Môn học", icon: CalendarRange }, { href: "/academic/classes", label: "Lớp & Ghi danh", icon: Boxes }, { href: "/academic/scheduling", label: "Chương trình & TKB", icon: CalendarClock }, { href: "/academic/imports", label: "Import Excel", icon: FileSpreadsheet }, { href: "/academic/profile-requests", label: "Duyệt hồ sơ", icon: UserRoundCheck }, { href: "/academic/grade-configurations", label: "Cấu hình điểm", icon: Settings2 }, { href: "/academic/gradebooks", label: "Duyệt & Công bố điểm", icon: BookOpenCheck }, { href: "/reports", label: "Duyệt báo cáo", icon: FileText }, ...common],
  SystemAdmin: [
    { href: "/admin", label: "Tổng quan", icon: LayoutDashboard },
    { href: "/admin/analytics/academic", label: "Phân tích học tập", icon: BarChart3 },
    { href: "/admin/analytics/trends", label: "Xu hướng học tập", icon: TrendingUp },
    { href: "/admin/analytics/risk", label: "Cảnh báo sớm", icon: ShieldAlert },
    { href: "/admin/reports", label: "Báo cáo quản trị", icon: FileChartColumn },
    { href: "/admin/people", label: "Tài khoản & Phân quyền", icon: UserRoundCog },
    { href: "/admin/school", label: "Thông tin trường", icon: School },
    { href: "/admin/system-health", label: "Sức khỏe hệ thống", icon: HeartHandshake },
    ...common
  ],
};

/** PortalShell dựng navigation theo role, topbar và responsive drawer cho portal. */
export function PortalShell({ children }: { children: ReactNode }) {
  const { status, user, logout } = useSession();
  const { state: realtime } = useRealtimeState();
  const pathname = usePathname();
  const router = useRouter();
  const [collapsed, setCollapsed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  useEffect(() => { if (status === "anonymous") router.replace(`/login?returnTo=${encodeURIComponent(pathname)}`); }, [pathname, router, status]);
  const items = useMemo(() => user ? navigation[user.role] : [], [user]);
  if (status === "bootstrapping" || !user) return <div className="portal-boot"><div className="portal-boot__logo">E</div><div className="portal-boot__line" /><span>Đang kiểm tra phiên đăng nhập...</span></div>;
  return <div className={`portal-shell ${collapsed ? "portal-shell--collapsed" : ""}`}>
    <aside className={`portal-sidebar ${mobileOpen ? "portal-sidebar--open" : ""}`}>
      <div className="sidebar-brand"><PortalLogo compact={collapsed} /><button className="sidebar-close" onClick={() => setMobileOpen(false)} aria-label="Đóng menu"><X size={20} /></button></div>
      <div className="sidebar-context"><span className="sidebar-context__avatar">{roleLabel(user.role).charAt(0)}</span>{collapsed ? null : <div><small>KHÔNG GIAN LÀM VIỆC</small><b>{roleLabel(user.role)}</b></div>}</div>
      <nav className="portal-nav" aria-label="Điều hướng portal">{items.map((item) => { const active = pathname === item.href || (item.href !== homeForRole(user.role) && pathname.startsWith(`${item.href}/`)); return <Link key={item.href} href={item.href} className={active ? "active" : ""} aria-current={active ? "page" : undefined} title={collapsed ? item.label : undefined} onClick={() => setMobileOpen(false)}><item.icon size={19} /><span>{item.label}</span></Link>; })}</nav>
      <div className="sidebar-footer"><button onClick={() => setCollapsed((value) => !value)} aria-label={collapsed ? "Mở rộng sidebar" : "Thu gọn sidebar"}>{collapsed ? <ChevronRight size={18} /> : <><ChevronLeft size={18} /><span>Thu gọn</span></>}</button></div>
    </aside>
    {mobileOpen ? <button className="sidebar-overlay" onClick={() => setMobileOpen(false)} aria-label="Đóng menu" /> : null}
    <div className="portal-workspace">
      <header className="portal-topbar"><div className="topbar-left"><button className="ui-icon-button topbar-menu" onClick={() => setMobileOpen(true)} aria-label="Mở menu"><Menu size={19} /></button><div><small>EDUHUB PORTAL</small><b>{roleLabel(user.role)}</b></div></div><div className="topbar-actions">{realtime !== "connected" ? <span className={`connection-state connection-state--${realtime}`}>{realtime === "connecting" ? "Đang kết nối" : realtime === "reconnecting" ? "Đang kết nối lại" : "Realtime gián đoạn"}</span> : null}<ThemeToggle /><Link href="/notifications" className="ui-icon-button" aria-label="Mở thông báo"><Bell size={18} /></Link><details className="profile-menu"><summary><CircleUserRound size={20} /><span>{user.fullName}</span></summary><div><span><b>{user.fullName}</b><small>{user.email}</small></span><button onClick={async () => { await logout(); router.replace("/login"); }}><LogOut size={17} /> Đăng xuất</button></div></details></div></header>
      <main id="portal-content" className="portal-content">{children}</main>
    </div>
  </div>;
}
