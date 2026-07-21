import type { MetadataRoute } from "next";

/** sitemap khai báo các route public được phép index. */
export default function sitemap(): MetadataRoute.Sitemap {
  const base = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000";
  return ["", "/features", "/for-parents", "/for-students", "/for-teachers", "/for-schools", "/results", "/about", "/support", "/privacy", "/terms"].map((path) => ({ url: `${base}${path}`, lastModified: new Date(), changeFrequency: path === "" ? "weekly" : "monthly", priority: path === "" ? 1 : .7 }));
}
