using Core.Application.Common;
using Core.Application.DTO.Recipe;
using Core.Application.Options;
using Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Recep.UnitTests;

public sealed class LocalRecipeMediaStorageTests
{
    [Fact]
    public void Uses_default_wwwroot_recipe_media_directory()
    {
        using var temp = new TempDirectory();
        var webRoot = Path.Combine(temp.Path, "wwwroot");
        var storage = CreateStorage(temp.Path, webRoot, new RecipeMediaOptions());

        storage.StorageDirectory.Should().Be(Path.GetFullPath(Path.Combine(webRoot, "images", "recipes")));
        storage.PublicPath.Should().Be("/images/recipes");
    }

    [Fact]
    public async Task Saves_with_configured_storage_path_and_public_url_then_deletes_managed_file()
    {
        using var temp = new TempDirectory();
        var configuredStorage = Path.Combine(temp.Path, "persistent", "recipes");
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions
        {
            StoragePath = configuredStorage,
            PublicPath = "/media/recipes"
        });

        var url = await storage.SaveAsync(Upload("photo.jpg", "image/jpeg", JpegBytes()));
        var savedFile = Directory.GetFiles(configuredStorage).Single();

        url.Should().StartWith("/media/recipes/").And.EndWith(".jpg");
        File.Exists(savedFile).Should().BeTrue();

        await storage.DeleteAsync(url);

        File.Exists(savedFile).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ignores_traversal_and_unmanaged_paths()
    {
        using var temp = new TempDirectory();
        var outside = Path.Combine(temp.Path, "outside.jpg");
        await File.WriteAllBytesAsync(outside, JpegBytes());
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions());

        await storage.DeleteAsync("/images/recipes/../outside.jpg");
        await storage.DeleteAsync("/images/recipes/nested/file.jpg");
        await storage.DeleteAsync("/other/managed.jpg");

        File.Exists(outside).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(ValidMedia))]
    public async Task Saves_valid_media_signatures(string fileName, string contentType, byte[] bytes)
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions());

        var url = await storage.SaveAsync(Upload(fileName, contentType, bytes));

        url.Should().StartWith("/images/recipes/");
        Directory.GetFiles(storage.StorageDirectory).Should().ContainSingle();
    }

    [Fact]
    public async Task Rejects_invalid_signature()
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions());

        var act = () => storage.SaveAsync(Upload("photo.jpg", "image/jpeg", [1, 2, 3, 4]));

        var ex = await act.Should().ThrowAsync<RecipeMediaValidationException>();
        ex.Which.Code.Should().Be("invalid_media");
    }

    [Theory]
    [InlineData("clip.mp4", "video/mp4")]
    [InlineData("clip.webm", "video/webm")]
    public async Task Rejects_video_uploads(string fileName, string contentType)
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions());

        var act = () => storage.SaveAsync(Upload(fileName, contentType, [1, 2, 3, 4]));

        var ex = await act.Should().ThrowAsync<RecipeMediaValidationException>();
        ex.Which.Code.Should().Be("unsupported_media_type");
        ex.Which.Message.Should().Be("Only JPEG, PNG, and WebP images are supported.");
    }

    [Fact]
    public async Task Rejects_extension_and_mime_mismatch()
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions());

        var act = () => storage.SaveAsync(Upload("photo.png", "image/jpeg", JpegBytes()));

        var ex = await act.Should().ThrowAsync<RecipeMediaValidationException>();
        ex.Which.Code.Should().Be("unsupported_media_type");
    }

    [Fact]
    public async Task Rejects_image_size_limit()
    {
        using var temp = new TempDirectory();
        var storage = CreateStorage(temp.Path, Path.Combine(temp.Path, "wwwroot"), new RecipeMediaOptions
        {
            MaxImageFileSizeBytes = 3
        });

        var act = () => storage.SaveAsync(Upload("photo.jpg", "image/jpeg", JpegBytes()));

        var ex = await act.Should().ThrowAsync<RecipeMediaValidationException>();
        ex.Which.Code.Should().Be("media_too_large");
    }

    public static TheoryData<string, string, byte[]> ValidMedia() => new()
    {
        { "photo.jpg", "image/jpeg", JpegBytes() },
        { "photo.png", "image/png", PngBytes() },
        { "photo.webp", "image/webp", WebpBytes() }
    };

    private static LocalRecipeMediaStorage CreateStorage(string contentRoot, string webRoot, RecipeMediaOptions options)
    {
        Directory.CreateDirectory(contentRoot);
        Directory.CreateDirectory(webRoot);
        return new LocalRecipeMediaStorage(
            new TestWebHostEnvironment(contentRoot, webRoot),
            Options.Create(options),
            NullLogger<LocalRecipeMediaStorage>.Instance);
    }

    private static RecipeMediaUpload Upload(string fileName, string contentType, byte[] bytes)
    {
        return new RecipeMediaUpload
        {
            Content = new MemoryStream(bytes),
            FileName = fileName,
            ContentType = contentType,
            Length = bytes.Length
        };
    }

    private static byte[] JpegBytes() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0xFF, 0xD9];
    private static byte[] PngBytes() => [137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0];
    private static byte[] WebpBytes() => [82, 73, 70, 70, 4, 0, 0, 0, 87, 69, 66, 80, 0, 0, 0, 0];

    private sealed class TestWebHostEnvironment(string contentRoot, string webRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Recep.UnitTests";
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRoot);
        public string ContentRootPath { get; set; } = contentRoot;
        public string EnvironmentName { get; set; } = "Testing";
        public string WebRootPath { get; set; } = webRoot;
        public IFileProvider WebRootFileProvider { get; set; } = new PhysicalFileProvider(webRoot);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"recepie-media-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
