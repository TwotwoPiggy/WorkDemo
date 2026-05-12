using System.ComponentModel.DataAnnotations;

namespace OcrDemo.Api.Models;

public class OcrTask
{
    public long Id { get; set; }

    public string RawJsonBlobPath { get; set; } = string.Empty;

    public string ResultJsonBlobPath { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "pending";

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}