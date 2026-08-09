using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Core.Application.DTO.Auth;
using Core.Application.DTO.Cuisines;
using Core.Application.DTO.Favorites;
using Core.Application.DTO.Recipe;
using Core.Application.DTO.Regions;
using Core.Application.DTO.Reviews;
using Core.Domain.Constants;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Recep.IntegrationTests;

public sealed class AuthBehaviorTests : IClassFixture<RecepApiFactory>
{
    private readonly RecepApiFactory _factory;

    public AuthBehaviorTests(RecepApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Duplicate_registration_is_rejected()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";
        var client = _factory.CreateClient();

        var first = await client.PostAsJsonAsync("/api/Auth/register", new RegisterDto { DisplayName = "Duplicate", Email = email, Password = RecepApiFactory.KnownPassword });
        var second = await client.PostAsJsonAsync("/api/Auth/register", new RegisterDto { DisplayName = "Duplicate", Email = email.ToUpperInvariant(), Password = RecepApiFactory.KnownPassword });

        first.EnsureSuccessStatusCode();
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registration_persists_trimmed_display_name()
    {
        var email = $"display-{Guid.NewGuid():N}@example.com";
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/Auth/register", new RegisterDto
        {
            DisplayName = "  Public Cook  ",
            Email = email,
            Password = RecepApiFactory.KnownPassword
        });

        response.EnsureSuccessStatusCode();
        var user = await _factory.FindUserByEmailAsync(email);
        user!.DisplayName.Should().Be("Public Cook");
    }

    [Fact]
    public async Task Login_and_refresh_token_behavior_is_correct()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await _factory.CreateUserAsync(email, AppRoles.User);
        var client = _factory.CreateClient();

        var invalidLogin = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto { Email = email, Password = "wrong" });
        var login = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto { Email = email, Password = RecepApiFactory.KnownPassword });
        var tokens = await login.Content.ReadFromJsonAsync<TokenResponse>();
        var invalidRefresh = await client.PostAsJsonAsync("/api/Auth/refresh", new TokenRequestDto { RefreshToken = "invalid" });
        var refresh = await client.PostAsJsonAsync("/api/Auth/refresh", new TokenRequestDto { RefreshToken = tokens!.RefreshToken });
        var rotated = await refresh.Content.ReadFromJsonAsync<TokenResponse>();

        invalidLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        login.EnsureSuccessStatusCode();
        invalidRefresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        refresh.EnsureSuccessStatusCode();
        rotated!.AccessToken.Should().NotBeNullOrWhiteSpace();
        rotated.RefreshToken.Should().NotBe(tokens.RefreshToken);

        var principal = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        principal.Claims.Should().Contain(claim => claim.Type == ClaimTypes.NameIdentifier);
        principal.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Name && claim.Value == email);
        principal.Claims.Should().Contain(claim => claim.Type == ClaimTypes.Role && claim.Value == AppRoles.User);
    }
}

public sealed class RecipeOwnershipBehaviorTests : IClassFixture<RecepApiFactory>
{
    private readonly RecepApiFactory _factory;

