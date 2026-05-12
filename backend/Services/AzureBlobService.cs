using Azure.Storage.Blobs;

namespace OcrDemo.Api.Services;

public class AzureBlobService : IBlobService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobService> _logger;

    public AzureBlobService(IConfiguration configuration, ILogger<AzureBlobService> logger)
    {
        _logger = logger;
        var connectionString = configuration.GetConnectionString("AzureBlob")
                              ?? configuration["AzureBlob:ConnectionString"];
        var containerName = configuration["AzureBlob:ContainerName"] ?? "ocr-data";

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> DownloadAsync(string blobPath)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }

    public async Task UploadAsync(string blobPath, string content)
    {
        var blobClient = _containerClient.GetBlobClient(blobPath);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        await blobClient.UploadAsync(stream, overwrite: true);
        _logger.LogInformation("Uploaded blob: {BlobPath}", blobPath);
    }
}