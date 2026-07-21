import type { MetadataRoute } from "next";

/** robots cho phép index website public và không tham chiếu portal riêng tư. */
export default function robots(): MetadataRoute.Robots {
  const base = process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000";
  return { rules: { userAgent: "*", allow: "/" }, sitemap: `${base}/sitemap.xml` };
}
