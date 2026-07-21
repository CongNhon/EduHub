export const roles = ["SystemAdmin", "AcademicAdmin", "Teacher", "Parent", "Student"] as const;
export type UserRole = (typeof roles)[number];

export interface CurrentUser {
  email: string;
  fullName: string;
  role: UserRole;
}

export interface AuthSession {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: CurrentUser;
}

/** Kiểm tra role trả về từ backend có thuộc tập role EduHub hỗ trợ. */
export function isUserRole(value: string): value is UserRole {
  return roles.includes(value as UserRole);
}

/** Trả route dashboard mặc định cho role sau đăng nhập. */
export function homeForRole(role: UserRole) {
  const homes: Record<UserRole, string> = {
    Parent: "/parent",
    Student: "/student",
    Teacher: "/teacher",
    AcademicAdmin: "/academic",
    SystemAdmin: "/admin",
  };
  return homes[role];
}

/** Trả nhãn tiếng Việt cho role để hiển thị trên portal. */
export function roleLabel(role: UserRole) {
  const labels: Record<UserRole, string> = {
    Parent: "Phụ huynh",
    Student: "Học sinh",
    Teacher: "Giáo viên",
    AcademicAdmin: "Quản trị học vụ",
    SystemAdmin: "Quản trị hệ thống",
  };
  return labels[role];
}
