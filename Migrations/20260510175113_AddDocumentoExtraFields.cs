using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prueba3._0.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "HistorialVersiones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Documentos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notas",
                table: "HistorialVersiones");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Documentos");
        }
    }
}
