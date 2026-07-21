using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations;

/// <summary>
/// Ghi chú: NormalizeLegacyAfternoonPeriods chuẩn hóa số tiết chiều legacy về phạm vi 1..5 và khóa invariant tại PostgreSQL.
/// </summary>
public partial class NormalizeLegacyAfternoonPeriods : Migration
{
    /// <summary>
    /// Ghi chú: Up đổi tiết chiều legacy 6..10 thành 1..5 trước khi thêm check constraint.
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE timetable_entries
            SET period_number = period_number - 5
            WHERE session = 'Afternoon'
              AND period_number BETWEEN 6 AND 10;
            """);

        migrationBuilder.AddCheckConstraint(
            name: "ck_timetable_entries_period_number",
            table: "timetable_entries",
            sql: "period_number BETWEEN 1 AND 5");
    }

    /// <summary>
    /// Ghi chú: Down chỉ gỡ constraint; dữ liệu đã chuẩn hóa không đổi ngược để tránh làm sai các tiết chiều mới.
    /// </summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_timetable_entries_period_number",
            table: "timetable_entries");
    }
}