    public RecipeOwnershipBehaviorTests(RecepApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Recipe_ownership_authorization_and_safe_author_fields_are_enforced()
    {
        var ownerId = await _factory.CreateUserAsync($"owner-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var otherId = await _factory.CreateUserAsync($"other-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var owner = _factory.CreateClientForUser(ownerId, AppRoles.User);
        var other = _factory.CreateClientForUser(otherId, AppRoles.User);
        var admin = _factory.CreateClientForUser(_factory.AdminId, AppRoles.Admin);
        var anonymous = _factory.CreateClient();

        (await anonymous.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest(DifficultyLevel.Easy))).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        var create = await owner.PostAsJsonAsync("/api/Recipes", new
        {
            title = "Owned soup",
            description = "Warm",
            preparationTimeMinutes = 20,
            categoryId = _factory.CategoryId,
            cuisineId = _factory.CuisineId,
            difficulty = DifficultyLevel.Easy,
            userId = otherId,
            ingredients = new[] { new { name = "Salt", quantity = "1 tsp" } },
            steps = new[] { new { stepNumber = 1, instruction = "Cook" } }
        });
        create.EnsureSuccessStatusCode();
        var recipe = await _factory.GetNewestRecipeAsync();
        recipe!.UserId.Should().Be(ownerId);

        (await owner.PutAsJsonAsync($"/api/Recipes/{recipe.Id}", _factory.CreateRecipeRequest(DifficultyLevel.Medium))).EnsureSuccessStatusCode();
        (await other.PutAsJsonAsync($"/api/Recipes/{recipe.Id}", _factory.CreateRecipeRequest(DifficultyLevel.Hard))).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
        (await admin.PutAsJsonAsync($"/api/Recipes/{recipe.Id}", _factory.CreateRecipeRequest(DifficultyLevel.Hard))).EnsureSuccessStatusCode();

        var missing = Guid.NewGuid();
        (await other.PutAsJsonAsync($"/api/Recipes/{missing}", _factory.CreateRecipeRequest(DifficultyLevel.Easy))).StatusCode
            .Should().Be(HttpStatusCode.NotFound);

        var details = await owner.GetFromJsonAsync<RecipieDto>($"/api/Recipes/{recipe.Id}");
        var listRaw = await owner.GetStringAsync("/api/Recipes/paged");
        details.Should().NotBeNull();
        details!.Author.Id.Should().Be(ownerId);
        details.Author.DisplayName.Should().NotContain("@");
        listRaw.Should().Contain("author");
        listRaw.Should().NotContain("passwordHash");
        listRaw.Should().NotContain("refreshToken");

        (await anonymous.DeleteAsync($"/api/Recipes/{recipe.Id}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await other.DeleteAsync($"/api/Recipes/{recipe.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await admin.DeleteAsync($"/api/Recipes/{recipe.Id}")).EnsureSuccessStatusCode();

        var ownerDeletedId = await _factory.CreateRecipeAsync(ownerId, DifficultyLevel.Easy);
        (await owner.DeleteAsync($"/api/Recipes/{ownerDeletedId}")).EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(DifficultyLevel.Easy)]
    [InlineData(DifficultyLevel.Medium)]
    [InlineData(DifficultyLevel.Hard)]
    public async Task Supported_difficulty_creation_succeeds(DifficultyLevel difficulty)
    {
        var owner = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);

        var response = await owner.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest(difficulty));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Undefined_difficulty_is_rejected_and_filtering_returns_expected_results()
    {
        var owner = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);
        await _factory.CreateRecipeAsync(_factory.UserId, DifficultyLevel.Easy);
        await _factory.CreateRecipeAsync(_factory.UserId, DifficultyLevel.Hard);

        var invalid = await owner.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest((DifficultyLevel)0));
        var hard = await owner.GetFromJsonAsync<PagedRecipeResponse>("/api/Recipes/paged?difficulty=Hard&pageSize=100");

        invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        hard!.Items.Should().OnlyContain(recipe => recipe.Difficulty == DifficultyLevel.Hard);
    }

    [Fact]
    public async Task Recipe_creation_persists_ingredients_steps_and_rejects_invalid_aggregate_inputs()
    {
        var owner = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);

        var valid = await owner.PostAsJsonAsync("/api/Recipes", _factory.CreateRecipeRequest(DifficultyLevel.Easy));
        valid.EnsureSuccessStatusCode();
        var created = await valid.Content.ReadFromJsonAsync<RecipieDto>();

        created!.Ingredients.Should().ContainSingle(ingredient => ingredient.Name == "Salt" && ingredient.Quantity == "1 tsp");
        created.Steps.Should().ContainSingle(step => step.StepNumber == 1 && step.Instruction == "Cook");

        var missingIngredients = _factory.CreateRecipeRequest(DifficultyLevel.Easy);
        missingIngredients.Ingredients = [];
        var missingSteps = _factory.CreateRecipeRequest(DifficultyLevel.Easy);
        missingSteps.Steps = [];
        var invalidCategory = _factory.CreateRecipeRequest(DifficultyLevel.Easy);
        invalidCategory.CategoryId = Guid.NewGuid();

