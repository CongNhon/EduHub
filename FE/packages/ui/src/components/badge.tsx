import type { HTMLAttributes } from "react";
import { cn } from "../lib/cn";

export type BadgeTone = "neutral" | "info" | "success" | "warning" | "danger";

/** Badge thể hiện trạng thái bằng text và màu semantic. */
export function Badge({ className, tone = "neutral", ...props }: HTMLAttributes<HTMLSpanElement> & { tone?: BadgeTone }) {
  return <span className={cn("ui-badge", `ui-badge--${tone}`, className)} {...props} />;
}
