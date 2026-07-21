import type { Metadata } from "next";
import { GraduationCap } from "lucide-react";
import { ContentPage } from "@/components/content-page";

export const metadata: Metadata = { title: "Dành cho học sinh" };
/** Trang học sinh mô tả cách xem điểm theo môn và báo cáo học kỳ. */
export default function StudentsPage() { return <ContentPage eyebrow="DÀNH CHO HỌC SINH" title="Xem rõ từng môn, từng đầu điểm và kết quả học kỳ" description="EduHub gom kết quả học tập vào một nơi để học sinh biết điểm nào đã được công bố, điểm đó thuộc bài kiểm tra nào và báo cáo nào có thể tải xuống." icon={GraduationCap} outcomes={[{ title: "Xem theo học kỳ", copy: "Chọn đúng năm học và học kỳ để không nhầm kết quả giữa các giai đoạn." },{ title: "Hiểu từng thành phần điểm", copy: "Xem điểm kiểm tra, điểm giữa kỳ, cuối kỳ và trọng số do nhà trường cấu hình." },{ title: "Không bỏ lỡ cập nhật", copy: "Thông báo mới dẫn trực tiếp tới kết quả vừa được giáo viên và nhà trường công bố." }]} steps={["Đăng nhập bằng tài khoản học sinh.","Mở mục Kết quả học tập hoặc chọn một thông báo mới.","Xem chi tiết từng môn và tải báo cáo học kỳ khi hoàn tất."]} />; }
