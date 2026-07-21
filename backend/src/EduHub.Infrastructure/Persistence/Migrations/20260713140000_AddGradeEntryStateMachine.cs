using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeEntryStateMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grade_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    component_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    publication_version = table.Column<int>(type: "integer", nullable: false),
                    submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reopened_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_entries", x => x.id);
                    table.CheckConstraint("ck_grade_entries_publication_version_non_negative", "publication_version >= 0");
                    table.CheckConstraint("ck_grade_entries_score_range", "score >= 0");
                    table.CheckConstraint("ck_grade_entries_version_positive", "version > 0");
                    table.ForeignKey(
                        name: "FK_grade_entries_grade_components_component_id",
                        column: x => x.component_id,
                        principalTable: "grade_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grade_entries_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grade_entries_teaching_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "teaching_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_change_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    new_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    changed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_change_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_grade_change_histories_grade_entries_grade_entry_id",
                        column: x => x.grade_entry_id,
                        principalTable: "grade_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grade_change_histories_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_grade_change_histories_changed_by_user_id",
                table: "grade_change_histories",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_grade_change_histories_grade_entry_id",
                table: "grade_change_histories",
                column: "grade_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_grade_entries_assignment_status",
                table: "grade_entries",
                columns: new[] { "assignment_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_grade_entries_component_id",
                table: "grade_entries",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "ux_grade_entries_student_assignment_component",
                table: "grade_entries",
                columns: new[] { "student_id", "assignment_id", "component_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_occurred",
                table: "outbox_messages",
                columns: new[] { "processed_at_utc", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "grade_change_histories");
            migrationBuilder.DropTable(name: "outbox_messages");
            migrationBuilder.DropTable(name: "grade_entries");
        }
    }
}
