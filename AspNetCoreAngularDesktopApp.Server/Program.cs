using AspNetCoreAngularDesktopApp.Server.Options;
using AspNetCoreAngularDesktopApp.Server.Pipe;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsConsole;

// App arguments
foreach (var arg in Environment.GetCommandLineArgs())
{
    if (arg == "--silent" && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        ConsoleHelper.HideConsole();
    }
}

Console.WriteLine("App started. Version: 0.0.1");

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
    app.UseHsts();
}

var appEndpointUrl = app.Configuration["Kestrel:Endpoints:App:Url"] ?? "https://localhost:55000";

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var provider = builder.Services.BuildServiceProvider();
    var pipeServer = provider.GetService<IPipeServer>();

    _ = Task.Run(async () =>
    {
        await RunPipeServerAndListen(app, pipeServer);
    });

    var systemTrayAppFilePath = string.Empty;

    if (app.Environment.IsDevelopment())
    {
        systemTrayAppFilePath = Path.Combine(Environment.CurrentDirectory, @"..\OS.Windows\SystemTrayApp\bin\Debug\net8.0-windows\SystemTrayApp.exe");
    }
    else if (app.Environment.IsProduction())
    {
        systemTrayAppFilePath = Path.Combine(Environment.CurrentDirectory, "SystemTrayApp.exe");
    }

    RunSystemTrayApp(systemTrayAppFilePath, appEndpointUrl);
}

OpenBrowser(appEndpointUrl);

app.Run();

return;

static async Task RunPipeServerAndListen(WebApplication app, IPipeServer pipeServer)
{
    var replyText = await pipeServer.RunAndGetTextReplyAsync();
    if (replyText == "command:exit")
    {
        await app.StopAsync();
    }
}

static void RunSystemTrayApp(string systemTrayAppFilePath, string url)
{
    var fileFullPath = Path.GetFullPath(systemTrayAppFilePath);

    if (File.Exists(fileFullPath))
    {
        Process.Start(fileFullPath, new List<string> { url });
    }
}

static void OpenBrowser(string url)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        Process.Start("xdg-open", url);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        Process.Start("open", url);
    }
    else
    {
        throw new NotImplementedException("Unknown OS type.");
    }
}