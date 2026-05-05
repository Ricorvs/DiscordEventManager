using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPinDateThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PinDateThreshold",
                table: "GuildConfiguration",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PinDateThreshold",
                table: "GuildConfiguration");
        }
    }
}
