import { Badge } from "@eduhub/ui";
import { ArrowRight, BellRing, BookOpenCheck, Building2, Check, FileCheck2, GraduationCap, LockKeyhole, School, ShieldCheck, Sparkles, Users } from "lucide-react";
import Link from "next/link";
import { ProductScene } from "@/components/product-scene";

const roleCards = [
  { icon: Users, title: "Phụ huynh", copy: "Xem kết quả đã công bố của từng con, nhận thông báo điểm mới và tải báo cáo học kỳ.", href: "/for-parents" },
  { icon: GraduationCap, title: "Học sinh", copy: "Theo dõi điểm từng môn, xem các thành phần đánh giá và biết kết quả mới ngay khi được công bố.", href: "/for-students" },
  { icon: School, title: "Giáo viên", copy: "Nhập điểm theo danh sách lớp, lưu bản nháp, rà soát điểm thiếu và nộp sổ điểm để duyệt.", href: "/for-teachers" },
  { icon: Building2, title: "Nhà trường", copy: "Quản lý học sinh, lớp học, phân công giảng dạy, công bố điểm và xuất báo cáo tập trung.", href: "/for-schools" },
];

/** Trang chủ public giới thiệu đúng sản phẩm và dẫn người dùng tới portal bảo mật. */
export default function HomePage() {
  const portalUrl = process.env.NEXT_PUBLIC_PORTAL_URL || "http://localhost:3001";
  return (
    <>
      <section className="hero">
        <div className="hero__scene"><ProductScene /></div>
        <div className="hero__content reveal">
          <Badge tone="info"><Sparkles size={13} /> Quản lý học vụ và kết quả học tập</Badge>
          <h1>EduHub</h1>
          <h2>Một hệ thống cho lớp học, sổ điểm và kết quả của học sinh.</h2>
          <p>Giáo viên nhập và nộp điểm. Nhà trường duyệt, công bố và xuất báo cáo. Phụ huynh, học sinh nhận đúng kết quả thuộc về mình.</p>
          <div className="hero__actions"><a className="ui-button ui-button--primary" href={`${portalUrl}/login`}>Đăng nhập hệ thống <ArrowRight size={17} /></a><Link className="ui-button ui-button--outline" href="/features">Xem EduHub làm được gì</Link></div>
          <div className="hero__proof"><span><Check size={16} /> Điểm nháp không hiển thị cho gia đình</span><span><Check size={16} /> Mỗi vai trò có quyền riêng</span></div>
        </div>
      </section>

      <section className="trust-strip"><span><LockKeyhole size={18} /> Bảo vệ dữ liệu học sinh</span><span><BellRing size={18} /> Báo ngay khi có điểm mới</span><span><FileCheck2 size={18} /> Tải báo cáo học kỳ</span></section>

      <section className="marketing-section">
        <div className="section-heading"><span>MỖI NGƯỜI MỘT KHÔNG GIAN LÀM VIỆC</span><h2>Đăng nhập một lần, thấy đúng việc cần làm</h2><p>Phụ huynh không nhìn thấy màn hình nhập điểm. Giáo viên không phải tìm chức năng học vụ của quản trị viên. EduHub tự mở đúng khu vực theo vai trò tài khoản.</p></div>
        <div className="role-grid">{roleCards.map((card, index) => <Link href={card.href} className="role-card" key={card.title} style={{ animationDelay: `${index * 70}ms` }}><span><card.icon size={22} /></span><h3>{card.title}</h3><p>{card.copy}</p><b>Tìm hiểu thêm <ArrowRight size={15} /></b></Link>)}</div>
      </section>

      <section className="flow-band">
        <div className="flow-band__inner">
          <div className="section-heading section-heading--left"><span>TỪ SỔ ĐIỂM ĐẾN GIA ĐÌNH</span><h2>Một kết quả đi qua ba bước rõ ràng</h2></div>
          <div className="flow-steps"><div><b>01</b><School /><h3>Giáo viên hoàn thành sổ điểm</h3><p>Nhập điểm theo từng thành phần, kiểm tra học sinh còn thiếu điểm và nộp sổ điểm khi đã sẵn sàng.</p></div><i /><div><b>02</b><ShieldCheck /><h3>Nhà trường kiểm tra và công bố</h3><p>Quản trị học vụ rà soát trạng thái, yêu cầu chỉnh sửa khi cần và quyết định thời điểm công bố.</p></div><i /><div><b>03</b><BookOpenCheck /><h3>Học sinh và phụ huynh nhận kết quả</h3><p>Thông báo xuất hiện ngay sau khi công bố; người dùng có thể mở chi tiết hoặc tải báo cáo chính thức.</p></div></div>
        </div>
      </section>

      <section className="security-section">
        <div><span className="security-icon"><ShieldCheck size={28} /></span><h2>Mỗi người chỉ xem và thay đổi dữ liệu được phép</h2><p>Kết quả học tập không thể tra cứu công khai bằng mã học sinh. Người dùng phải đăng nhập và hệ thống kiểm tra quyền ở từng thao tác.</p></div>
        <ul><li><Check /> Phụ huynh chỉ xem học sinh đã được liên kết</li><li><Check /> Học sinh chỉ xem kết quả của chính mình</li><li><Check /> Giáo viên chỉ nhập điểm lớp được phân công</li><li><Check /> Điểm chỉ tới gia đình sau khi nhà trường công bố</li></ul>
      </section>

      <section className="final-cta"><div><span>BẮT ĐẦU VỚI EDUHUB</span><h2>Mở đúng khu vực dành cho tài khoản của bạn.</h2><p>Dùng email và mật khẩu do nhà trường cấp để đăng nhập.</p></div><a className="ui-button ui-button--primary" href={`${portalUrl}/login`}>Đăng nhập ngay <ArrowRight size={17} /></a></section>
    </>
  );
}
