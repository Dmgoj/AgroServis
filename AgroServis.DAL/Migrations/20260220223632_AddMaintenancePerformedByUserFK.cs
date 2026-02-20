using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroServis.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenancePerformedByUserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "MaintenanceRecords",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_PerformedBy",
                table: "MaintenanceRecords",
                column: "PerformedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_AspNetUsers_PerformedBy",
                table: "MaintenanceRecords",
                column: "PerformedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_AspNetUsers_PerformedBy",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_PerformedBy",
                table: "MaintenanceRecords");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedBy",
                table: "MaintenanceRecords",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
