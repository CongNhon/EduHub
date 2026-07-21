"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { homeForRole } from "@eduhub/auth";
import { Button, Field, Input, ThemeToggle } from "@eduhub/ui";
import { loginSchema } from "@eduhub/validation";
import { BookOpenCheck, Check, Eye, EyeOff, LockKeyhole, ShieldCheck } from "lucide-react";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { PortalLogo } from "@/components/portal-logo";
import { sessionErrorMessage, useSession } from "@/components/session-provider";

type LoginValues = z.infer<typeof loginSchema>;

/** LoginPage xác thực qua BFF và điều hướng theo role backend trả về. */
export default function LoginPage() {
  const { status, user, login } = useSession();
  const router = useRouter();
  const [showPassword, setShowPassword] = useState(false);
  const [formError, setFormError] = useState("");
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<LoginValues>({ resolver: zodResolver(loginSchema), defaultValues: { email: "", password: "" } });
  useEffect(() => { if (status === "authenticated" && user) router.replace(homeForRole(user.role)); }, [router, status, user]);
  const submit = handleSubmit(async (values) => { setFormError(""); try { const current = await login(values); const returnTo = new URLSearchParams(window.location.search).get("returnTo"); const home = homeForRole(current.role); const commonRoute = returnTo === "/notifications" || returnTo?.startsWith("/reports?"); const safeReturn = returnTo?.startsWith(home) || commonRoute ? returnTo! : home; router.replace(safeReturn); } catch (error) { setFormError(sessionErrorMessage(error)); } });
  return <main className="login-page"><div className="login-brand-panel"><div className="login-brand-panel__top"><PortalLogo /><ThemeToggle /></div><div className="login-brand-panel__content"><span>EDUHUB PORTAL</span><h1>Một không gian làm việc.<br />Đúng dữ liệu cho từng vai trò.</h1><p>Điểm số, học vụ, báo cáo và thông báo được tổ chức theo quy trình an toàn của nhà trường.</p><div className="login-benefits"><span><Check /> Không lưu token trong localStorage</span><span><Check /> Backend kiểm tra role và ownership</span><span><Check /> Chỉ hiển thị điểm đã công bố cho gia đình</span></div></div><div className="login-product-preview"><div><small>HỌC KỲ II</small><b>Tiến độ học tập</b><span className="login-score">8.42</span><em>12 môn đã công bố</em></div><div><BookOpenCheck /><b>Kết quả mới</b><p>Điểm Toán đã được công bố</p></div></div></div><div className="login-form-panel"><form className="login-card" onSubmit={submit} noValidate><div className="login-card__icon"><LockKeyhole size={23} /></div><span>TRUY CẬP AN TOÀN</span><h2>Chào mừng trở lại</h2><p>Đăng nhập bằng tài khoản được nhà trường cấp.</p>{formError ? <div className="form-alert" role="alert"><ShieldCheck size={18} /><span>{formError}</span></div> : null}<Field label="Email" htmlFor="email" error={errors.email?.message} required><Input id="email" type="email" autoComplete="username" placeholder="ten@truong.edu.vn" aria-invalid={Boolean(errors.email)} {...register("email")} /></Field><Field label="Mật khẩu" htmlFor="password" error={errors.password?.message} required><div className="password-field"><Input id="password" type={showPassword ? "text" : "password"} autoComplete="current-password" aria-invalid={Boolean(errors.password)} {...register("password")} /><button type="button" onClick={() => setShowPassword((value) => !value)} aria-label={showPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu"}>{showPassword ? <EyeOff size={18} /> : <Eye size={18} />}</button></div></Field><Button type="submit" loading={isSubmitting}>{isSubmitting ? "Đang đăng nhập..." : "Đăng nhập"}</Button><small className="login-support">Quên mật khẩu? Liên hệ quản trị viên nhà trường.</small></form><a className="back-to-site" href={process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000"}>← Quay lại website EduHub</a></div></main>;
}
