using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations;

/// <summary>
/// Ghi chú: ReplaceCycleWeeksWithTeachingWeeks thay chu kỳ A/B bằng số tuần thực tế của học kỳ.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260715153000_ReplaceCycleWeeksWithTeachingWeeks")]
public partial class ReplaceCycleWeeksWithTeachingWeeks : Migration
{
    /// <summary>
    /// Ghi chú: Up giữ dữ liệu lịch hiện có, đổi cột tuần, mở rộng khóa slot và siết một phân công active cho mỗi lớp-môn-học kỳ.
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ux_timetable_entries_class_slot",
            table: "timetable_entries");

        migrationBuilder.DropIndex(
            name: "ux_timetable_entries_teacher_slot",
            table: "timetable_entries");

        migrationBuilder.DropIndex(
            name: "ux_teaching_assignments_active_scope",
            table: "teaching_assignments");

        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM teaching_assignments
                    WHERE is_active = true
                    GROUP BY semester_id, class_room_id, subject_id
                    HAVING COUNT(*) > 1
                ) THEN
                    RAISE EXCEPTION 'Duplicate active teaching assignments must be reviewed before applying this migration.';
                END IF;
            END $$;
            """);

        migrationBuilder.DropColumn(
            name: "week_a_periods",
            table: "curriculum_subject_quotas");

        migrationBuilder.DropColumn(
            name: "week_b_periods",
            table: "curriculum_subject_quotas");

        migrationBuilder.RenameColumn(
            name: "cycle_week",
            table: "timetable_entries",
            newName: "week_number");

        migrationBuilder.CreateIndex(
            name: "ux_timetable_entries_class_slot",
            table: "timetable_entries",
            columns: new[] { "timetable_version_id", "class_room_id", "week_number", "day_of_week", "session", "period_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_timetable_entries_teacher_slot",
            table: "timetable_entries",
            columns: new[] { "timetable_version_id", "teacher_id", "week_number", "day_of_week", "session", "period_number" },
            unique: true,
            filter: "teacher_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ux_teaching_assignments_active_scope",
            table: "teaching_assignments",
            columns: new[] { "semester_id", "class_room_id", "subject_id" },
            unique: true,
            filter: "is_active = true");
    }

    /// <summary>
    /// Ghi chú: Down khôi phục schema chu kỳ A/B trước thay đổi.
    /// </summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ux_timetable_entries_class_slot",
            table: "timetable_entries");

        migrationBuilder.DropIndex(
            name: "ux_timetable_entries_teacher_slot",
            table: "timetable_entries");

        migrationBuilder.DropIndex(
            name: "ux_teaching_assignments_active_scope",
            table: "teaching_assignments");

        migrationBuilder.RenameColumn(
            name: "week_number",
            table: "timetable_entries",
            newName: "cycle_week");

        migrationBuilder.AddColumn<int>(
            name: "week_a_periods",
            table: "curriculum_subject_quotas",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "week_b_periods",
            table: "curriculum_subject_quotas",
            type: "integer",
            nullable: false,
            defaultValue: 0);

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
            name: "ux_teaching_assignments_active_scope",
            table: "teaching_assignments",
            columns: new[] { "semester_id", "class_room_id", "subject_id", "teacher_id" },
            unique: true,
            filter: "is_active = true");
    }
}
