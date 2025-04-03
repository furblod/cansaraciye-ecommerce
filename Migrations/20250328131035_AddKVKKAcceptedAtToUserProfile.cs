using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cansaraciye_ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddKVKKAcceptedAtToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "KVKKAcceptedAt",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KVKKAcceptedAt",
                table: "UserProfiles");
        }
    }
}
