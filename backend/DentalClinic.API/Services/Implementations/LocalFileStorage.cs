using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DentalClinic.API.Services.Interfaces;

namespace DentalClinic.API.Services.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IConfiguration config, ILogger<LocalFileStorage> logger)
    {
        _logger = logger;
        var uploads = config["Uploads:Path"] ?? "uploads";
        // store under application base path
        _rootPath = Path.Combine(AppContext.BaseDirectory, uploads);
    }

    public void EnsureStorageExists()
    {
        try
        {
            Directory.CreateDirectory(_rootPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure uploads directory exists: {Path}", _rootPath);
            throw;
        }
    }

    public async Task<string> SaveAsync(string relativePath, Stream content)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);

        using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs);
        // return a URL-friendly path relative to app root, using forward slashes
        var relativeForUrl = Path.Combine(Path.GetFileName(_rootPath), relativePath).Replace(Path.DirectorySeparatorChar, '/');
        return "/" + relativeForUrl;
    }

    public Task<bool> DeleteAsync(string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        if (!File.Exists(fullPath)) return Task.FromResult(true);
        try
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {Path}", fullPath);
            return Task.FromResult(false);
        }
    }

    public Task<(Stream Stream, string ContentType)?> OpenReadAsync(string relativePath)
    {
        var fullPath = Path.Combine(_rootPath, relativePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<(Stream Stream, string ContentType)?>(null);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = ResolveContentType(relativePath);
        return Task.FromResult<(Stream Stream, string ContentType)?>((stream, contentType));
    }

    private static string ResolveContentType(string relativePath)
    {
        var extension = Path.GetExtension(relativePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}
