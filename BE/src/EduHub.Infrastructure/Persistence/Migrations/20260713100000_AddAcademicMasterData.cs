using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Ghi chú: AddAcademicMasterData đại diện cho add academic master data trong hệ thống EduHub.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260713100000_AddAcademicMasterData")]
    public partial class AddAcademicMasterData : Migration
    {
        /// <summary>
        /// Ghi chú: Up thực hiện phần xử lý của add academic master data.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_years",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_years", x => x.id);
                    table.CheckConstraint("ck_academic_years_date_range", "start_date < end_date");
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    normalized_subject_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.id);
                    table.CheckConstraint("ck_subjects_credits_positive", "credits > 0");
                    table.CheckConstraint("ck_subjects_max_score_positive", "max_score > 0");
                });

            migrationBuilder.CreateTable(
                name: "semesters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    grade_entry_from = table.Column<DateOnly>(type: "date", nullable: false),
                    grade_entry_to = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semesters", x => x.id);
                    table.CheckConstraint("ck_semesters_date_range", "start_date < end_date");
                    table.CheckConstraint("ck_semesters_grade_entry_range", "grade_entry_from <= grade_entry_to");
                    table.ForeignKey(
                        name: "FK_semesters_academic_years_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "academic_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_academic_years_normalized_name",
                table: "academic_years",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_subjects_normalized_subject_code",
                table: "subjects",
                column: "normalized_subject_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_semesters_academic_year_id",
                table: "semesters",
                column: "academic_year_id");

            migrationBuilder.CreateIndex(
                name: "ux_semesters_academic_year_id_normalized_name",
                table: "semesters",
                columns: new[] { "academic_year_id", "normalized_name" },
                unique: true);
        }

        /// <summary>
        /// Ghi chú: Down thực hiện phần xử lý của add academic master data.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "semesters");
            migrationBuilder.DropTable(name: "subjects");
            migrationBuilder.DropTable(name: "academic_years");
        }
    }
}
