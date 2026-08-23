namespace DentalClinic.API.DTOs.Attachments;

public sealed class AttachmentDownloadResult : IDisposable
{
    public required Stream Content { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }

    public void Dispose() => Content.Dispose();
}
