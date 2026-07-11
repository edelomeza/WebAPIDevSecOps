using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIDevSecOps.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenBlacklistTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpEmpleado",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    strNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    strAPaterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    strAMaterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    strCURP = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    idEmpCatTipoEmpleado = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpEmpleado", x => x.id);
                    table.ForeignKey(
                        name: "FK_EmpEmpleado_EmpCatTipoEmpleado_idEmpCatTipoEmpleado",
                        column: x => x.idEmpCatTipoEmpleado,
                        principalTable: "EmpCatTipoEmpleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SegTokenBlacklist",
                columns: table => new
                {
                    Jti = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiryUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegTokenBlacklist", x => x.Jti);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpEmpleado_idEmpCatTipoEmpleado",
                table: "EmpEmpleado",
                column: "idEmpCatTipoEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_EmpEmpleado_strCURP",
                table: "EmpEmpleado",
                column: "strCURP",
                unique: true,
                filter: "[strCURP] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpEmpleado");

            migrationBuilder.DropTable(
                name: "SegTokenBlacklist");
        }
    }
}
