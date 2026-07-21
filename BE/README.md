# EduHub

EduHub là ASP.NET Core 10 API và Next.js portal cho quản lý học sinh, lớp, chương trình học, thời khóa biểu, điểm, thông báo, report PDF, cache Redis, Hangfire jobs, Ministry sync qua Refit/Polly và audit log MongoDB/Serilog.

## Prerequisites

- .NET SDK 10
- Docker Desktop
- Node.js LTS
- pnpm 11.13 (`npm install -g pnpm@11.13.0`)

## Backend workspace

```powershell
Copy-Item .env.example .env
.\start-backend.ps1
```

`start-backend.ps1` đọc `.env`, khởi động PostgreSQL/MongoDB/Redis local, apply migrations và chạy `dotnet watch` tại cổng `8080`. Seed không tự chạy cùng API.

Seed bộ dữ liệu test độc lập:

```powershell
.\start-backend.ps1 -InitializeOnly -SeedData
```

Frontend nằm tại thư mục `../FE` trong cùng EduHub monorepo.

Swagger:

```text
http://localhost:8080/swagger
```

Postman:

```text
docs/postman/EduHub.postman_collection.json
```

Health:

```text
GET /health
GET /health/live
GET /health/ready
```

- `/health/live`: public, chỉ trả process status.
- `/health` và `/health/ready`: yêu cầu JWT role `SystemAdmin`.

`/health/ready` pings:

```text
postgres, redis, mongo, ministry
```

## Docker local — fresh clone

Lần đầu clone:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

Compose tự chạy PostgreSQL, Redis, MongoDB, migration one-shot và API. Các cổng chỉ bind vào `127.0.0.1`; dữ liệu nằm trong named volumes của máy hiện tại.

Seed dữ liệu test khi cần:

```powershell
docker compose --profile tools run --rm eduhub-seed
```

Seed idempotent: chạy lại không xóa dữ liệu cũ. Muốn tạo database sạch hoàn toàn:

```powershell
docker compose down -v
docker compose up -d --build
docker compose --profile tools run --rm eduhub-seed
```

Services:

```text
API: http://localhost:8080
PostgreSQL local: localhost:5433
Redis: localhost:6379
MongoDB local: localhost:27018
```

Docker giữ PostgreSQL, MongoDB, Redis, evidence và report trong named volumes; local `.NET` giữ file dưới `.local`.

## Development account

Seeder riêng tạo admin bằng cấu hình trong `.env`:

```text
Email: admin@eduhub.local
Password: Admin@123456
Role: SystemAdmin
```

Development seed also creates happy-flow accounts:

```text
AcademicAdmin: academic@eduhub.local / Admin@123456
Teacher: teacher@eduhub.local / Admin@123456
Parent: parent@eduhub.local / Admin@123456
Student: student@eduhub.local / Admin@123456
```

Development seed also creates academic test data:

```text
AcademicYear: EduHub 2026-2027
Semester: HK1
Subject: MATH10
Class: 10A1
Student: STU0001
TeachingAssignment + GradeComponents
```

Extended school seed tạo chương trình 35 tuần, môn THPT, 9 lớp khối 10-12, 24 giáo viên, năng lực giảng dạy, GVCN và 30 học sinh/phụ huynh mỗi lớp.

## Architecture

```text
WebApi Module
-> DTO
-> Mapping
-> Application Command/Query
-> Feature Handler
-> Service Interface
-> Application Service
-> Repository Interface
-> Infrastructure Repository
-> ApplicationDbContext
-> PostgreSQL
```

Folder chính:

```text
src/EduHub.Domain         Entity, enum, domain rule
src/EduHub.Application    Contract, feature, service, repository interface
src/EduHub.Infrastructure EF Core, repository implementation, Redis, Hangfire, Refit, Mongo audit
src/EduHub.WebApi         Carter modules, DTO, mapping, middleware, Swagger
tests                     Unit, integration, architecture tests
```

## Key flows

Auth:

```text
POST /api/v1/auth/login -> JWT + refresh token
POST /api/v1/auth/refresh -> rotate refresh token
POST /api/v1/auth/logout -> revoke current refresh token
```

Grades:

```text
Teacher -> update/bulk grades -> submit
AcademicAdmin -> publish/reopen/lock
Publish/Lock -> Outbox -> Notification/Email/Ministry sync
Parent -> read Published/Locked only
```

Reports:

```text
POST /api/v1/reports/report-cards -> Hangfire PDF job
GET /api/v1/reports/jobs/{id}
GET /api/v1/reports/jobs/{id}/download
```

Integration:

```text
POST /api/v1/admin/sync/grades/{assignmentId}/retry
GET /api/v1/admin/sync/records/{id}
```

## Configuration

Core env keys:

```text
ConnectionStrings__Postgres
ConnectionStrings__Redis
ConnectionStrings__Mongo
Auth__Jwt__Issuer
Auth__Jwt__Audience
Auth__Jwt__Secret
Grades__AutoLockAfterDays
Reports__StoragePath
EvidenceStorage__R2__BucketName
EvidenceStorage__R2__ServiceUrl
EvidenceStorage__R2__AccessKey
EvidenceStorage__R2__SecretKey
EvidenceStorage__LocalRootPath
MinistryApi__BaseUrl
MinistryApi__ApiKey
Audit__Mongo__Enabled
Audit__Mongo__RetentionDays
```

## Audit and safety

```text
PostgreSQL = source of truth
Redis outage = fallback to DB
Mongo audit outage = request continues
Ministry outage = local grade remains committed, sync record retries
SignalR/email/sync = side effects after commit
```

Serilog/Mongo audit fields include:

```text
timestampUtc, level, correlationId, actorUserId, actorRole, useCase, duration, exception type
```

Redacted:

```text
password, token, refresh token, API key, Authorization header, connection string
```

## Current quality status

P21 backend đã build thành công trong Docker, migration `20260715072650_AddCurriculumTimetableAndProfiles` đã apply lên Neon và Portal production build đã chạy thành công. Full test suite chưa chạy tại checkpoint này.

P22 timetable refactor đã build thành công ngày 2026-07-15. Migration `20260715153000_ReplaceCycleWeeksWithTeachingWeeks` và corrective migration `20260715093356_NormalizeLegacyAfternoonPeriods` đã apply lên Neon; dữ liệu tiết chiều legacy `6/7` đã chuẩn hóa thành `1/2` và được bảo vệ bằng DB constraint. Development seed đã đồng bộ quota/năng lực giáo viên và Docker API đã rebuild. API/Swagger cùng Site/Portal production đã smoke-check; full test suite P22 chưa chạy tại checkpoint này.

P23 đã chuẩn hóa Backend/Frontend thành hai workspace `BE/` và `FE/` trong cùng Git repository, thêm Docker local databases, migration gate và seeder riêng. Backend build `0 warning/error`, toàn bộ `22/22` tests, Frontend typecheck/lint/build, NuGet/pnpm vulnerability audits và full Docker runtime rehearsal đều PASS ngày 2026-07-21.

Last verified commands:

```powershell
dotnet build .\EduHub.sln
dotnet test .\EduHub.sln --no-build
dotnet ef database update --project .\src\EduHub.Infrastructure\EduHub.Infrastructure.csproj --startup-project .\src\EduHub.WebApi\EduHub.WebApi.csproj --context ApplicationDbContext
docker compose up -d --build eduhub-api
```

Known gap:

```text
Real Ministry sandbox URL/auth still needs provider confirmation.
Rotate external secrets before any real deployment.
```
