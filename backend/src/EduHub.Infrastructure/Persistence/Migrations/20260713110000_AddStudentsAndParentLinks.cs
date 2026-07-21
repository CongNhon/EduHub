using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Ghi chú: AddStudentsAndParentLinks đại diện cho add students and parent links trong hệ thống EduHub.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260713110000_AddStudentsAndParentLinks")]
    public partial class AddStudentsAndParentLinks : Migration
    {
        /// <summary>
        /// Ghi chú: Up thực hiện phần xử lý của add students and parent links.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    normalized_student_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    full_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parent_students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    effective_from_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deactivated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parent_students", x => x.id);
                    table.ForeignKey(
                        name: "FK_parent_students_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_parent_students_users_parent_user_id",
                        column: x => x.parent_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_students_normalized_student_code",
                table: "students",
                column: "normalized_student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parent_students_student_id_is_active",
                table: "parent_students",
                columns: new[] { "student_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ux_parent_students_parent_user_id_student_id",
                table: "parent_students",
                columns: new[] { "parent_user_id", "student_id" },
                unique: true);
        }

        /// <summary>
        /// Ghi chú: Down thực hiện phần xử lý của add students and parent links.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "parent_students");
            migrationBuilder.DropTable(name: "students");
        }
    }
}
