import { backendFetch } from "@/lib/backend";

const blockedAuthPaths = new Set(["api/v1/auth/login", "api/v1/auth/refresh", "api/v1/auth/logout"]);

/** Proxy same-origin chuyển request có access token tới backend và giữ correlation header. */
async function proxy(request: Request, context: { params: Promise<{ path: string[] }> }) {
  const { path } = await context.params;
  const joined = path.join("/");
  if (blockedAuthPaths.has(joined)) return Response.json({ title: "Endpoint bị chặn", detail: "Sử dụng session BFF." }, { status: 404 });
  const sourceUrl = new URL(request.url);
  const headers = new Headers();
  for (const name of ["authorization", "content-type", "accept", "x-correlation-id", "idempotency-key", "if-match"]) {
    const value = request.headers.get(name);
    if (value) headers.set(name, value);
  }
  if (!headers.has("x-correlation-id")) headers.set("x-correlation-id", crypto.randomUUID());
  const method = request.method.toUpperCase();
  const upstream = await backendFetch(`/${joined}${sourceUrl.search}`, { method, headers, body: method === "GET" || method === "HEAD" ? undefined : await request.arrayBuffer() });
  const responseHeaders = new Headers();
  for (const name of ["content-type", "content-disposition", "x-correlation-id", "retry-after", "etag"]) {
    const value = upstream.headers.get(name);
    if (value) responseHeaders.set(name, value);
  }
  return new Response(upstream.body, { status: upstream.status, headers: responseHeaders });
}

export const GET = proxy;
export const POST = proxy;
export const PUT = proxy;
export const PATCH = proxy;
export const DELETE = proxy;
