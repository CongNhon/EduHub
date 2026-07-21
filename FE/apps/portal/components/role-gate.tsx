"use client";

import type { UserRole } from "@eduhub/auth";
import { ShieldX } from "lucide-react";
import type { ReactNode } from "react";
import { useSession } from "./session-provider";

/** RoleGate ẩn screen không phù hợp role; backend vẫn là lớp authorization quyết định. */
export function RoleGate({ allow, children }: { allow: UserRole[]; children: ReactNode }) {
  const { user } = useSession();
  if (!user || !allow.includes(user.role)) return <div className="access-state"><ShieldX size={32} /><h1>Không có quyền truy cập</h1><p>Tài khoản hiện tại không được phép mở khu vực này.</p></div>;
  return children;
}
