using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Migrations
{
    /// <inheritdoc />
    public partial class Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "Usuarios");

            migrationBuilder.Sql("UPDATE Usuarios SET Role = 'Usuarios' WHERE Role IS NULL OR Role <> 'admin'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Usuarios");
        }
    }
}
