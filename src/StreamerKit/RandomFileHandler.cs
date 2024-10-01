// <copyright file="RandomFileHandler.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

namespace StreamerKit;

public class RandomFileHandler
{
    private static readonly HashSet<string> SupportedExtensions = new HashSet<string>
    {
        ".mp4", ".webm", ".mkv", ".avi", ".mov", ".flv", ".wmv", ".m4v", ".m4p",
        ".m4a", ".mp3", ".wav", ".ogg", ".flac", ".opus", ".aac", ".wma", ".weba",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
    };

    private Random random;
    private string baseDirectory;
    private bool defaultDelete;
    private string name;

    public RandomFileHandler(string directory, bool defaultDelete = true)
    {
        this.random = new Random();
        this.baseDirectory = directory;
        Directory.CreateDirectory(this.baseDirectory);
        this.defaultDelete = defaultDelete;
        this.name = Path.GetFileName(directory) ?? throw new DirectoryNotFoundException("The provided directory path is not valid or does not exist.");
    }

    public string Name => this.name;

    public IResult GetRandomFile (HttpContext context, bool? delete)
    {
        delete = delete ?? this.defaultDelete;
        var (file, length) = GetRandomFileFromDirectory(baseDirectory);

        var extension = Path.GetExtension(file).TrimStart('.');

        var mimeType = GetMimeTypeFromExtension(extension);

        if (string.IsNullOrEmpty(file) || !File.Exists(file))
        {
            var defaultFile = Path.Combine(this.baseDirectory, "..", $"{name}.{extension}");
            if (File.Exists(defaultFile))
            {
                return Results.File(new FileStream(defaultFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.None), mimeType);
            }

            return Results.Empty;
        }

        context.Response.Headers.Append("Total-Files", length.ToString());

        return Results.File(new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096, delete ?? false ? FileOptions.DeleteOnClose : FileOptions.None), mimeType);
    }

    private string GetMimeTypeFromExtension(string extension)
    {
        return extension switch
        {
            "mp4" => "video/mp4",
            "webm" => "video/webm",
            "mkv" => "video/x-matroska",
            "avi" => "video/x-msvideo",
            "mov" => "video/quicktime",
            "flv" => "video/x-flv",
            "wmv" => "video/x-ms-wmv",
            "m4v" => "video/x-m4v",
            "m4p" => "video/mp4",
            "m4a" => "audio/mp4",
            "mp3" => "audio/mpeg",
            "wav" => "audio/wav",
            "ogg" => "audio/ogg",
            "flac" => "audio/flac",
            "opus" => "audio/ogg",
            "aac" => "audio/aac",
            "wma" => "audio/x-ms-wma",
            "weba" => "audio/webm",
            "jpg" => "image/jpeg",
            "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "webp" => "image/webp",
            _ => "application/octet-stream",
        };
    }

    private (string, int) GetRandomFileFromDirectory(string directory)
    {
        var files = this.GetMediaFiles(directory).ToArray();
        if (files.Length == 0)
            return ("", 0);
        return (files[random.Next(files.Length)], files.Length);
    }

    private List<string> GetMediaFiles(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException("The provided directory path is not valid or does not exist.");

        var mediaFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                                  .Where(file => SupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                                  .ToList();

        return mediaFiles;
    }

    public int GetTotal()
        => this.GetMediaFiles(this.baseDirectory).Count();
}