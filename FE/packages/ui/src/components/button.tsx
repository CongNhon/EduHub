import * as React from "react";
import { LoaderCircle } from "lucide-react";
import { cn } from "../lib/cn";

export type ButtonVariant = "primary" | "secondary" | "outline" | "ghost" | "danger";

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  loading?: boolean;
}

/** Button chuẩn hóa trạng thái thao tác, loading và variant trong EduHub. */
export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(function Button(
  { className, variant = "primary", loading = false, disabled, children, ...props },
  ref,
) {
  return (
    <button
      ref={ref}
      className={cn("ui-button", `ui-button--${variant}`, className)}
      disabled={disabled || loading}
      aria-busy={loading}
      {...props}
    >
      {loading ? <LoaderCircle size={17} className="ui-spin" aria-hidden="true" /> : null}
      <span>{children}</span>
    </button>
  );
});
