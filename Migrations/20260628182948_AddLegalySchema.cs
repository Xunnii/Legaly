using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Legaly.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    NomorSurat = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NamaPenjual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NikPenjual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatPenjual = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NamaPembeli = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NikPembeli = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlamatPembeli = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerkMotor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipeMotor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TahunMotor = table.Column<int>(type: "int", nullable: false),
                    WarnaMotor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorPolisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorBPKB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NomorSTNK = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TanggalPerjanjian = table.Column<DateOnly>(type: "date", nullable: false),
                    GeneratedContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_NomorSurat",
                table: "Documents",
                column: "NomorSurat",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TemplateId",
                table: "Documents",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Templates");
        }
    }
}
