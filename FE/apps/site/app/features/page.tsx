import type { Metadata } from "next";
import { BellRing, BookOpenCheck, FileText, School, Settings2, Users } from "lucide-react";

export const metadata: Metadata = { title: "Tính năng" };

/** Trang tính năng mô tả phạm vi backend thật của EduHub. */
export default function FeaturesPage() {
  const features = [
    { icon: School, title: "Tổ chức năm học và lớp học", copy: "Tạo năm học, học kỳ, môn, lớp; phân công giáo viên và quản lý danh sách học sinh đang học trong từng lớp." },
    { icon: Users, title: "Quản lý hồ sơ học sinh", copy: "Tạo, cập nhật và tìm kiếm học sinh; chuyển lớp, rút khỏi lớp hoặc liên kết tài khoản phụ huynh khi cần." },
    { icon: Settings2, title: "Cấu hình cách tính điểm", copy: "Khai báo các thành phần như kiểm tra thường xuyên, giữa kỳ và cuối kỳ cùng trọng số phù hợp cho từng môn." },
    { icon: BookOpenCheck, title: "Nhập và công bố sổ điểm", copy: "Giáo viên lưu nháp và nộp sổ điểm; nhà trường kiểm tra, công bố, khóa hoặc mở lại khi cần điều chỉnh." },
    { icon: BellRing, title: "Thông báo ngay khi có thay đổi", copy: "Người dùng đang mở EduHub nhận thông báo mới mà không cần tải lại trang, sau đó đi thẳng tới nội dung liên quan." },
    { icon: FileText, title: "Tạo và tải báo cáo", copy: "Yêu cầu báo cáo học tập, theo dõi trạng thái xử lý và tải tệp khi hệ thống hoàn thành." },
  ];
  return <section className="catalog-page"><div className="section-heading"><span>TÍNH NĂNG EDUHUB</span><h1>Từ dữ liệu đầu năm tới báo cáo kết quả</h1><p>Các công việc học vụ chính được nối thành một quy trình để dữ liệu chỉ cần nhập đúng một lần và tiếp tục được sử dụng ở các bước sau.</p></div><div className="feature-catalog">{features.map((feature) => <article key={feature.title}><span><feature.icon /></span><h2>{feature.title}</h2><p>{feature.copy}</p></article>)}</div></section>;
}
