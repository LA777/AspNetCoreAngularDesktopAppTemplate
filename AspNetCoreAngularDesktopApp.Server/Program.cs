using AspNetCoreAngularDesktopApp.Server.Options;
using AspNetCoreAngularDesktopApp.Server.Pipe;

Console.WriteLine("App started. Version: 0.0.1");

var trayAppFilePath = string.Empty;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Configuration.GetSection("ASPNETCORE_ENVIRONMENT").Value ?? "Production";

builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var settings = new Settings();
builder.Configuration.GetSection(nameof(Settings)).Bind(settings);

// Add services to the container.
builder.Services.AddSingleton<IPipeServer, PipeServer>();
builder.Services.AddOptions<Settings>().Bind(builder.Configuration.GetSection(nameof(Settings)));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

if (app.Environment.IsProduction())
{
    app.UseHsts(); // https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-8.0&tabs=visual-studio%2Clinux-ubuntu

    trayAppFilePath = Path.Combine(Environment.CurrentDirectory, "SystemTrayApp.exe");
}

app.Run();
