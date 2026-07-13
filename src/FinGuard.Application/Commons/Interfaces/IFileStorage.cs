namespace FinGuard.Application.Commons.Interfaces;

public interface IFileStorage
{
    Task<string> SaveAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken);
}
