namespace OcrDemo.Api.Models;

public class OcrTaskHistory
{
    public long Id { get; set; }

    public long TaskId { get; set; }

    public int Version { get; set; }

    public string BlobPath { get; set; } = string.Empty;

    public string? ModifiedBy { get; set; }

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}