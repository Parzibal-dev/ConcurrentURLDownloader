using System.Text.Json;

namespace ConcurrentURLDownloader;

public sealed class Config
{
    public required List<string> Urls { get; init; }
    public required TimeSpan MaxDownloadTime { get; init; }
    public required string OutputPath { get; init; }
    public required int MaxConcurrentDownloads { get; init; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // Factory method instead of constructor to enable async file handling
    public static async Task<Config> LoadFromFileAsync(FileInfo filePath, CancellationToken cancellationToken = default)
    {
        if (!filePath.Exists)
        {
            throw new FileNotFoundException(Messages.ConfigFileNotFound.Replace("{Path}", filePath.FullName));
        }

        await using var stream = filePath.OpenRead();
        var config = await JsonSerializer.DeserializeAsync<Config>(stream, JsonOptions, cancellationToken) ??
                     throw new InvalidDataException(Messages.ConfigDeserializeFailed);

        // Validate configuration immediately after loading
        config.Validate();
        return config;
    }

    public void Validate()
    {
        // Ensure we have at least one URL to download
        if (Urls.Count == 0)
        {
            throw new InvalidDataException(Messages.UrlListEmpty);
        }

        // Ensure that the output path is not empty or whitespace
        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            throw new InvalidDataException(Messages.OutputPathEmpty);
        }

        // Concurrency level must be positive for semaphore to work
        if (MaxConcurrentDownloads <= 0)
        {
            throw new InvalidDataException(Messages.MaxConcurrentDownloadsInvalid);
        }
    }
}