using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4QueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_CategoryId",
                table: "Recipes");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowedUserId_CreatedAt",
                table: "UserFollows",
                columns: new[] { "FollowedUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowerUserId_CreatedAt",
                table: "UserFollows",
                columns: new[] { "FollowerUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CategoryId_CreatedAt",
                table: "Recipes",
                columns: new[] { "CategoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_RegionId_CreatedAt",
                table: "Recipes",
                columns: new[] { "RegionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteRecipes_UserId_CreatedAt",
                table: "FavoriteRecipes",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFollows_FollowedUserId_CreatedAt",
                table: "UserFollows");

            migrationBuilder.DropIndex(
                name: "IX_UserFollows_FollowerUserId_CreatedAt",
                table: "UserFollows");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_CategoryId_CreatedAt",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_RegionId_CreatedAt",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteRecipes_UserId_CreatedAt",
                table: "FavoriteRecipes");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CategoryId",
                table: "Recipes",
                column: "CategoryId");
        }
    }
}
