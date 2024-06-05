using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace giat_xay_server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLaundry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConditionValue",
                schema: "identity",
                table: "LaundryServiceTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionValue",
                schema: "identity",
                table: "LaundryServiceTypes");
        }
    }
}
