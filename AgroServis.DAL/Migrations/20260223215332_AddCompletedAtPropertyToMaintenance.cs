using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroServis.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedAtPropertyToMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "MaintenanceRecords",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "MaintenanceRecords");
        }
    }
}
