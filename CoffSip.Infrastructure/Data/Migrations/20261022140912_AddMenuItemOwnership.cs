using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffSip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItemOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "MenuItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_OwnerId",
                table: "MenuItems",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_AspNetUsers_OwnerId",
                table: "MenuItems",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_AspNetUsers_OwnerId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_OwnerId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "MenuItems");
        }
    }
}
