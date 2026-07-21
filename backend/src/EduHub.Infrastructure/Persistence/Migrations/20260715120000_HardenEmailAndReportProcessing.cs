using System;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations;

/// <summary>
/// Ghi chu: HardenEmailAndReportProcessing them retry state cho email va unique constraint cho report request dang mo.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260715120000_HardenEmailAndReportProcessing")]
public partial class HardenEmailAndReportProcessing : Migration
{
    /// <summary>
    /// Ghi chu: Up nang schema email delivery va report request theo business invariant moi.
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_report_requests_owner_scope_status",
            table: "report_requests");

        migrationBuilder.AlterColumn<DateTime>(
            name: "sent_at_utc",
            table: "email_digest_deliveries",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AddColumn<int>(
            name: "attempt_count",
            table: "email_digest_deliveries",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<DateTime>(
            name: "last_attempt_at_utc",
            table: "email_digest_deliveries",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "last_error",
            table: "email_digest_deliveries",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "status",
            table: "email_digest_deliveries",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "Sent");

        migrationBuilder.CreateIndex(
            name: "ux_report_requests_open_owner_scope",
            table: "report_requests",
            columns: new[] { "requester_user_id", "student_id", "semester_id" },
            unique: true,
            filter: "status IN ('Pending', 'Approved', 'Generating')");
    }

    /// <summary>
    /// Ghi chu: Down khoi phuc schema email delivery va index report request truoc migration.
    /// </summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ux_report_requests_open_owner_scope",
            table: "report_requests");

        migrationBuilder.Sql("UPDATE email_digest_deliveries SET sent_at_utc = COALESCE(sent_at_utc, last_attempt_at_utc, NOW()) WHERE sent_at_utc IS NULL;");

        migrationBuilder.DropColumn(name: "attempt_count", table: "email_digest_deliveries");
        migrationBuilder.DropColumn(name: "last_attempt_at_utc", table: "email_digest_deliveries");
        migrationBuilder.DropColumn(name: "last_error", table: "email_digest_deliveries");
        migrationBuilder.DropColumn(name: "status", table: "email_digest_deliveries");

        migrationBuilder.AlterColumn<DateTime>(
            name: "sent_at_utc",
            table: "email_digest_deliveries",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_report_requests_owner_scope_status",
            table: "report_requests",
            columns: new[] { "requester_user_id", "student_id", "semester_id", "status" });
    }
}
