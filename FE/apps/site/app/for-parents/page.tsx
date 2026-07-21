import type { Metadata } from "next";
import { Users } from "lucide-react";
import { ContentPage } from "@/components/content-page";

export const metadata: Metadata = { title: "Dành cho phụ huynh" };
/** Trang phụ huynh mô tả cách theo dõi kết quả của từng học sinh đã liên kết. */
export default function ParentsPage() { return <ContentPage eyebrow="DÀNH CHO PHỤ HUYNH" title="Biết con đang học thế nào ngay khi nhà trường công bố" description="Theo dõi kết quả của từng con theo học kỳ và môn học. Khi có điểm mới, EduHub gửi thông báo để phụ huynh mở đúng nội dung cần xem." icon={Users} outcomes={[{ title: "Theo dõi từng con", copy: "Chuyển nhanh giữa các học sinh đã được nhà trường liên kết với tài khoản phụ huynh." },{ title: "Xem kết quả đã xác nhận", copy: "Chỉ điểm đã được nhà trường công bố mới xuất hiện; điểm nháp của giáo viên luôn được giữ kín." },{ title: "Nhận thông báo điểm mới", copy: "Mở thông báo để đi thẳng tới môn học, học kỳ và kết quả vừa được cập nhật." }]} steps={["Đăng nhập bằng tài khoản phụ huynh do nhà trường cấp.","Chọn học sinh, học kỳ và môn học muốn theo dõi.","Xem chi tiết điểm hoặc tải báo cáo học kỳ khi đã sẵn sàng."]} />; }
