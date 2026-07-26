using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeOwnershipAndUserDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var systemUserId = new Guid("11111111-1111-1111-1111-111111111111");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Users]
                SET [DisplayName] = LEFT(
                    CASE
                        WHEN CHARINDEX('@', [Email]) > 1 THEN LEFT([Email], CHARINDEX('@', [Email]) - 1)
                        ELSE [Email]
                    END,
                    100)
                WHERE [DisplayName] IS NULL OR LTRIM(RTRIM([DisplayName])) = ''
                """);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.Sql($"""
                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = '{systemUserId}')
                BEGIN
                    INSERT INTO [Users] ([Id], [DisplayName], [Email], [PasswordHash], [Role], [IsActive], [RefreshToken], [RefreshTokenExpiryTime])
                    VALUES ('{systemUserId}', 'Recepie System', 'system@recepie.local', 'SYSTEM_ACCOUNT_NO_LOGIN', 'User', CAST(0 AS bit), NULL, NULL)
                END
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Recipes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql($"""
                UPDATE [Recipes]
                SET [UserId] = '{systemUserId}'
                WHERE [UserId] IS NULL
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Recipes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Recipes",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Quantity",
                table: "Ingredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_UserId",
                table: "Recipes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_UserId_CreatedAt",
                table: "Recipes",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Users_UserId",
                table: "Recipes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var systemUserId = new Guid("11111111-1111-1111-1111-111111111111");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Users_UserId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_UserId_CreatedAt",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_UserId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.Sql($"""
                DELETE FROM [Users]
                WHERE [Id] = '{systemUserId}'
                    AND [Email] = 'system@recepie.local'
                    AND NOT EXISTS (
                        SELECT 1 FROM [FavoriteRecipes] WHERE [UserId] = '{systemUserId}'
                        UNION ALL
                        SELECT 1 FROM [RecipeReviews] WHERE [UserId] = '{systemUserId}'
                    )
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Quantity",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
