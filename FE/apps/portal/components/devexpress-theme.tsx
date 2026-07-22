"use client";

import { useTheme } from "next-themes";
import { useEffect } from "react";

/** DevExpressTheme đồng bộ stylesheet DevExtreme và Analytics với theme sáng/tối của Portal. */
export function DevExpressTheme() {
  const { resolvedTheme } = useTheme();

  useEffect(() => {
    const theme = resolvedTheme === "dark" ? "dark" : "light";
    updateThemeLink("devextreme-theme", `/themes/devextreme-${theme}.css`);
    updateThemeLink("devexpress-analytics-theme", `/themes/analytics-${theme}.css`);
  }, [resolvedTheme]);

  return null;
}

/** updateThemeLink chỉ thay href khi theme thực tế đổi để tránh tải lại stylesheet không cần thiết. */
function updateThemeLink(id: string, href: string) {
  const link = document.getElementById(id) as HTMLLinkElement | null;
  if (link && link.getAttribute("href") !== href) link.href = href;
}
