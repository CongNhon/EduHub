import Link from "next/link";

/** Logo chữ EduHub dùng chung trong public navigation. */
export function Logo() {
  return <Link href="/" className="brand" aria-label="EduHub - Trang chủ"><span className="brand__mark">E</span><span>EduHub</span></Link>;
}
