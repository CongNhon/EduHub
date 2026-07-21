import type { HTMLAttributes, ReactNode } from "react";
import { cn } from "../lib/cn";

/** Surface có border dùng cho một khối dữ liệu hoặc công cụ độc lập. */
export function Card({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <section className={cn("ui-card", className)} {...props} />;
}

/** Header thống nhất title, mô tả và action của Card. */
export function CardHeader({ title, description, action }: { title: string; description?: string; action?: ReactNode }) {
  return (
    <header className="ui-card__header">
      <div>
        <h2 className="ui-card__title">{title}</h2>
        {description ? <p className="ui-card__description">{description}</p> : null}
      </div>
      {action ? <div className="ui-card__action">{action}</div> : null}
    </header>
  );
}
