using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Argus.Operations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefoneEContatoEmergenciaToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NOME_EMERGENCIA",
                table: "USUARIO",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RELACAO_EMERGENCIA",
                table: "USUARIO",
                type: "NVARCHAR2(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TELEFONE",
                table: "USUARIO",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TELEFONE_EMERGENCIA",
                table: "USUARIO",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NOME_EMERGENCIA",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "RELACAO_EMERGENCIA",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "TELEFONE",
                table: "USUARIO");

            migrationBuilder.DropColumn(
                name: "TELEFONE_EMERGENCIA",
                table: "USUARIO");
        }
    }
}
