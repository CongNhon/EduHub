import { Bell, BookOpenCheck, CheckCircle2, FileText, TrendingUp } from "lucide-react";

/** ProductScene mô phỏng đúng các bề mặt điểm, report và notification đã có trong EduHub. */
export function ProductScene() {
  return (
    <div className="product-scene" aria-label="Xem trước portal EduHub">
      <div className="product-scene__shell">
        <aside className="mock-sidebar"><div className="mock-logo">E</div><span className="active" /><span /><span /><span /><span /></aside>
        <div className="mock-content">
          <div className="mock-topbar"><i /><div><span /><span /></div></div>
          <div className="mock-greeting"><small>TỔNG QUAN HỌC KỲ</small><strong>Tiến độ học tập của Minh Anh</strong><p>Học kỳ II · Năm học 2025–2026</p></div>
          <div className="mock-grid">
            <div className="mock-metric"><span className="mock-icon teal"><TrendingUp size={18} /></span><small>Điểm trung bình</small><strong>8.42</strong><em>12 môn đã công bố</em></div>
            <div className="mock-metric"><span className="mock-icon indigo"><BookOpenCheck size={18} /></span><small>Điểm mới</small><strong>04</strong><em>Trong 7 ngày qua</em></div>
            <div className="mock-metric"><span className="mock-icon amber"><FileText size={18} /></span><small>Báo cáo</small><strong>02</strong><em>Sẵn sàng tải xuống</em></div>
          </div>
          <div className="mock-lower">
            <div className="mock-subjects"><div className="mock-section-title">Kết quả gần đây <span>Xem tất cả</span></div>{[["Toán học","8.8"],["Ngữ văn","8.2"],["Tiếng Anh","8.6"]].map(([name,score]) => <div className="mock-row" key={name}><span className="mock-dot" /><b>{name}</b><i /><strong>{score}</strong><CheckCircle2 size={15} /></div>)}</div>
            <div className="mock-alert"><Bell size={18} /><div><small>THÔNG BÁO MỚI</small><b>Điểm Toán đã được công bố</b><p>Vừa xong · Xem chi tiết</p></div></div>
          </div>
        </div>
      </div>
      <div className="phone-preview"><div className="phone-notch" /><div className="phone-content"><span className="phone-eyebrow">KẾT QUẢ HỌC TẬP</span><h3>Chào Minh Anh</h3><p>Học kỳ II</p><div className="phone-score"><small>Điểm trung bình</small><strong>8.42</strong><span>Khá tốt</span></div><div className="phone-item"><i /><div><b>Toán học</b><small>Đã công bố</small></div><strong>8.8</strong></div><div className="phone-item"><i /><div><b>Ngữ văn</b><small>Đã công bố</small></div><strong>8.2</strong></div></div></div>
    </div>
  );
}