        (await owner.PostAsJsonAsync("/api/Recipes", missingIngredients)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await owner.PostAsJsonAsync("/api/Recipes", missingSteps)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await owner.PostAsJsonAsync("/api/Recipes", invalidCategory)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task My_recipes_returns_only_current_user_recipes_with_pagination()
    {
        var currentUser = await _factory.CreateUserAsync($"mine-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var otherUser = await _factory.CreateUserAsync($"not-mine-{Guid.NewGuid():N}@example.com", AppRoles.User);
        await _factory.CreateRecipeAsync(currentUser, DifficultyLevel.Easy);
        await _factory.CreateRecipeAsync(currentUser, DifficultyLevel.Medium);
        await _factory.CreateRecipeAsync(otherUser, DifficultyLevel.Hard);
        var client = _factory.CreateClientForUser(currentUser, AppRoles.User);

        var page = await client.GetFromJsonAsync<PagedRecipeResponse>("/api/Recipes/me?page=1&pageSize=1");

        page!.Items.Should().ContainSingle();
        page.Total.Should().Be(2);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(1);
        page.Items.Should().OnlyContain(recipe => recipe.Author.Id == currentUser);
    }
}

public sealed class FavoriteAndReviewBehaviorTests : IClassFixture<RecepApiFactory>
{
    private readonly RecepApiFactory _factory;

    public FavoriteAndReviewBehaviorTests(RecepApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Favorites_are_user_scoped_and_duplicate_safe()
    {
        var userId = await _factory.CreateUserAsync($"favorite-user-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var otherRecipe = await _factory.CreateRecipeAsync(_factory.AdminId, DifficultyLevel.Easy);
        var ownRecipe = await _factory.CreateRecipeAsync(userId, DifficultyLevel.Medium);
        var client = _factory.CreateClientForUser(userId, AppRoles.User);

        (await client.PostAsync($"/api/Favorites/{otherRecipe}", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/Favorites/{ownRecipe}", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/Favorites/{ownRecipe}", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.DeleteAsync($"/api/Favorites/{Guid.NewGuid()}")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var favorites = await client.GetFromJsonAsync<List<FavoriteRecipeDto>>("/api/Favorites/me");
        favorites!.Select(favorite => favorite.RecipeId).Should().Contain([otherRecipe, ownRecipe]);
    }

    [Fact]
    public async Task Reviews_are_owner_scoped_and_admin_can_moderate()
    {
        var reviewerId = await _factory.CreateUserAsync($"reviewer-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var otherId = await _factory.CreateUserAsync($"review-other-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var recipeId = await _factory.CreateRecipeAsync(_factory.AdminId, DifficultyLevel.Easy);
        var reviewer = _factory.CreateClientForUser(reviewerId, AppRoles.User);
        var other = _factory.CreateClientForUser(otherId, AppRoles.User);
        var admin = _factory.CreateClientForUser(_factory.AdminId, AppRoles.Admin);

        (await reviewer.PostAsJsonAsync("/api/Reviews", new CreateReviewDto { RecipeId = recipeId, Rating = 5, Comment = "Great" }))
            .EnsureSuccessStatusCode();
        var reviewId = await _factory.GetNewestReviewIdAsync();

        (await reviewer.PostAsJsonAsync("/api/Reviews", new CreateReviewDto { RecipeId = recipeId, Rating = 4, Comment = "Again" })).StatusCode
            .Should().Be(HttpStatusCode.Conflict);
        (await other.PostAsJsonAsync("/api/Reviews", new CreateReviewDto { RecipeId = recipeId, Rating = 0, Comment = "Bad rating" })).StatusCode
            .Should().Be(HttpStatusCode.Conflict);
        (await other.PostAsJsonAsync("/api/Reviews", new CreateReviewDto { RecipeId = recipeId, Rating = 6, Comment = "Bad rating" })).StatusCode
            .Should().Be(HttpStatusCode.Conflict);

        (await reviewer.PutAsJsonAsync($"/api/Reviews/{reviewId}", new UpdateReviewDto { Rating = 4, Comment = "Updated" }))
            .EnsureSuccessStatusCode();
        (await other.PutAsJsonAsync($"/api/Reviews/{reviewId}", new UpdateReviewDto { Rating = 3, Comment = "Nope" })).StatusCode
            .Should().Be(HttpStatusCode.BadRequest);
        (await admin.DeleteAsync($"/api/Reviews/{reviewId}")).EnsureSuccessStatusCode();

        var ownedReviewRecipe = await _factory.CreateRecipeAsync(_factory.AdminId, DifficultyLevel.Easy);
        await reviewer.PostAsJsonAsync("/api/Reviews", new CreateReviewDto { RecipeId = ownedReviewRecipe, Rating = 5, Comment = "Mine" });
        var ownedReviewId = await _factory.GetNewestReviewIdAsync();
        (await reviewer.DeleteAsync($"/api/Reviews/{ownedReviewId}")).EnsureSuccessStatusCode();
    }
}

public sealed class CulturalDiscoveryBehaviorTests : IClassFixture<RecepApiFactory>
{
    private readonly RecepApiFactory _factory;

    public CulturalDiscoveryBehaviorTests(RecepApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_can_create_cuisine_and_duplicate_slug_is_rejected()
    {
        var admin = _factory.CreateClientForUser(_factory.AdminId, AppRoles.Admin);
        var user = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);

        var nonAdmin = await user.PostAsJsonAsync("/api/Cuisines", NewCuisine("Korean", "korean", "KR"));
        var create = await admin.PostAsJsonAsync("/api/Cuisines", NewCuisine("Korean", "korean", "KR"));
        var created = await create.Content.ReadFromJsonAsync<CuisineDto>();
        var duplicate = await admin.PostAsJsonAsync("/api/Cuisines", NewCuisine("Korean Food", "korean", "KR"));

        nonAdmin.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        created!.Slug.Should().Be("korean");
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Admin_can_create_region_and_relationship_validation_is_enforced_for_recipes()
    {
        var admin = _factory.CreateClientForUser(_factory.AdminId, AppRoles.Admin);
        var user = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);

        var cuisine = await CreateCuisineAsync(admin, "Spanish", "spanish", "ES");
        var otherCuisine = await CreateCuisineAsync(admin, "Greek", "greek", "GR");
        var region = await CreateRegionAsync(admin, cuisine.Id, "Andalusia", "andalusia");
        var otherRegion = await CreateRegionAsync(admin, otherCuisine.Id, "Crete", "crete");

        var validRecipe = _factory.CreateRecipeRequest(DifficultyLevel.Easy);
        validRecipe.CuisineId = cuisine.Id;
        validRecipe.RegionId = region.Id;
        validRecipe.IsTraditional = true;

        var valid = await user.PostAsJsonAsync("/api/Recipes", validRecipe);

        var invalidRecipe = _factory.CreateRecipeRequest(DifficultyLevel.Easy);
        invalidRecipe.CuisineId = cuisine.Id;
        invalidRecipe.RegionId = otherRegion.Id;

        var invalid = await user.PostAsJsonAsync("/api/Recipes", invalidRecipe);

        valid.StatusCode.Should().Be(HttpStatusCode.Created);
        invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Referenced_cuisine_and_region_deletions_return_conflict()
    {
        var admin = _factory.CreateClientForUser(_factory.AdminId, AppRoles.Admin);
        var user = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);
        var cuisine = await CreateCuisineAsync(admin, "Thai", "thai", "TH");
        var region = await CreateRegionAsync(admin, cuisine.Id, "Isan", "isan");
        var recipeRequest = _factory.CreateRecipeRequest(DifficultyLevel.Medium);
        recipeRequest.CuisineId = cuisine.Id;
        recipeRequest.RegionId = region.Id;

        (await user.PostAsJsonAsync("/api/Recipes", recipeRequest)).EnsureSuccessStatusCode();

        var deleteRegion = await admin.DeleteAsync($"/api/Regions/{region.Id}");
        var deleteCuisine = await admin.DeleteAsync($"/api/Cuisines/{cuisine.Id}");

        deleteRegion.StatusCode.Should().Be(HttpStatusCode.Conflict);
        deleteCuisine.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cuisine_region_and_traditional_filters_return_expected_recipes()
    {
        var admin = _factory.CreateClientForUser(_factory.AdminId, AppRoles.Admin);
        var user = _factory.CreateClientForUser(_factory.UserId, AppRoles.User);
        var cuisine = await CreateCuisineAsync(admin, "Peruvian", "peruvian", "PE");
        var region = await CreateRegionAsync(admin, cuisine.Id, "Lima", "lima");
        var request = _factory.CreateRecipeRequest(DifficultyLevel.Hard);
        request.CuisineId = cuisine.Id;
        request.RegionId = region.Id;
        request.IsTraditional = true;

        (await user.PostAsJsonAsync("/api/Recipes", request)).EnsureSuccessStatusCode();

        var byCuisine = await user.GetFromJsonAsync<PagedRecipeResponse>($"/api/Recipes/paged?cuisineId={cuisine.Id}");
        var byRegion = await user.GetFromJsonAsync<PagedRecipeResponse>($"/api/Recipes/paged?regionId={region.Id}");
        var traditional = await user.GetFromJsonAsync<PagedRecipeResponse>("/api/Recipes/paged?isTraditional=true");

        byCuisine!.Items.Should().Contain(recipe => recipe.CuisineId == cuisine.Id);
        byRegion!.Items.Should().Contain(recipe => recipe.RegionId == region.Id);
        traditional!.Items.Should().Contain(recipe => recipe.IsTraditional);
    }

    private static CreateCuisineDto NewCuisine(string name, string slug, string countryCode) =>
        new()
        {
            Name = name,
            Slug = slug,
            CountryCode = countryCode,
            IsActive = true
        };

    private static async Task<CuisineDto> CreateCuisineAsync(HttpClient admin, string name, string slug, string countryCode)
    {
        var response = await admin.PostAsJsonAsync("/api/Cuisines", NewCuisine(name, slug, countryCode));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CuisineDto>())!;
    }

    private static async Task<RegionDto> CreateRegionAsync(HttpClient admin, Guid cuisineId, string name, string slug)
    {
        var response = await admin.PostAsJsonAsync("/api/Regions", new CreateRegionDto
        {
            CuisineId = cuisineId,
            Name = name,
            Slug = slug,
            IsActive = true
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RegionDto>())!;
    }
}

public sealed class SocialNetworkBehaviorTests : IClassFixture<RecepApiFactory>
{
    private readonly RecepApiFactory _factory;

    public SocialNetworkBehaviorTests(RecepApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Follow_feed_like_comment_notifications_and_privacy_are_enforced()
    {
        var followerId = await _factory.CreateUserAsync($"follower-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var cookId = await _factory.CreateUserAsync($"cook-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var otherId = await _factory.CreateUserAsync($"social-other-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var follower = _factory.CreateClientForUser(followerId, AppRoles.User);
        var cook = _factory.CreateClientForUser(cookId, AppRoles.User);
        var other = _factory.CreateClientForUser(otherId, AppRoles.User);
        var anonymous = _factory.CreateClient();

        (await anonymous.PostAsync($"/api/users/{cookId}/follow", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await follower.PostAsync($"/api/users/{followerId}/follow", null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await follower.PostAsync($"/api/users/{Guid.NewGuid()}/follow", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await follower.PostAsync($"/api/users/{cookId}/follow", null)).EnsureSuccessStatusCode();
        (await follower.PostAsync($"/api/users/{cookId}/follow", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        var status = await follower.GetStringAsync($"/api/users/{cookId}/follow-status");
        status.Should().Contain("isFollowing");
        status.Should().Contain("true");

        var followers = await follower.GetStringAsync($"/api/users/{cookId}/followers");
        followers.Should().Contain("displayName");
        followers.ToLowerInvariant().Should().NotContain("email");

        var recipeId = await _factory.CreateRecipeAsync(cookId, DifficultyLevel.Easy);
        await _factory.CreateRecipeAsync(otherId, DifficultyLevel.Hard);

        var feed = await follower.GetStringAsync("/api/feed?page=1&pageSize=10");
        feed.Should().Contain(recipeId.ToString());
        feed.Should().Contain("likeCount");
        feed.Should().NotContain(otherId.ToString());
        feed.Should().NotContain("passwordHash");

        (await anonymous.PostAsync($"/api/recipes/{recipeId}/likes", null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await follower.PostAsync($"/api/recipes/{recipeId}/likes", null)).EnsureSuccessStatusCode();
        (await follower.PostAsync($"/api/recipes/{recipeId}/likes", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        var likedStatus = await follower.GetStringAsync($"/api/recipes/{recipeId}/likes/status");
        likedStatus.Should().Contain("\"isLiked\":true");
        likedStatus.Should().Contain("\"likeCount\":1");
        (await follower.DeleteAsync($"/api/recipes/{recipeId}/likes")).EnsureSuccessStatusCode();
        var unlikedStatus = await follower.GetStringAsync($"/api/recipes/{recipeId}/likes/status");
        unlikedStatus.Should().Contain("\"isLiked\":false");
        unlikedStatus.Should().Contain("\"likeCount\":0");
        (await follower.DeleteAsync($"/api/recipes/{recipeId}/likes")).EnsureSuccessStatusCode();
        (await follower.PostAsync($"/api/recipes/{recipeId}/likes", null)).EnsureSuccessStatusCode();

        var followerExplore = await follower.GetStringAsync("/api/Recipes/paged?page=1&pageSize=20");
        followerExplore.Should().Contain(recipeId.ToString());
        followerExplore.Should().Contain("\"likeCount\":1");
        followerExplore.Should().Contain("\"isLikedByCurrentUser\":true");

        var otherExplore = await other.GetStringAsync("/api/Recipes/paged?page=1&pageSize=20");
        otherExplore.Should().Contain(recipeId.ToString());
        otherExplore.Should().Contain("\"likeCount\":1");
        otherExplore.Should().Contain("\"isLikedByCurrentUser\":false");

        var createComment = await follower.PostAsJsonAsync($"/api/recipes/{recipeId}/comments", new { content = "  Looks excellent  " });
        createComment.StatusCode.Should().Be(HttpStatusCode.Created);
        var commentId = ReadId(await createComment.Content.ReadAsStringAsync());

        (await other.PutAsJsonAsync($"/api/comments/{commentId}", new { content = "No" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await follower.PutAsJsonAsync($"/api/comments/{commentId}", new { content = "Updated comment" })).EnsureSuccessStatusCode();
        (await other.DeleteAsync($"/api/comments/{commentId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var notifications = await cook.GetStringAsync("/api/notifications");
        notifications.Should().Contain("Follow");
        notifications.Should().Contain("RecipeLike");
        notifications.Should().Contain("RecipeComment");
        notifications.Should().Contain(followerId.ToString());
        notifications.Should().NotContain("passwordHash");
        notifications.ToLowerInvariant().Should().NotContain("email");

        var unread = await cook.GetStringAsync("/api/notifications/unread-count");
        unread.Should().Contain("count");
        (await cook.PutAsync("/api/notifications/read-all", null)).EnsureSuccessStatusCode();

        (await follower.DeleteAsync($"/api/users/{cookId}/follow")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var emptyFeed = await follower.GetStringAsync("/api/feed?page=1&pageSize=10");
        emptyFeed.Should().NotContain(recipeId.ToString());
    }

    [Fact]
    public async Task Public_profile_update_is_owner_only_and_does_not_expose_email()
    {
        var userId = await _factory.CreateUserAsync($"profile-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var client = _factory.CreateClientForUser(userId, AppRoles.User);

        var update = await client.PutAsJsonAsync("/api/users/me/profile", new
        {
            displayName = "Public Cook",
            bio = "Moroccan food explorer",
            avatarUrl = "/images/avatar.png",
            countryCode = "ma"
        });
        update.EnsureSuccessStatusCode();

        var profile = await client.GetStringAsync($"/api/users/{userId}");
        profile.Should().Contain("Public Cook");
        profile.Should().Contain("MA");
        profile.ToLowerInvariant().Should().NotContain("email");
        profile.Should().NotContain("passwordHash");
    }

    [Fact]
    public async Task Reviews_no_longer_return_user_email()
    {
        var reviewerId = await _factory.CreateUserAsync($"review-privacy-{Guid.NewGuid():N}@example.com", AppRoles.User);
        var recipeId = await _factory.CreateRecipeAsync(_factory.AdminId, DifficultyLevel.Easy);
        var reviewer = _factory.CreateClientForUser(reviewerId, AppRoles.User);

        (await reviewer.PostAsJsonAsync("/api/Reviews", new CreateReviewDto { RecipeId = recipeId, Rating = 5, Comment = "Great" }))
            .EnsureSuccessStatusCode();

        var raw = await reviewer.GetStringAsync($"/api/Reviews/recipe/{recipeId}");

        raw.Should().Contain("author");
        raw.Should().NotContain("userEmail");
        raw.Should().NotContain("@example.com");
    }

    private static Guid ReadId(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("id").GetGuid();
    }
}

public sealed class RecepApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string KnownPassword = "StrongPass123";
    private const string JwtKey = "TEST_SIGNING_KEY_FOR_RECEP_INTEGRATION_1234567890";
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public Guid CategoryId { get; private set; }
    public Guid CuisineId { get; private set; }
    public Guid AdminId { get; private set; }
    public Guid UserId { get; private set; }

    public RecepApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=Unused;Trusted_Connection=True;");
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "5");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", "7");
    }

    public HttpClient CreateClientForUser(Guid userId, string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId, role));
        return client;
    }

    public CreateRecipeDto CreateRecipeRequest(DifficultyLevel difficulty)
    {
        return new CreateRecipeDto
        {
            Title = $"Recipe {Guid.NewGuid():N}",
            Description = "Description",
            PreparationTimeMinutes = 20,
            CategoryId = CategoryId,
            CuisineId = CuisineId,
            Difficulty = difficulty,
            Ingredients = [new CreateIngredientDto { Name = "Salt", Quantity = "1 tsp" }],
            Steps = [new CreateRecipeStepDto { StepNumber = 1, Instruction = "Cook" }]
        };
    }

    public async Task<Guid> CreateUserAsync(string email, string role)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = Guid.NewGuid();
        context.Users.Add(NewUser(id, email, role));
        await context.SaveChangesAsync();
        return id;
    }

    public async Task<Users?> FindUserByEmailAsync(string email)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email.Trim().ToLowerInvariant());
    }

    public async Task<Guid> CreateRecipeAsync(Guid ownerId, DifficultyLevel difficulty)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = Guid.NewGuid();
        context.Recipies.Add(new Recipie
        {
            Id = id,
            Title = $"Existing {Guid.NewGuid():N}",
            Description = "Description",
            PreparationTimeMinutes = 20,
            CategoryId = CategoryId,
            CuisineId = CuisineId,
            UserId = ownerId,
            Difficulty = difficulty
        });
        await context.SaveChangesAsync();
        return id;
    }

    public async Task<Recipie?> GetNewestRecipeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Recipies.AsNoTracking().OrderByDescending(recipe => recipe.CreatedAt).FirstOrDefaultAsync();
    }

    public async Task<Guid> GetNewestReviewIdAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.RecipeReviews
            .AsNoTracking()
            .OrderByDescending(review => review.CreatedAt)
            .Select(review => review.Id)
            .FirstAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=Unused;Trusted_Connection=True;",
                ["Jwt:Key"] = JwtKey,
                ["Jwt:AccessTokenMinutes"] = "5",
                ["Jwt:RefreshTokenDays"] = "7"
            });
        });
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        CategoryId = Guid.NewGuid();
        CuisineId = Guid.NewGuid();
        AdminId = Guid.NewGuid();
        UserId = Guid.NewGuid();

        context.Categories.Add(new Category { Id = CategoryId, Name = "Dinner" });
        context.Cuisines.Add(new Cuisine
        {
            Id = CuisineId,
            Name = "Moroccan",
            Slug = "moroccan",
            CountryCode = "MA"
        });
        context.Users.AddRange(
            NewUser(AdminId, "admin@example.com", AppRoles.Admin),
            NewUser(UserId, "user@example.com", AppRoles.User));
        await context.SaveChangesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Jwt__Key", null);
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", null);
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenDays", null);
    }

    private static Users NewUser(Guid id, string email, string role)
    {
        return new Users
        {
            Id = id,
            DisplayName = email.Split('@')[0],
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(KnownPassword),
            Role = role,
            IsActive = true
        };
    }

    private static string CreateToken(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, $"{role.ToLowerInvariant()}@example.com"),
            new Claim(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class PagedRecipeResponse
{
    public List<RecipieDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
