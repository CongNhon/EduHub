using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolPeopleGradebookAndReportRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_code",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "normalized_full_name",
                table: "students",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "students",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("UPDATE users SET full_name = email WHERE full_name = '';");
            migrationBuilder.Sql("UPDATE students SET normalized_full_name = lower(unaccent(full_name));");

            migrationBuilder.CreateTable(
                name: "report_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    semester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    report_job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    review_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_requests_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_requests_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_requests_users_requester_user_id",
                        column: x => x.requester_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_requests_users_reviewer_user_id",
                        column: x => x.reviewer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_remarks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_remarks", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_remarks_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_remarks_teaching_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "teaching_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_remarks_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_reference_code",
                table: "users",
                column: "reference_code");

            migrationBuilder.CreateIndex(
                name: "ix_students_normalized_full_name",
                table: "students",
                column: "normalized_full_name");

            migrationBuilder.CreateIndex(
                name: "ux_students_user_id",
                table: "students",
                column: "user_id",
                unique: true,
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_report_requests_owner_scope_status",
                table: "report_requests",
                columns: new[] { "requester_user_id", "student_id", "semester_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_report_requests_report_job_id",
                table: "report_requests",
                column: "report_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_requests_reviewer_user_id",
                table: "report_requests",
                column: "reviewer_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_requests_semester_id",
                table: "report_requests",
                column: "semester_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_requests_student_id",
                table: "report_requests",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_remarks_student_id",
                table: "student_remarks",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_remarks_teacher_id",
                table: "student_remarks",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ux_student_remarks_assignment_student",
                table: "student_remarks",
                columns: new[] { "assignment_id", "student_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_students_users_user_id",
                table: "students",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_students_users_user_id",
                table: "students");

            migrationBuilder.DropTable(
                name: "report_requests");

            migrationBuilder.DropTable(
                name: "student_remarks");

            migrationBuilder.DropIndex(
                name: "ix_users_reference_code",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_students_normalized_full_name",
                table: "students");

            migrationBuilder.DropIndex(
                name: "ux_students_user_id",
                table: "students");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "users");

            migrationBuilder.DropColumn(
                name: "reference_code",
                table: "users");

            migrationBuilder.DropColumn(
                name: "normalized_full_name",
                table: "students");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "students");
        }
    }
}
