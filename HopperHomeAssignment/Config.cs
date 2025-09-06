using System.Text.Json;
using Serilog;

namespace HopperHomeAssignment;

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

    // Factory as a constructor alternative, to allow async file reading
    public static async Task<Config> LoadFromFileAsync(FileInfo filePath, CancellationToken cancellationToken = default)
    {
        if (!filePath.Exists)
        {
            throw new FileNotFoundException(Messages.ConfigFileNotFound.Replace("{Path}", filePath.FullName));
        }

        await using var stream = filePath.OpenRead();
        var config = await JsonSerializer.DeserializeAsync<Config>(stream, JsonOptions, cancellationToken) ??
                     throw new InvalidDataException(Messages.ConfigDeserializeFailed);

        config.Validate();
        return config;
    }

    public void Validate()
    {
        if (Urls.Count == 0)
        {
            throw new InvalidDataException(Messages.UrlListEmpty);
        }

        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            throw new InvalidDataException(Messages.OutputPathEmpty);
        }

        if (MaxConcurrentDownloads <= 0)
        {
            throw new InvalidDataException(Messages.MaxConcurrentDownloadsInvalid);
        }
    }
}