import Link from "next/link";

/** Logo portal giữ nhận diện EduHub trên sidebar và login. */
export function PortalLogo({ compact = false }: { compact?: boolean }) { return <Link href="/" className="portal-logo" aria-label="EduHub Portal"><span>E</span>{compact ? null : <b>EduHub</b>}</Link>; }
