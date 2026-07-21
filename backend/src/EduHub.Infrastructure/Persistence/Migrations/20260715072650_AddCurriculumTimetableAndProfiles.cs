using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurriculumTimetableAndProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "students",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gender",
                table: "students",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "students",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "curriculum_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_level = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    total_weeks = table.Column<int>(type: "integer", nullable: false),
                    semester_1_weeks = table.Column<int>(type: "integer", nullable: false),
                    semester_2_weeks = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_curriculum_plans_academic_years_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "academic_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "homeroom_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homeroom_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_homeroom_assignments_class_rooms_class_room_id",
                        column: x => x.class_room_id,
                        principalTable: "class_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_homeroom_assignments_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_profile_change_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_full_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    requested_date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    requested_gender = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    requested_phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    requested_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    evidence_object_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    reviewer_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_profile_change_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_profile_change_requests_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_profile_change_requests_users_requester_user_id",
                        column: x => x.requester_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_profile_change_requests_users_reviewer_user_id",
                        column: x => x.reviewer_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_subject_capabilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    max_periods_per_week = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_subject_capabilities", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_subject_capabilities_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_subject_capabilities_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "timetable_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    semester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timetable_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_timetable_versions_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_subject_quotas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    curriculum_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    annual_periods = table.Column<int>(type: "integer", nullable: false),
                    semester_1_periods = table.Column<int>(type: "integer", nullable: false),
                    semester_2_periods = table.Column<int>(type: "integer", nullable: false),
                    week_a_periods = table.Column<int>(type: "integer", nullable: false),
                    week_b_periods = table.Column<int>(type: "integer", nullable: false),
                    can_double_period = table.Column<bool>(type: "boolean", nullable: false),
                    max_periods_per_day = table.Column<int>(type: "integer", nullable: false),
                    includes_homeroom = table.Column<bool>(type: "boolean", nullable: false),
                    preferred_session = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_subject_quotas", x => x.id);
                    table.ForeignKey(
                        name: "FK_curriculum_subject_quotas_curriculum_plans_curriculum_plan_~",
                        column: x => x.curriculum_plan_id,
                        principalTable: "curriculum_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_curriculum_subject_quotas_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "timetable_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timetable_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cycle_week = table.Column<int>(type: "integer", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    session = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    period_number = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    counts_toward_quota = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timetable_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_timetable_entries_class_rooms_class_room_id",
                        column: x => x.class_room_id,
                        principalTable: "class_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_timetable_versions_timetable_version_id",
                        column: x => x.timetable_version_id,
                        principalTable: "timetable_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timetable_entries_users_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_curriculum_plans_year_grade",
                table: "curriculum_plans",
                columns: new[] { "academic_year_id", "grade_level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_curriculum_subject_quotas_subject_id",
                table: "curriculum_subject_quotas",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ux_curriculum_subject_quotas_plan_subject",
                table: "curriculum_subject_quotas",
                columns: new[] { "curriculum_plan_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_homeroom_assignments_active_class",
                table: "homeroom_assignments",
                column: "class_room_id",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ux_homeroom_assignments_active_teacher",
                table: "homeroom_assignments",
                column: "teacher_id",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_student_profile_change_requests_requester_user_id",
                table: "student_profile_change_requests",
                column: "requester_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_profile_change_requests_reviewer_user_id",
                table: "student_profile_change_requests",
                column: "reviewer_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_student_profile_change_requests_pending_student",
                table: "student_profile_change_requests",
                column: "student_id",
                unique: true,
                filter: "status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_subject_capabilities_subject_id",
                table: "teacher_subject_capabilities",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ux_teacher_subject_capabilities_teacher_subject",
                table: "teacher_subject_capabilities",
                columns: new[] { "teacher_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_class_room_id",
                table: "timetable_entries",
                column: "class_room_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_subject_id",
                table: "timetable_entries",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_teacher_id",
                table: "timetable_entries",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ux_timetable_entries_class_slot",
                table: "timetable_entries",
                columns: new[] { "timetable_version_id", "class_room_id", "cycle_week", "day_of_week", "period_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_timetable_entries_teacher_slot",
                table: "timetable_entries",
                columns: new[] { "timetable_version_id", "teacher_id", "cycle_week", "day_of_week", "period_number" },
                unique: true,
                filter: "teacher_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_timetable_versions_semester_status",
                table: "timetable_versions",
                columns: new[] { "semester_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "curriculum_subject_quotas");

            migrationBuilder.DropTable(
                name: "homeroom_assignments");

            migrationBuilder.DropTable(
                name: "student_profile_change_requests");

            migrationBuilder.DropTable(
                name: "teacher_subject_capabilities");

            migrationBuilder.DropTable(
                name: "timetable_entries");

            migrationBuilder.DropTable(
                name: "curriculum_plans");

            migrationBuilder.DropTable(
                name: "timetable_versions");

            migrationBuilder.DropColumn(
                name: "address",
                table: "students");

            migrationBuilder.DropColumn(
                name: "gender",
                table: "students");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "students");
        }
    }
}
