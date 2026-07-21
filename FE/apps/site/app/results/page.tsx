import type { Metadata } from "next";
import { ArrowRight, CheckCircle2, LockKeyhole } from "lucide-react";

export const metadata: Metadata = { title: "Xem kết quả an toàn" };

/** Trang results hướng dẫn xem điểm qua portal, không cho public lookup PII. */
export default function ResultsPage() {
  const portalUrl = process.env.NEXT_PUBLIC_PORTAL_URL || "http://localhost:3001";
  return <section className="results-page"><div className="results-page__intro"><span><LockKeyhole size={18} /> KẾT QUẢ CHỈ DÀNH CHO NGƯỜI CÓ QUYỀN</span><h1>Mở điểm và báo cáo học tập của bạn</h1><p>Dùng tài khoản do nhà trường cấp. Sau khi đăng nhập, EduHub tự hiển thị học sinh và kết quả mà tài khoản được phép xem.</p><a className="ui-button ui-button--primary" href={`${portalUrl}/login`}>Đăng nhập để xem kết quả <ArrowRight size={17} /></a></div><div className="results-steps">{["Đăng nhập bằng email và mật khẩu EduHub.","Chọn học sinh, năm học và học kỳ cần xem.","Mở điểm từng môn hoặc tải báo cáo đã hoàn thành."].map((step, index) => <div key={step}><b>{index + 1}</b><p>{step}</p><CheckCircle2 /></div>)}</div><div className="safe-note"><h2>Tại sao EduHub không có ô tra cứu điểm công khai?</h2><p>Mã học sinh, họ tên và ngày sinh có thể bị người khác biết. Đăng nhập giúp hệ thống xác minh đúng phụ huynh hoặc học sinh trước khi hiển thị dữ liệu học tập.</p></div></section>;
}
