import { redirect } from "next/navigation";

/** Route gốc chuyển người dùng tới màn hình đăng nhập. */
export default function PortalIndexPage() { redirect("/login"); }
