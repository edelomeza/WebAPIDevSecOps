using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIDevSecOps.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La tabla SegUsuario se crea directamente
            // con índice único en strNombre

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SegUsuario')
                BEGIN
                    CREATE TABLE [SegUsuario] (
                        [id] int NOT NULL IDENTITY,
                        [strNombre] nvarchar(50) NOT NULL,
                        [strPWD] nvarchar(200) NOT NULL,
                        [strCorreoElectronico] nvarchar(50) NOT NULL,
                        [dteFechaRegistro] datetime2 NULL,
                        [RowVersion] rowversion NOT NULL,
                        CONSTRAINT [PK_SegUsuario] PRIMARY KEY ([id])
                    );
                END");

            // Crear índice único en strNombre si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SegUsuario_strNombre')
                BEGIN
                    CREATE UNIQUE INDEX [IX_SegUsuario_strNombre] ON [SegUsuario] ([strNombre]);
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No es necesario revertir en producción, se deja como no operación
        }
    }
}
