using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroServis.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalTokenToPendingRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalToken",
                table: "PendingRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiresAt",
                table: "PendingRegistrations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalToken",
                table: "PendingRegistrations");

            migrationBuilder.DropColumn(
                name: "TokenExpiresAt",
                table: "PendingRegistrations");
        }
    }
}
