import { redirect } from "next/navigation";

/** Public login hand-off chuyển người dùng sang authenticated portal. */
export default function LoginHandoffPage() { redirect(`${process.env.NEXT_PUBLIC_PORTAL_URL || "http://localhost:3001"}/login`); }
