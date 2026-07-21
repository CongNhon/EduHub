import { Card } from "@eduhub/ui";
import { FeatureGap } from "@/components/feature-gap";
import { PageHeader } from "@/components/page-header";

/** SyncMonitor giữ feature off khi chỉ có endpoint detail/retry nhưng không có danh sách record. */
export function SyncMonitor() { return <div className="page-stack"><PageHeader eyebrow="TÍCH HỢP" title="Đồng bộ Bộ GDĐT" description="Theo dõi trạng thái riêng biệt với công bố điểm nội bộ." /><Card className="gap-card"><FeatureGap title="Chưa có danh sách bản ghi đồng bộ" description="Backend hiện đọc được một record theo ID và retry theo assignment, nhưng portal chưa có nguồn để liệt kê và chọn record an toàn." gap="FE-GAP: GET /admin/sync/records" /></Card></div>; }
