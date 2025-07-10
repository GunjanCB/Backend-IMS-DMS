using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentManagementBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableUserIdToCapturedImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadDate",
                table: "CapturedImages");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "CapturedImages",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapturedImages_UserId",
                table: "CapturedImages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CapturedImages_Users_UserId",
                table: "CapturedImages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapturedImages_Users_UserId",
                table: "CapturedImages");

            migrationBuilder.DropIndex(
                name: "IX_CapturedImages_UserId",
                table: "CapturedImages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CapturedImages");

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadDate",
                table: "CapturedImages",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
