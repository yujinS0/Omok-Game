using GameServer.Repository;
using GameServer.Services.Interfaces;
using GameServer.Services;
using MatchServer.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS 정책 추가 - blazor에서 호출을 위한
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

builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("ConnectionStrings")); 
builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("RedisConfig")); 

builder.Services.AddScoped<IGameDb, GameDb>(); // game mysql
builder.Services.AddSingleton<IMemoryDb, MemoryDb>(); // Game Redis
builder.Services.AddSingleton<IMasterDb, MasterDb>(); // Master Data
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPlayerInfoService, PlayerInfoService>();

builder.Services.AddHttpClient(); // HttpClientFactory 추가

// 로깅 설정
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();

var app = builder.Build();

// CORS 미들웨어 추가
app.UseCors("AllowAllOrigins");

app.UseMiddleware<GameServer.Middleware.CheckVersion>();
app.UseMiddleware<GameServer.Middleware.CheckUserAuth>();

app.MapControllers();

app.Run();
