namespace OcrDemo.Api.Services;

public class LocalFileBlobService : IBlobService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileBlobService> _logger;

    public LocalFileBlobService(IConfiguration configuration, ILogger<LocalFileBlobService> logger)
    {
        _logger = logger;
        _rootPath = configuration["LocalStorage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> DownloadAsync(string blobPath)
    {
        var filePath = Path.Combine(_rootPath, blobPath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Blob not found: {blobPath}");

        return await File.ReadAllTextAsync(filePath);
    }

    public async Task UploadAsync(string blobPath, string content)
    {
        var filePath = Path.Combine(_rootPath, blobPath);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(filePath, content);
        _logger.LogInformation("Saved file: {FilePath}", filePath);
    }
}