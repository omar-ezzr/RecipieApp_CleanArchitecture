using Core.Application.Common;
using Core.Application.DTO.Recipe;
using Core.Application.Interfaces.Services;
using Core.Application.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class LocalRecipeMediaStorage : IRecipeMediaStorage
{
    private static readonly IReadOnlyDictionary<string, string> Extensions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly string _directory;
    private readonly string _publicPath;
    private readonly RecipeMediaOptions _options;
    private readonly ILogger<LocalRecipeMediaStorage> _logger;

    public LocalRecipeMediaStorage(
        IWebHostEnvironment environment,
        IOptions<RecipeMediaOptions> options,
        ILogger<LocalRecipeMediaStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _publicPath = NormalizePublicPath(_options.PublicPath);
        _directory = ResolveStoragePath(environment, _options.StoragePath);
    }

    public string StorageDirectory => _directory;
    public string PublicPath => _publicPath;

    public async Task<string> SaveAsync(RecipeMediaUpload upload, CancellationToken cancellationToken = default)
    {
        if (upload.Length <= 0)
        {
            throw new RecipeMediaValidationException("invalid_media", "An image file is required.");
        }

        var extension = Path.GetExtension(upload.FileName);
        if (!Extensions.TryGetValue(extension, out var expected) ||
            !string.Equals(expected, upload.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new RecipeMediaValidationException(
                "unsupported_media_type",
                "Only JPEG, PNG, and WebP images are supported.");
        }

        if (!_options.AllowedImageContentTypes.Contains(upload.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new RecipeMediaValidationException("unsupported_media_type", "Only JPEG, PNG, and WebP images are supported.");
        }

        if (upload.Length > _options.MaxImageFileSizeBytes)
        {
            throw new RecipeMediaValidationException("media_too_large", "Images must be 5 MB or smaller.");
        }

        if (!await HasExpectedSignatureAsync(upload.Content, expected, cancellationToken))
        {
            throw new RecipeMediaValidationException("invalid_media", "The uploaded file signature does not match its type.");
        }

        Directory.CreateDirectory(_directory);
        var fileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
        var path = Path.Combine(_directory, fileName);

        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await upload.Content.CopyToAsync(output, cancellationToken);

        return $"{_publicPath}/{fileName}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!TryGetManagedPath(url, out var path))
        {
            return Task.CompletedTask;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not remove managed recipe media {Url}", url);
        }

        return Task.CompletedTask;
    }

    public static string ResolveStoragePath(IWebHostEnvironment environment, string? configuredStoragePath)
    {
        if (!string.IsNullOrWhiteSpace(configuredStoragePath))
        {
            return Path.GetFullPath(Path.IsPathRooted(configuredStoragePath)
                ? configuredStoragePath
                : Path.Combine(environment.ContentRootPath, configuredStoragePath));
        }

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        return Path.GetFullPath(Path.Combine(webRoot, "images", "recipes"));
    }

    public static string NormalizePublicPath(string? publicPath)
    {
        var normalized = string.IsNullOrWhiteSpace(publicPath) ? "/images/recipes" : publicPath.Trim();
        normalized = '/' + normalized.Trim('/');
        return normalized == "/" ? "/images/recipes" : normalized;
    }

    private bool TryGetManagedPath(string url, out string path)
    {
        path = string.Empty;
        var prefix = _publicPath + "/";

        if (!url.StartsWith(prefix, StringComparison.Ordinal) ||
            url.Contains("..", StringComparison.Ordinal) ||
            url.Contains('\\'))
        {
            return false;
        }

        var fileName = url[prefix.Length..];
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
        {
            return false;
        }

        var candidate = Path.GetFullPath(Path.Combine(_directory, fileName));
        var root = Path.GetFullPath(_directory) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(root, StringComparison.Ordinal))
        {
            return false;
        }

        path = candidate;
        return true;
    }

    private static async Task<bool> HasExpectedSignatureAsync(Stream stream, string contentType, CancellationToken cancellationToken)
    {
        if (!stream.CanSeek)
        {
            return false;
        }

        var position = stream.Position;
        var header = new byte[256];
        var read = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        stream.Position = position;

        return contentType switch
        {
            "image/jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "image/webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
