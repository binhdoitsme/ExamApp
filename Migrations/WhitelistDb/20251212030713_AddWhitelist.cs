using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamApp.Migrations.WhitelistDb
{
    /// <inheritdoc />
    public partial class AddWhitelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "whitelisted_users",
                columns: table => new
                {
                    email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_whitelisted_users", x => x.email);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "whitelisted_users");
        }
    }
}
