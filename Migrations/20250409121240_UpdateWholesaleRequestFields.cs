using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cansaraciye_ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWholesaleRequestFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "WholesaleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "WholesaleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "WholesaleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "WholesaleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "WholesaleRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "WholesaleRequests");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "WholesaleRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "WholesaleRequests");
        }
    }
}
