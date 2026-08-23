using System.IO;
using System.Threading.Tasks;

namespace DentalClinic.API.Services.Interfaces;

public interface IFileStorage
{
    /// <summary>
    /// Saves a stream to the storage and returns the public URL/path.
    /// </summary>
    Task<string> SaveAsync(string relativePath, Stream content);

    /// <summary>
    /// Deletes a file at the given relative path. Returns true if deleted or not found.
    /// </summary>
    Task<bool> DeleteAsync(string relativePath);

    /// <summary>
    /// Ensures storage root exists (e.g., creates folder).
    /// </summary>
    void EnsureStorageExists();
}
