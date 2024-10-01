// <copyright file="Program.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Reflection;
using System.Text;
using ConsoleAppFramework;
using StreamerKit;

var app = ConsoleApp.Create();
app.Add<AppCommands>();
app.Run(args);

/// <summary>
/// App Commands.
/// </summary>
#pragma warning disable SA1649 // File name should match first type name
public class AppCommands
#pragma warning restore SA1649 // File name should match first type name
{
    HashSet<RandomFileHandler> handlers = new HashSet<RandomFileHandler>();

    /// <summary>
    /// Launch the application.
    /// </summary>
    /// <param name="port">-p, Port to listen on.</param>
    /// <param name="debugMode">-d, Debug Mode. When requesting a file, it will not be deleted, no matter what the directory name is.</param>
    private WebApplication? app;
    [Command("")]
    public void Launch(int port = 8080, bool debugMode = false)
    {

        Directory.CreateDirectory(FileHelpers.GetDataDirectory());
        var builder = WebApplication.CreateBuilder();

        // Set Port
        builder.Configuration["Urls"] = $"http://*:{port}";

        this.app = builder.Build();
        this.app.UseDefaultFiles();
        this.app.UseStaticFiles();
        var directories = Directory.EnumerateDirectories(FileHelpers.GetDataDirectory());
        foreach (var directory in directories)
        {
            var name = Path.GetFileName(directory);
            var shouldNotDelete = name.EndsWith("-static", StringComparison.OrdinalIgnoreCase);
            var handler = new RandomFileHandler(directory, !debugMode && !shouldNotDelete);
            this.app.MapGet($"/{name}", handler.GetRandomFile);
            this.handlers.Add(handler);
        }

        app.MapGet("/", () => {
            var sb = new StringBuilder();
            sb.AppendLine($"StreamerKit: {this.GetAppVersion()}");
            sb.AppendLine("https://github.com/drasticactions/streamer-kit");
            sb.AppendLine("To add a new directory, add a new folder to the data directory. Then restart the application.");
            sb.AppendLine("Available Directories:");
            foreach (var handler in this.handlers)
            {
                sb.AppendLine($"- {handler.Name}");
            }
            return Results.Text(sb.ToString());
        });

        app.Run();
    }

    private string GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "Unknown";
    }
}
