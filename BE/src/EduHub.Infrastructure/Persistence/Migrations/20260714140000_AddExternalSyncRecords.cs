using System;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260714140000_AddExternalSyncRecords")]
    public partial class AddExternalSyncRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_sync_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    external_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    last_error = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    next_retry_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    succeeded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_manual_retry_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_manual_retry_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_sync_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_external_sync_records_users_last_manual_retry_by_user_id",
                        column: x => x.last_manual_retry_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_external_sync_records_last_manual_retry_by_user_id",
                table: "external_sync_records",
                column: "last_manual_retry_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_sync_records_status_next_retry",
                table: "external_sync_records",
                columns: new[] { "status", "next_retry_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ux_external_sync_records_aggregate_version",
                table: "external_sync_records",
                columns: new[] { "aggregate_id", "version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "external_sync_records");
        }
    }
}
