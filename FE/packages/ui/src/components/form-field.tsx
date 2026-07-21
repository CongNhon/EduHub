import type { InputHTMLAttributes, ReactNode, SelectHTMLAttributes, TextareaHTMLAttributes } from "react";
import { cn } from "../lib/cn";

/** Field liên kết label, help text và lỗi validation của một input. */
export function Field({ label, htmlFor, error, hint, required, children }: { label: string; htmlFor: string; error?: string; hint?: string; required?: boolean; children: ReactNode }) {
  const descriptionId = `${htmlFor}-description`;
  return (
    <div className="ui-field">
      <label htmlFor={htmlFor} className="ui-field__label">
        {label}{required ? <span aria-hidden="true"> *</span> : null}
      </label>
      {children}
      {error || hint ? <p id={descriptionId} className={cn("ui-field__message", error && "ui-field__message--error")}>{error || hint}</p> : null}
    </div>
  );
}

/** Input văn bản chuẩn của EduHub. */
export function Input({ className, ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return <input className={cn("ui-input", className)} {...props} />;
}

/** Select native có khả năng truy cập tốt cho danh sách lựa chọn ngắn. */
export function Select({ className, ...props }: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select className={cn("ui-input", "ui-select", className)} {...props} />;
}

/** Textarea chuẩn cho lý do và nội dung dài. */
export function Textarea({ className, ...props }: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea className={cn("ui-input", "ui-textarea", className)} {...props} />;
}
