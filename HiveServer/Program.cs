using HiveServer.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DbConfig>(builder.Configuration.GetSection("ConnectionStrings")); // DbConfig 설정 로드

builder.Services.AddScoped<IHiveDb, HiveDb>(); // hive mysql

// 로깅 설정
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();

// Swagger 서비스 추가
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 개발 환경에서만 Swagger를 사용하도록 설정
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
