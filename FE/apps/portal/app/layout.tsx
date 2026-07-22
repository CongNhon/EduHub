import type { Metadata } from "next";
import { Be_Vietnam_Pro } from "next/font/google";
import "@devexpress/analytics-core/dist/css/dx-analytics.common.css";
import "devexpress-reporting/dist/css/dx-webdocumentviewer.css";
import "@eduhub/ui/tokens.css";
import "@eduhub/ui/components.css";
import "./globals.css";
import { Providers } from "./providers";
import { DevExpressLicense } from "@/components/devexpress-license";

const font = Be_Vietnam_Pro({ subsets: ["latin", "vietnamese"], weight: ["400", "500", "600", "700"] });

export const metadata: Metadata = { title: { default: "EduHub Portal", template: "%s | EduHub" }, description: "Không gian học vụ bảo mật dành cho gia đình, giáo viên và nhà trường.", robots: { index: false, follow: false } };

/** Root layout cung cấp font và provider bảo mật cho portal. */
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return <html lang="vi" suppressHydrationWarning><head><link id="devextreme-theme" rel="stylesheet" href="/themes/devextreme-light.css" /><link id="devexpress-analytics-theme" rel="stylesheet" href="/themes/analytics-light.css" /></head><body suppressHydrationWarning className={font.className}><DevExpressLicense /><Providers><a className="skip-link" href="#portal-content">Chuyển tới nội dung chính</a>{children}</Providers></body></html>;
}
