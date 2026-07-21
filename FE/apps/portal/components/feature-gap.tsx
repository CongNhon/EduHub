import { Construction } from "lucide-react";

/** FeatureGap thông báo rõ API backend còn thiếu, không giả dữ liệu production. */
export function FeatureGap({ title, description, gap }: { title: string; description: string; gap: string }) {
  return <div className="feature-gap"><span><Construction size={22} /></span><div><b>{title}</b><p>{description}</p>{process.env.NODE_ENV === "development" ? <code>{gap}</code> : null}</div></div>;
}
