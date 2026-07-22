# EduHub Frontend

Frontend workspace cho EduHub Site và Portal. Backend nằm tại thư mục `../BE` trong cùng EduHub monorepo.

Portal sinh local stylesheet cho DevExtreme và DevExpress Analytics trước `dev`, `build`, `typecheck`, `lint`. Component `DevExpressTheme` đổi đồng thời stylesheet Light/Dark theo `next-themes`, tránh chữ sáng trên nền sáng.

## Docker — fresh clone

Khởi chạy Backend trước để tạo network `eduhub-dev`, sau đó:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

```text
Site: http://localhost:3000
Portal: http://localhost:3001
Backend API qua internal Docker network: http://eduhub-api:8080
```

Không commit `.env`, token, cookie hoặc secret. Các biến `NEXT_PUBLIC_*` luôn được xem là dữ liệu public.

Monorepo frontend gồm hai ứng dụng độc lập:

- `apps/site`: website công khai, không đọc dữ liệu học sinh.
- `apps/portal`: portal đăng nhập và phân quyền theo backend.
- `packages/ui`: design system và theme dùng chung.
- `packages/api-client`: OpenAPI client sinh từ Swagger.
- `packages/auth`: role, session contract và route guard helpers.
- `packages/validation`: schema form dùng chung.

## Chạy local

```powershell
pnpm install
pnpm api:generate
pnpm dev:site
pnpm dev:portal
```

Hoặc dùng PowerShell 5.1:

```powershell
.\start-frontend.ps1 -App portal
.\start-frontend.ps1 -App site
```

- Public site: `http://localhost:3000`
- Portal: `http://localhost:3001`
- Backend mặc định: `http://localhost:8080`

Refresh token chỉ nằm trong HttpOnly cookie do BFF của portal quản lý. Access token chỉ giữ trong memory của tab.

## Role routes

```text
Student:       /student/timetable, /student/profile
Parent:        /parent/timetable
Teacher:       /teacher/timetable
AcademicAdmin: /academic/scheduling, /academic/imports, /academic/profile-requests
```

- Timetable: tuần thực tế có khoảng ngày và tuần hiện tại; chỉ đọc bản Published theo resource scope; AcademicAdmin sinh, hoán đổi slot, đổi giáo viên, khóa và công bố bản Draft.
- Student profile: upload ảnh bằng chứng rồi gửi request; hồ sơ chỉ đổi sau AcademicAdmin approval.
- Excel import: tải template, upload XLSX và nhận kết quả từng dòng/mật khẩu tạm một lần.

## DevExpress client license

- `pnpm dev/build/typecheck/lint` tự chạy `devextreme-license` từ private key đã cài trên máy.
- Public client key được sinh tại `apps/portal/app/devextreme-license.ts` và bị Git ignore.
- Docker đọc `%AppData%\DevExpress\DevExpress_License.txt` bằng BuildKit secret; CI dùng GitHub secret `DevExpressLicenseKey`.

## SystemAdmin analytics

```text
/admin                DevExtreme KPI, charts, grids và semester filter
/admin/reports        DevExpress Web Document Viewer + PDF/XLSX/CSV
/admin/system-health  Dependency health + Redis/Hangfire/queue monitoring
```

- Viewer gọi `/api/backend/DXXRDV`; access token chỉ gắn vào request trong memory.
- Session mutation kiểm tra public `Host/X-Forwarded-*`, nên chạy đúng cả local và Docker reverse proxy.
