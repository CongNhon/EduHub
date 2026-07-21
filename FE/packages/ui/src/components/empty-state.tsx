import type { LucideIcon } from "lucide-react";
import type { ReactNode } from "react";

/** EmptyState giải thích trạng thái trống và đưa ra hành động phù hợp quyền. */
export function EmptyState({ icon: Icon, title, description, action }: { icon: LucideIcon; title: string; description: string; action?: ReactNode }) {
  return (
    <div className="ui-empty-state">
      <div className="ui-empty-state__icon"><Icon size={24} /></div>
      <h3>{title}</h3>
      <p>{description}</p>
      {action ? <div>{action}</div> : null}
    </div>
  );
}
