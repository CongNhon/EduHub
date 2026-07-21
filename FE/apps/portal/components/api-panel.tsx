import { AlertTriangle, RefreshCw } from "lucide-react";
import { Button, Card, EmptyState } from "@eduhub/ui";
import type { ReactNode } from "react";

/** LoadingPanel giữ kích thước khu vực dữ liệu trong lúc tải lần đầu. */
export function LoadingPanel({ rows = 5 }: { rows?: number }) { return <Card className="loading-panel" aria-label="Đang tải dữ liệu">{Array.from({ length: rows }).map((_, index) => <span key={index} style={{ width: `${88 - index * 7}%` }} />)}</Card>; }

/** ErrorPanel hiển thị lỗi cần hành động và correlation ID nếu có. */
export function ErrorPanel({ message, correlationId, onRetry }: { message: string; correlationId?: string; onRetry?: () => void }) { return <Card><EmptyState icon={AlertTriangle} title="Không tải được dữ liệu" description={`${message}${correlationId ? ` · Mã hỗ trợ: ${correlationId}` : ""}`} action={onRetry ? <Button variant="outline" onClick={onRetry}><RefreshCw size={16} /> Thử lại</Button> : undefined} /></Card>; }

/** MetricCard hiển thị một chỉ số có bối cảnh và không dùng màu làm tín hiệu duy nhất. */
export function MetricCard({ label, value, caption, icon }: { label: string; value: string | number; caption: string; icon: ReactNode }) { return <Card className="metric-card"><div className="metric-card__icon">{icon}</div><span>{label}</span><strong>{value}</strong><small>{caption}</small></Card>; }
