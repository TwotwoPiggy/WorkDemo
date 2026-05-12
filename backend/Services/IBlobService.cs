namespace OcrDemo.Api.Services;

public interface IBlobService
{
    Task<string> DownloadAsync(string blobPath);

    Task UploadAsync(string blobPath, string content);
}