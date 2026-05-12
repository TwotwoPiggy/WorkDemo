using OcrDemo.Api.Data;
using OcrDemo.Api.Models;
using System.Text.Json;

namespace OcrDemo.Api.Services;

public class OcrStorageService
{
    private readonly AppDbContext _db;
    private readonly IBlobService _blob;
    private readonly ILogger<OcrStorageService> _logger;

    public OcrStorageService(AppDbContext db, IBlobService blob, ILogger<OcrStorageService> logger)
    {
        _db = db;
        _blob = blob;
        _logger = logger;
    }

    public async Task<OcrTask> CreateFromRawJsonAsync(string rawJson)
    {
        var task = new OcrTask
        {
            Status = "raw_saved",
            CreatedAt = DateTime.UtcNow
        };

        _db.OcrTasks.Add(task);
        await _db.SaveChangesAsync();

        var rawPath = $"raw/{task.Id}_raw.json";
        var resultPath = $"result/{task.Id}_v1.json";

        await _blob.UploadAsync(rawPath, rawJson);

        task.RawJsonBlobPath = rawPath;
        task.ResultJsonBlobPath = resultPath;
        task.Version = 1;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Created OCR task {TaskId}", task.Id);

        return task;
    }

    public async Task SaveProcessedResultAsync(long taskId, object processedResult)
    {
        var task = await _db.OcrTasks.FindAsync(taskId)
            ?? throw new InvalidOperationException($"Task {taskId} not found");

        var json = JsonSerializer.Serialize(processedResult);
        await _blob.UploadAsync(task.ResultJsonBlobPath, json);

        task.Status = "processed";
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<(OcrTask Task, string Json)> GetResultAsync(long taskId)
    {
        var task = await _db.OcrTasks.FindAsync(taskId)
            ?? throw new InvalidOperationException($"Task {taskId} not found");

        var json = await _blob.DownloadAsync(task.ResultJsonBlobPath);

        return (task, json);
    }

    public async Task<bool> UpdateResultAsync(long taskId, JsonElement data, int clientVersion)
    {
        var task = await _db.OcrTasks.FindAsync(taskId)
            ?? throw new InvalidOperationException($"Task {taskId} not found");

        if (task.Version != clientVersion)
            return false;

        var newVersion = task.Version + 1;
        var newBlobPath = $"result/{taskId}_v{newVersion}.json";

        var json = data.GetRawText();
        await _blob.UploadAsync(newBlobPath, json);

        var history = new OcrTaskHistory
        {
            TaskId = taskId,
            Version = newVersion,
            BlobPath = newBlobPath,
            ModifiedAt = DateTime.UtcNow
        };
        _db.OcrTaskHistories.Add(history);

        task.ResultJsonBlobPath = newBlobPath;
        task.Version = newVersion;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated OCR task {TaskId} to version {Version}", taskId, newVersion);

        return true;
    }
}