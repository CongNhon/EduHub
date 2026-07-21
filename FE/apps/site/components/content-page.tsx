import type { LucideIcon } from "lucide-react";
import { ArrowRight, CheckCircle2 } from "lucide-react";

/** ContentPage tạo bố cục nhất quán cho các trang giới thiệu theo đối tượng. */
export function ContentPage({ eyebrow, title, description, icon: Icon, outcomes, steps }: { eyebrow: string; title: string; description: string; icon: LucideIcon; outcomes: { title: string; copy: string }[]; steps: string[] }) {
  const portalUrl = process.env.NEXT_PUBLIC_PORTAL_URL || "http://localhost:3001";
  return (
    <>
      <section className="content-hero"><div><span>{eyebrow}</span><h1>{title}</h1><p>{description}</p><a className="ui-button ui-button--primary" href={`${portalUrl}/login`}>Đăng nhập EduHub <ArrowRight size={17} /></a></div><div className="content-hero__symbol"><Icon size={72} strokeWidth={1.4} /><i /><i /><i /></div></section>
      <section className="marketing-section"><div className="section-heading"><span>NHỮNG VIỆC BẠN CÓ THỂ LÀM</span><h2>Thông tin cần thiết, đúng lúc cần dùng</h2></div><div className="outcome-grid">{outcomes.map((item) => <article key={item.title}><CheckCircle2 size={20} /><h3>{item.title}</h3><p>{item.copy}</p></article>)}</div></section>
      <section className="journey-section"><div><span>CÁCH SỬ DỤNG</span><h2>Bắt đầu với ba bước</h2></div><ol>{steps.map((step, index) => <li key={step}><b>{String(index + 1).padStart(2, "0")}</b><span>{step}</span></li>)}</ol></section>
    </>
  );
}
