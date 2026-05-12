using Microsoft.EntityFrameworkCore;
using OcrDemo.Api.Data;
using OcrDemo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    ));

var useAzureBlob = !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("AzureBlob"));
if (useAzureBlob)
{
    builder.Services.AddSingleton<IBlobService, AzureBlobService>();
}
else
{
    builder.Services.AddSingleton<IBlobService, LocalFileBlobService>();
}

builder.Services.AddScoped<OcrStorageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();