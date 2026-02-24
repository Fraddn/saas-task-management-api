using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectSaas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_OrganisationId_AssignedToUserId_IsDeleted",
                table: "tickets",
                columns: new[] { "OrganisationId", "AssignedToUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_OrganisationId_CreatedByUserId_IsDeleted",
                table: "tickets",
                columns: new[] { "OrganisationId", "CreatedByUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_OrganisationId_IsDeleted",
                table: "tickets",
                columns: new[] { "OrganisationId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_OrganisationId_Status_IsDeleted",
                table: "tickets",
                columns: new[] { "OrganisationId", "Status", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
