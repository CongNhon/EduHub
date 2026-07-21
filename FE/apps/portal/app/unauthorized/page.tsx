import { ShieldX } from "lucide-react";
export default function UnauthorizedPage() { return <main className="standalone-state"><ShieldX /><h1>Không có quyền truy cập</h1><p>Tài khoản hiện tại không được phép mở khu vực này.</p><a className="ui-button ui-button--primary" href="/login">Về trang đăng nhập</a></main>; }
