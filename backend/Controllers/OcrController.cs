using Microsoft.AspNetCore.Mvc;
using OcrDemo.Api.Services;
using System.Text.Json;

namespace OcrDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly OcrStorageService _ocrService;
    private readonly ILogger<OcrController> _logger;

    public OcrController(OcrStorageService ocrService, ILogger<OcrController> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(long id)
    {
        try
        {
            var (task, json) = await _ocrService.GetResultAsync(id);

            return Ok(new
            {
                id = task.Id,
                data = JsonSerializer.Deserialize<object>(json),
                version = task.Version,
                status = task.Status,
                updatedAt = task.UpdatedAt ?? task.CreatedAt
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Task {id} not found" });
        }
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] JsonElement body)
    {
        try
        {
            var rawJson = body.GetProperty("rawJson").GetRawText();
            var task = await _ocrService.CreateFromRawJsonAsync(rawJson);

            return CreatedAtAction(nameof(Get), new { id = task.Id }, new
            {
                id = task.Id,
                status = task.Status,
                version = task.Version
            });
        }
        catch (KeyNotFoundException)
        {
            return BadRequest(new { message = "rawJson field is required" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] JsonElement body)
    {
        try
        {
            var data = body.GetProperty("data");
            var version = body.GetProperty("version").GetInt32();

            var success = await _ocrService.UpdateResultAsync(id, data, version);

            if (!success)
            {
                return Conflict(new { message = "数据已被他人修改，请刷新后重试" });
            }

            return Ok(new { version = version + 1 });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Task {id} not found" });
        }
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(long id, [FromBody] JsonElement body)
    {
        try
        {
            var result = body.GetProperty("result");
            await _ocrService.SaveProcessedResultAsync(id, result);

            return Ok(new { message = "Processed successfully" });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Task {id} not found" });
        }
    }
}