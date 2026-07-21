import Link from "next/link";
import { Logo } from "./logo";

/** PublicFooter gom product, resources, legal và contact theo tài liệu UX. */
export function PublicFooter() {
  return (
    <footer className="public-footer">
      <div className="public-footer__inner">
        <div className="footer-intro"><Logo /><p>Quản lý học sinh, lớp học, sổ điểm và báo cáo trong một quy trình chung cho nhà trường, giáo viên và gia đình.</p></div>
        <div><h2>Sản phẩm</h2><Link href="/features">Tính năng</Link><Link href="/for-parents">Phụ huynh</Link><Link href="/for-teachers">Giáo viên</Link><Link href="/for-schools">Nhà trường</Link></div>
        <div><h2>Tài nguyên</h2><Link href="/results">Xem kết quả</Link><Link href="/support">Trợ giúp</Link><Link href="/about">Về EduHub</Link></div>
        <div><h2>Pháp lý</h2><Link href="/privacy">Quyền riêng tư</Link><Link href="/terms">Điều khoản</Link><a href="mailto:support@eduhub.vn">support@eduhub.vn</a></div>
      </div>
      <div className="public-footer__bottom"><span>© 2026 EduHub</span><span>Mỗi tài khoản chỉ truy cập dữ liệu được nhà trường cấp quyền.</span></div>
    </footer>
  );
}
