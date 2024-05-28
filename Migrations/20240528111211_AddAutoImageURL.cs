using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giat_xay_server.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoImageURL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "identity",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "identity",
                table: "LaundryServices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "identity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "identity",
                table: "LaundryServices");
        }
    }
}
