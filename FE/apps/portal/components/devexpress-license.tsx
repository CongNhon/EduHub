"use client";

import config from "devextreme/core/config";
import { licenseKey } from "@/app/devextreme-license";

/** Đăng ký public client key trước khi portal khởi tạo bất kỳ DevExtreme component nào. */
export function DevExpressLicense() {
  if (typeof window !== "undefined") {
    config({ licenseKey });
  }

  return null;
}
