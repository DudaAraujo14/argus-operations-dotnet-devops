using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus.Operations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrigadistaIdToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ID_BRIGADISTA",
                table: "USUARIO",
                type: "NUMBER(19)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_USUARIO_ID_BRIGADISTA",
                table: "USUARIO",
                column: "ID_BRIGADISTA");

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_BRIGADISTA_ID_BRIGADISTA",
                table: "USUARIO",
                column: "ID_BRIGADISTA",
                principalTable: "BRIGADISTA",
                principalColumn: "ID_BRIGADISTA",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USUARIO_BRIGADISTA_ID_BRIGADISTA",
                table: "USUARIO");

            migrationBuilder.DropIndex(
                name: "IX_USUARIO_ID_BRIGADISTA",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "ID_BRIGADISTA",
                table: "USUARIO");
        }
    }
}
