using Core.Application.Common;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services;
public sealed class LocalRecipeImageStorage : IRecipeImageStorage
{
    private const string PublicPrefix = "/images/recipes/";
    private static readonly IReadOnlyDictionary<string, string> Extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".webp"] = "image/webp" };
    private readonly string _directory; private readonly long _maxFileSize; private readonly HashSet<string> _allowedTypes; private readonly ILogger<LocalRecipeImageStorage> _logger;
    public LocalRecipeImageStorage(IWebHostEnvironment environment, IConfiguration configuration, ILogger<LocalRecipeImageStorage> logger)
    { _directory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "images", "recipes"); _maxFileSize = configuration.GetValue<long>("RecipeImages:MaxFileSizeBytes", 5 * 1024 * 1024); _allowedTypes = (configuration.GetSection("RecipeImages:AllowedContentTypes").Get<string[]>() ?? ["image/jpeg", "image/png", "image/webp"]).ToHashSet(StringComparer.OrdinalIgnoreCase); _logger = logger; }
    public async Task<string> SaveAsync(RecipeImageUpload upload, CancellationToken cancellationToken = default)
    { if (upload.Length <= 0) throw new RecipeImageValidationException("invalid_image", "An image file is required."); if (upload.Length > _maxFileSize) throw new RecipeImageValidationException("image_too_large", "Image exceeds the allowed size."); var extension = Path.GetExtension(upload.FileName); if (!Extensions.TryGetValue(extension, out var expectedType) || !_allowedTypes.Contains(upload.ContentType) || !string.Equals(expectedType, upload.ContentType, StringComparison.OrdinalIgnoreCase)) throw new RecipeImageValidationException("unsupported_image_type", "Only JPEG, PNG, and WEBP images are supported."); if (!await HasExpectedSignatureAsync(upload.Content, expectedType, cancellationToken)) throw new RecipeImageValidationException("invalid_image", "The uploaded file is not a valid image."); Directory.CreateDirectory(_directory); var fileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant(); var physicalPath = Path.Combine(_directory, fileName); await using var output = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true); await upload.Content.CopyToAsync(output, cancellationToken); return PublicPrefix + fileName; }
    public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    { if (!TryGetManagedPath(imageUrl, out var physicalPath)) return Task.CompletedTask; try { if (File.Exists(physicalPath)) File.Delete(physicalPath); } catch (Exception ex) { _logger.LogWarning(ex, "Could not remove managed recipe image {ImageUrl}", imageUrl); } return Task.CompletedTask; }
    private bool TryGetManagedPath(string imageUrl, out string path)
    { path = string.Empty; if (!imageUrl.StartsWith(PublicPrefix, StringComparison.Ordinal) || imageUrl.Contains("..", StringComparison.Ordinal) || imageUrl.Contains((char)92)) return false; var name = imageUrl[PublicPrefix.Length..]; if (name.Length == 0 || name != Path.GetFileName(name)) return false; var candidate = Path.GetFullPath(Path.Combine(_directory, name)); var root = Path.GetFullPath(_directory) + Path.DirectorySeparatorChar; if (!candidate.StartsWith(root, StringComparison.Ordinal)) return false; path = candidate; return true; }
    private static async Task<bool> HasExpectedSignatureAsync(Stream stream, string contentType, CancellationToken cancellationToken)
    { if (!stream.CanSeek) return false; var position = stream.Position; var header = new byte[12]; var read = await stream.ReadAsync(header.AsMemory(), cancellationToken); stream.Position = position; return contentType switch { "image/jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF, "image/png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "image/webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8), _ => false }; }
}
