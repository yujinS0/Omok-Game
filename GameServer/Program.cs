using GameServer.Repository;

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

builder.Services.AddScoped<IGameDb, GameDb>(); // game mysql

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
