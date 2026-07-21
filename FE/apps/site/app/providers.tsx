"use client";

import { ThemeProvider } from "next-themes";
import type { ReactNode } from "react";

/** Providers khởi tạo theme động cho website công khai. */
export function Providers({ children }: { children: ReactNode }) {
  return <ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>{children}</ThemeProvider>;
}
