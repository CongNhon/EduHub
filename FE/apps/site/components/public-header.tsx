"use client";

import { ThemeToggle } from "@eduhub/ui";
import { Menu, X } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { Logo } from "./logo";

const links = [
  { href: "/features", label: "Tính năng" },
  { href: "/for-parents", label: "Phụ huynh" },
  { href: "/for-teachers", label: "Giáo viên" },
  { href: "/for-schools", label: "Nhà trường" },
  { href: "/support", label: "Trợ giúp" },
];

/** PublicHeader điều hướng website và mở menu mobile có thể dùng bằng bàn phím. */
export function PublicHeader() {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  const portalUrl = process.env.NEXT_PUBLIC_PORTAL_URL || "http://localhost:3001";
  return (
    <header className="public-header">
      <div className="public-header__inner">
        <Logo />
        <nav className="public-nav" aria-label="Điều hướng chính">
          {links.map((link) => <Link key={link.href} href={link.href} aria-current={pathname === link.href ? "page" : undefined}>{link.label}</Link>)}
        </nav>
        <div className="public-header__actions">
          <ThemeToggle />
          <Link href="/results" className="header-result-link">Xem kết quả</Link>
          <a className="ui-button ui-button--primary" href={`${portalUrl}/login`}>Đăng nhập</a>
          <button className="ui-icon-button mobile-menu-button" onClick={() => setOpen((value) => !value)} aria-expanded={open} aria-controls="mobile-menu" aria-label={open ? "Đóng menu" : "Mở menu"}>{open ? <X size={20} /> : <Menu size={20} />}</button>
        </div>
      </div>
      {open ? <nav id="mobile-menu" className="mobile-menu" aria-label="Điều hướng mobile">{links.map((link) => <Link key={link.href} href={link.href} onClick={() => setOpen(false)}>{link.label}</Link>)}<Link href="/results" onClick={() => setOpen(false)}>Xem kết quả</Link><a className="mobile-menu__login" href={`${portalUrl}/login`}>Đăng nhập portal</a></nav> : null}
    </header>
  );
}
