import { z } from "zod";

export const loginSchema = z.object({
  email: z.email("Email không đúng định dạng."),
  password: z.string().min(1, "Vui lòng nhập mật khẩu."),
});

export const studentSchema = z.object({
  studentCode: z.string().trim().min(2, "Mã học sinh cần ít nhất 2 ký tự.").max(50),
  fullName: z.string().trim().min(2, "Họ tên cần ít nhất 2 ký tự.").max(150),
  dateOfBirth: z.string().min(1, "Vui lòng chọn ngày sinh."),
});

export const academicYearSchema = z.object({
  name: z.string().trim().min(3, "Tên năm học cần ít nhất 3 ký tự."),
  startDate: z.string().min(1, "Vui lòng chọn ngày bắt đầu."),
  endDate: z.string().min(1, "Vui lòng chọn ngày kết thúc."),
}).refine((value) => value.endDate > value.startDate, { message: "Ngày kết thúc phải sau ngày bắt đầu.", path: ["endDate"] });

export const subjectSchema = z.object({
  subjectCode: z.string().trim().min(2).max(30),
  name: z.string().trim().min(2).max(120),
  credits: z.coerce.number().int().min(1).max(20),
  maxScore: z.coerce.number().positive().max(100),
});

export const classSchema = z.object({
  classCode: z.string().trim().min(2).max(30),
  name: z.string().trim().min(2).max(120),
  academicYearId: z.uuid("Vui lòng chọn năm học."),
  gradeLevel: z.coerce.number().int().min(1).max(12),
  capacity: z.coerce.number().int().min(1).max(100),
});
