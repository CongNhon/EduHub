using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Ghi chú: AddClassAssignmentEnrollment tạo bảng lớp học, phân công giáo viên và ghi danh học sinh.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260713120000_AddClassAssignmentEnrollment")]
    public partial class AddClassAssignmentEnrollment : Migration
    {
        /// <summary>
        /// Ghi chú: Up tạo schema cho class_rooms, teaching_assignments và enrollments.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class_rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    normalized_class_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    academic_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_level = table.Column<int>(type: "integer", nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    active_enrollment_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_rooms", x => x.id);
                    table.CheckConstraint("ck_class_rooms_active_count_valid", "active_enrollment_count >= 0 AND active_enrollment_count <= capacity");
                    table.CheckConstraint("ck_class_rooms_capacity_positive", "capacity > 0");
                    table.CheckConstraint("ck_class_rooms_grade_level_positive", "grade_level > 0");
                    table.ForeignKey(
                        name: "FK_class_rooms_academic_years_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "academic_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    semester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    enrolled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_enrollments_class_rooms_class_room_id",
                        column: x => x.class_room_id,
                        principalTable: "class_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollments_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_enrollments_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teaching_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    semester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teaching_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_class_rooms_class_room_id",
                        column: x => x.class_room_id,
                        principalTable: "class_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teaching_assignments_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_class_rooms_academic_year_id_normalized_class_code",
                table: "class_rooms",
                columns: new[] { "academic_year_id", "normalized_class_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_class_room_id",
                table: "enrollments",
                column: "class_room_id");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_student_id",
                table: "enrollments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_active_semester_student",
                table: "enrollments",
                columns: new[] { "semester_id", "student_id" },
                unique: true,
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_semester_class_room_student",
                table: "enrollments",
                columns: new[] { "semester_id", "class_room_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teaching_assignments_class_room_id",
                table: "teaching_assignments",
                column: "class_room_id");

            migrationBuilder.CreateIndex(
                name: "ix_teaching_assignments_subject_id",
                table: "teaching_assignments",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_teaching_assignments_teacher_id",
                table: "teaching_assignments",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ux_teaching_assignments_active_scope",
                table: "teaching_assignments",
                columns: new[] { "semester_id", "class_room_id", "subject_id", "teacher_id" },
                unique: true,
                filter: "is_active = true");
        }

        /// <summary>
        /// Ghi chú: Down xóa schema cho enrollments, teaching_assignments và class_rooms.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "enrollments");
            migrationBuilder.DropTable(name: "teaching_assignments");
            migrationBuilder.DropTable(name: "class_rooms");
        }
    }
}
