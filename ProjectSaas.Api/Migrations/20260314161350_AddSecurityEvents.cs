using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectSaas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "security_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    organisation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    family_id = table.Column<Guid>(type: "uuid", nullable: true),
                    request_ip_address = table.Column<string>(type: "text", nullable: true),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    metadata_json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_security_events_event_type_occurred_at_utc",
                table: "security_events",
                columns: new[] { "event_type", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_security_events_occurred_at_utc",
                table: "security_events",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_security_events_organisation_id_occurred_at_utc",
                table: "security_events",
                columns: new[] { "organisation_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_security_events_user_id_occurred_at_utc",
                table: "security_events",
                columns: new[] { "user_id", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "security_events");
        }
    }
}
