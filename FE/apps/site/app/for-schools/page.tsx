import type { Metadata } from "next";
import { Building2 } from "lucide-react";
import { ContentPage } from "@/components/content-page";

export const metadata: Metadata = { title: "Dành cho nhà trường" };
/** Trang nhà trường mô tả quy trình quản lý học vụ từ dữ liệu nền tới công bố điểm. */
export default function SchoolsPage() { return <ContentPage eyebrow="DÀNH CHO NHÀ TRƯỜNG" title="Quản lý học sinh, lớp học và kết quả trên một hệ thống" description="EduHub giúp bộ phận học vụ chuẩn bị năm học, xếp lớp, phân công giáo viên, kiểm tra sổ điểm và công bố kết quả theo cùng một quy trình." icon={Building2} outcomes={[{ title: "Dữ liệu học vụ tập trung", copy: "Quản lý năm học, học kỳ, môn học, hồ sơ học sinh, lớp và danh sách ghi danh tại một nơi." },{ title: "Kiểm soát trước khi công bố", copy: "Theo dõi sổ điểm đang nhập, đã nộp, đã công bố hoặc cần mở lại để điều chỉnh." },{ title: "Theo dõi vận hành", copy: "Kiểm tra trạng thái dịch vụ, tiến trình tạo báo cáo và các lần đồng bộ cần xử lý lại." }]} steps={["Tạo năm học, học kỳ, môn học, lớp và ghi danh học sinh.","Phân công giáo viên, cấu hình thành phần điểm và kiểm tra sổ điểm đã nộp.","Công bố kết quả, tạo báo cáo và theo dõi thông báo gửi tới người dùng."]} />; }
