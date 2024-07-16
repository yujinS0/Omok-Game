using MatchServer.Repository;
using MatchServer;
using MatchServer.Services.Interfaces;
using MatchServer.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS ��å �߰� - blazor���� ȣ���� ����
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("ConnectionStrings")); // DbConfig ���� �ε�

builder.Services.AddSingleton<IMemoryDb, MemoryDb>(); // Game Redis
builder.Services.AddScoped<IRequestMatchingService, RequestMatchingService>();
builder.Services.AddScoped<ICheckMatchingService, CheckMatchingService>();
builder.Services.AddSingleton<MatchWorker>(); // MatchWorker �̱���?

builder.Services.AddHttpClient(); // HttpClientFactory �߰�

// �α� ����
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();

var app = builder.Build();

// CORS �̵���� �߰�
app.UseCors("AllowAllOrigins");

app.MapControllers();

app.Run();
