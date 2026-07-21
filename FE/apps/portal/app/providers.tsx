"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "next-themes";
import { useState, type ReactNode } from "react";
import { Toaster } from "sonner";
import { RealtimeProvider } from "@/components/realtime-provider";
import { SessionProvider } from "@/components/session-provider";

/** Providers khởi tạo session, server state, realtime, theme và toast của portal. */
export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(() => new QueryClient({ defaultOptions: { queries: { staleTime: 30_000, retry: (count, error) => { const status = (error as { status?: number })?.status; return count < 2 && (!status || status >= 500); }, refetchOnWindowFocus: true }, mutations: { retry: false } } }));
  return <ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange><QueryClientProvider client={queryClient}><SessionProvider><RealtimeProvider>{children}</RealtimeProvider></SessionProvider></QueryClientProvider><Toaster richColors position="top-right" closeButton /></ThemeProvider>;
}
