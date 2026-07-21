import { Clock3 } from "lucide-react";
export default function SessionExpiredPage() { return <main className="standalone-state"><Clock3 /><h1>Phiên đăng nhập đã hết hạn</h1><p>Đăng nhập lại để tiếp tục từ màn hình hiện tại.</p><a className="ui-button ui-button--primary" href="/login">Đăng nhập lại</a></main>; }
