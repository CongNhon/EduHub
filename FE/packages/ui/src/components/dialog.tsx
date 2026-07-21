"use client";

import type { ReactNode } from "react";
import * as DialogPrimitive from "@radix-ui/react-dialog";
import { X } from "lucide-react";

/** Modal giữ focus để nhập dữ liệu hoặc xác nhận thao tác quan trọng. */
export function Dialog({ open, onOpenChange, title, description, trigger, children, footer }: { open?: boolean; onOpenChange?: (open: boolean) => void; title: string; description?: string; trigger?: ReactNode; children: ReactNode; footer?: ReactNode }) {
  return (
    <DialogPrimitive.Root open={open} onOpenChange={onOpenChange}>
      {trigger ? <DialogPrimitive.Trigger asChild>{trigger}</DialogPrimitive.Trigger> : null}
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="ui-dialog__overlay" />
        <DialogPrimitive.Content className="ui-dialog__content">
          <div className="ui-dialog__header">
            <div>
              <DialogPrimitive.Title className="ui-dialog__title">{title}</DialogPrimitive.Title>
              {description ? <DialogPrimitive.Description className="ui-dialog__description">{description}</DialogPrimitive.Description> : null}
            </div>
            <DialogPrimitive.Close className="ui-icon-button" aria-label="Đóng hộp thoại"><X size={19} /></DialogPrimitive.Close>
          </div>
          <div className="ui-dialog__body">{children}</div>
          {footer ? <div className="ui-dialog__footer">{footer}</div> : null}
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  );
}
