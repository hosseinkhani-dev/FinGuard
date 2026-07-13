using FinGuard.Application.Commons.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FinGuard.Infrastructure.Storages;

public class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:RootPath"] ??
            throw new InvalidOperationException("FileStorage:RootPath configuration is missing.");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_rootPath, fileName);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            81920,
            useAsync: true);

        await stream.CopyToAsync(fileStream, cancellationToken);

        return filePath;
    }

    public async Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var stream = new FileStream(
            storagePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        return await Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (File.Exists(storagePath))
            File.Delete(storagePath);

        return Task.CompletedTask;
    }
}
