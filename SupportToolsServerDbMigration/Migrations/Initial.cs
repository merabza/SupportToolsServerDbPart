#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace SupportToolsServerDbMigration.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("ApiKeysByRemoteIpAddresses",
            table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                ApiKey = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                RemoteIpAddress = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false)
            }, constraints: table => { table.PrimaryKey("PK_ApiKeysByRemoteIpAddresses", x => x.Id); });

        migrationBuilder.CreateTable("GitIgnoreFileTypes",
            table => new
            {
                Id = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                Content = table.Column<string>("nvarchar(max)", maxLength: 16384, nullable: false)
            }, constraints: table => { table.PrimaryKey("PK_GitIgnoreFileTypes", x => x.Id); });

        migrationBuilder.CreateTable("GitData",
            table => new
            {
                GdId = table.Column<int>("int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                GdGitAddress = table.Column<string>("nvarchar(128)", maxLength: 128, nullable: false),
                GdName = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: false),
                GdFolderName = table.Column<string>("nvarchar(max)", nullable: false),
                GitIgnoreFileTypeId = table.Column<int>("int", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_GitData", x => x.GdId);
                table.ForeignKey("FK_GitData_GitIgnoreFileTypes_GitIgnoreFileTypeId", x => x.GitIgnoreFileTypeId,
                    "GitIgnoreFileTypes", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_ApiKeysByRemoteIpAddresses_ApiKey_RemoteIpAddress",
            "ApiKeysByRemoteIpAddresses", new[] { "ApiKey", "RemoteIpAddress" }, unique: true);

        migrationBuilder.CreateIndex("IX_GitData_GdName", "GitData", "GdName", unique: true);

        migrationBuilder.CreateIndex("IX_GitData_GitIgnoreFileTypeId", "GitData", "GitIgnoreFileTypeId");

        migrationBuilder.CreateIndex("IX_GitIgnoreFileTypes_Name", "GitIgnoreFileTypes", "Name", unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ApiKeysByRemoteIpAddresses");

        migrationBuilder.DropTable("GitData");

        migrationBuilder.DropTable("GitIgnoreFileTypes");
    }
}