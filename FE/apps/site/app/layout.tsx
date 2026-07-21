import type { Metadata } from "next";
import { Be_Vietnam_Pro } from "next/font/google";
import "@eduhub/ui/tokens.css";
import "@eduhub/ui/components.css";
import "./globals.css";
import { PublicFooter } from "@/components/public-footer";
import { PublicHeader } from "@/components/public-header";
import { Providers } from "./providers";

const font = Be_Vietnam_Pro({ subsets: ["latin", "vietnamese"], weight: ["400", "500", "600", "700"] });

export const metadata: Metadata = {
  title: { default: "EduHub | Quản lý học vụ và kết quả học tập", template: "%s | EduHub" },
  description: "Quản lý học sinh, lớp học, sổ điểm, thông báo và báo cáo trong cùng một hệ thống.",
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000"),
  openGraph: { title: "EduHub", description: "Một hệ thống cho lớp học, sổ điểm và kết quả của học sinh.", type: "website", locale: "vi_VN" },
};

/** Root layout cung cấp navigation, theme và metadata cho public site. */
export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi" suppressHydrationWarning>
      <body suppressHydrationWarning className={font.className}>
        <Providers>
          <a className="skip-link" href="#main-content">Chuyển tới nội dung chính</a>
          <div className="site-ambient" aria-hidden="true"><span /><span /><span /><span /><span /><span /></div>
          <PublicHeader />
          <main id="main-content">{children}</main>
          <PublicFooter />
        </Providers>
      </body>
    </html>
  );
}
