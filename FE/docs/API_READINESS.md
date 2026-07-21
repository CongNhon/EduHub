# EduHub Frontend API Readiness

Đối chiếu trực tiếp Swagger `http://localhost:8080/swagger/v1/swagger.json` ngày 2026-07-14.

## READY

| Surface | Backend API | Frontend |
|---|---|---|
| Session | `POST /auth/login`, `/refresh`, `/logout`, `GET /auth/me` | BFF + HttpOnly refresh cookie + in-memory access token |
| Students | `GET/POST /students`, `GET/PUT /students/{id}` | Table, filter, paging, create/edit dialog |
| Academic setup | Academic years, semesters, subjects | Tabs, tables, create/edit/disable dialogs |
| Classes | `GET/POST/PUT /classes` | Table, capacity, create/edit dialogs |
| Enrollment | Single + bulk enrollment | Student picker + bulk checklist |
| Grade configuration | `GET/POST /grade-configurations` | Version table + component editor |
| Parent published grade | `GET /assignments/{assignmentId}/students/{studentId}/grades/published` | Deep-link từ notification |
| Notifications | List + mark read | Filtered inbox + optimistic rollback |
| Reports | Create/status/download | Async job card + polling + SignalR invalidation |
| Health | `/health/ready` | Dependency status for SystemAdmin UI |
| Realtime | `/hubs/notifications`, event `NotificationReceived` | One connection per authenticated tab |

## BACKEND_GAP / FEATURE-GATED

| Screen | Missing backend contract |
|---|---|
| Parent child switcher/dashboard | Linked children/current child API |
| Student grades/transcript | `/me/student/...` family-safe read models |
| Teacher classes | Teacher-scoped teaching assignments query |
| Gradebook editor | Bounded gradebook read model with rows, components, versions, capabilities |
| Parent links | Parent user lookup/search API |
| Class roster/transfer/withdraw UI | Class detail + enrollment roster read model |
| Report archive | Report job list by current user/student |
| Ministry monitor | Sync record list/filter API |
| System users/audit/settings | User CRUD, audit query and settings APIs |

Feature `BACKEND_GAP` không xuất hiện trong production navigation. Mutation rời rạc không được bật nếu thiếu read model để người dùng chọn đúng đối tượng.
