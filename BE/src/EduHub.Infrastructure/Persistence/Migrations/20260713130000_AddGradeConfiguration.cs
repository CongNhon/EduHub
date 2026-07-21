using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Ghi chú: AddGradeConfiguration tạo bảng grade_components cho cấu hình thành phần điểm.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260713130000_AddGradeConfiguration")]
    public partial class AddGradeConfiguration : Migration
    {
        /// <summary>
        /// Ghi chú: Up tạo schema grade_components với unique name/order theo subject-semester-version.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grade_components",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    semester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    max_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    include_in_gpa = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_components", x => x.id);
                    table.CheckConstraint("ck_grade_components_display_order_positive", "display_order > 0");
                    table.CheckConstraint("ck_grade_components_max_score_positive", "max_score > 0");
                    table.CheckConstraint("ck_grade_components_version_positive", "version > 0");
                    table.CheckConstraint("ck_grade_components_weight_range", "weight > 0 AND weight <= 1");
                    table.ForeignKey(
                        name: "FK_grade_components_semesters_semester_id",
                        column: x => x.semester_id,
                        principalTable: "semesters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grade_components_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_grade_components_subject_semester",
                table: "grade_components",
                columns: new[] { "subject_id", "semester_id" });

            migrationBuilder.CreateIndex(
                name: "ix_grade_components_semester_id",
                table: "grade_components",
                column: "semester_id");

            migrationBuilder.CreateIndex(
                name: "ux_grade_components_version_name",
                table: "grade_components",
                columns: new[] { "subject_id", "semester_id", "version", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_grade_components_version_order",
                table: "grade_components",
                columns: new[] { "subject_id", "semester_id", "version", "display_order" },
                unique: true);
        }

        /// <summary>
        /// Ghi chú: Down xóa schema grade_components.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "grade_components");
        }
    }
}
