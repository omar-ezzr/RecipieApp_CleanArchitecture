using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCuisineAndRegionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CuisineId",
                table: "Recipes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTraditional",
                table: "Recipes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginDescription",
                table: "Recipes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RegionId",
                table: "Recipes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServingOccasion",
                table: "Recipes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraditionalName",
                table: "Recipes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cuisines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuisines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CuisineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Cuisines_CuisineId",
                        column: x => x.CuisineId,
                        principalTable: "Cuisines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Cuisines] WHERE [Slug] = N'international')
                BEGIN
                    INSERT INTO [Cuisines] ([Id], [Name], [Slug], [Description], [CountryCode], [ImageUrl], [IsActive], [CreatedAt])
                    VALUES ('22222222-2222-2222-2222-222222222222', N'International', N'international', N'Migration-safe default cuisine for existing recipes without a specific cultural origin.', N'XX', NULL, CAST(1 AS bit), SYSUTCDATETIME())
                END
                """);

            migrationBuilder.Sql("""
                DECLARE @InternationalCuisineId uniqueidentifier;
                SELECT TOP(1) @InternationalCuisineId = [Id]
                FROM [Cuisines]
                WHERE [Slug] = N'international'
                ORDER BY [CreatedAt], [Id];

                UPDATE [Recipes]
                SET [CuisineId] = @InternationalCuisineId
                WHERE [CuisineId] IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "CuisineId",
                table: "Recipes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CuisineId",
                table: "Recipes",
                column: "CuisineId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CuisineId_CreatedAt",
                table: "Recipes",
                columns: new[] { "CuisineId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CuisineId_RegionId_CreatedAt",
                table: "Recipes",
                columns: new[] { "CuisineId", "RegionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_RegionId",
                table: "Recipes",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuisines_Slug",
                table: "Cuisines",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CuisineId",
                table: "Regions",
                column: "CuisineId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CuisineId_Slug",
                table: "Regions",
                columns: new[] { "CuisineId", "Slug" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Cuisines_CuisineId",
                table: "Recipes",
                column: "CuisineId",
                principalTable: "Cuisines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Regions_RegionId",
                table: "Recipes",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Cuisines_CuisineId",
                table: "Recipes");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Regions_RegionId",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Cuisines");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_CuisineId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_CuisineId_CreatedAt",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_CuisineId_RegionId_CreatedAt",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_RegionId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CuisineId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "IsTraditional",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "OriginDescription",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ServingOccasion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "TraditionalName",
                table: "Recipes");
        }
    }
}
