using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giat_xay_server.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "identity",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "identity",
                table: "Orders");
        }
    }
}
