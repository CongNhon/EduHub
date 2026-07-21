import type { Metadata } from "next";
import { School } from "lucide-react";
import { ContentPage } from "@/components/content-page";

export const metadata: Metadata = { title: "Dành cho giáo viên" };
/** Trang giáo viên mô tả quy trình nhập, kiểm tra và nộp sổ điểm. */
export default function TeachersPage() { return <ContentPage eyebrow="DÀNH CHO GIÁO VIÊN" title="Hoàn thành sổ điểm theo đúng lớp và môn được phân công" description="Danh sách học sinh, các cột điểm và trạng thái sổ điểm được đặt trên cùng màn hình để giáo viên nhập liệu, kiểm tra và nộp duyệt mà không phải chuyển qua nhiều công cụ." icon={School} outcomes={[{ title: "Nhập điểm theo danh sách lớp", copy: "Cập nhật từng học sinh hoặc nhiều điểm cùng lúc theo cấu hình của môn học." },{ title: "Phát hiện điểm thiếu trước khi nộp", copy: "Các giá trị không hợp lệ và học sinh chưa đủ điểm được chỉ rõ để giáo viên xử lý." },{ title: "Điều chỉnh có kiểm soát", copy: "Sổ điểm đã công bố chỉ được mở lại khi có quyền, giúp mọi thay đổi đều theo đúng quy trình." }]} steps={["Chọn lớp, môn và học kỳ trong danh sách được phân công.","Nhập điểm, lưu bản nháp và xử lý các mục còn thiếu.","Nộp sổ điểm để quản trị học vụ kiểm tra và công bố."]} />; }
