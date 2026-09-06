using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeMediaGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeMedia_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeMedia_RecipeId",
                table: "RecipeMedia",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeMedia_RecipeId_SortOrder",
                table: "RecipeMedia",
                columns: new[] { "RecipeId", "SortOrder" });

            migrationBuilder.Sql("""
                INSERT INTO [RecipeMedia] ([Id], [RecipeId], [Url], [MediaType], [ContentType], [IsMain], [SortOrder], [CreatedAt])
                SELECT NEWID(), [Id], [ImageUrl], 1,
                    CASE LOWER(RIGHT([ImageUrl], CHARINDEX('.', REVERSE([ImageUrl]) + '.') - 1))
                        WHEN 'png' THEN 'image/png' WHEN 'webp' THEN 'image/webp' ELSE 'image/jpeg' END,
                    1, 0, SYSUTCDATETIME()
                FROM [Recipes] WHERE [ImageUrl] IS NOT NULL AND LTRIM(RTRIM([ImageUrl])) <> '';
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[RecipeImage]', N'U') IS NOT NULL
                BEGIN
                    INSERT INTO [RecipeMedia] ([Id], [RecipeId], [Url], [MediaType], [ContentType], [IsMain], [SortOrder], [CreatedAt])
                    SELECT NEWID(), COALESCE([RecipieId], [RecipeId]), [Url], 1, 'image/jpeg',
                        CASE WHEN [IsMain] = 1 THEN 1 ELSE 0 END, 0, COALESCE([CreatedAt], SYSUTCDATETIME())
                    FROM [RecipeImage] WHERE COALESCE([RecipieId], [RecipeId]) IS NOT NULL AND [Url] IS NOT NULL;
                    DROP TABLE [RecipeImage];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeMedia");

            migrationBuilder.CreateTable(
                name: "RecipeImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipieId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeImage_Recipes_RecipieId",
                        column: x => x.RecipieId,
                        principalTable: "Recipes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImage_RecipieId",
                table: "RecipeImage",
                column: "RecipieId");
        }
    }
}
