namespace HopperHomeAssignment;

public abstract class Messages
{
    // --------------- Info ---------------- //
    public const string StartingDownloads =
        "Starting downloads to {OutputPath} with max {MaxConcurrentDownloads} concurrent downloads.";
    public const string DownloadingUrl = "Downloading {Url} to {OutputPath}";
    public const string DownloadCompleted     = "Completed download of {Url} in {Elapsed} ms";
    public const string TotalDownloadsSummary = "Finished {Success}/{Total} downloads in {Elapsed} ms total";

    // ---------------- Warning ---------------- //
    public const string CancellationViaCtrlC = "Cancellation requested via Ctrl+C";
    public const string CancellationViaSigTerm = "Cancellation requested via SIGTERM";
    public const string DownloadCancelled = "Download cancelled for {Url}";
    public const string AllDownloadsCancelled = "All downloads cancelled";
    public const string OperationCancelled = "Operation cancelled";

    // ---------------- Error ---------------- //
    // Configuration related errors
    public const string ConfigFileNotFound = "CFG001 Config file not found: {Path}";
    public const string ConfigDeserializeFailed = "CFG002 Failed to deserialize config file.";
    public const string UrlListEmpty = "CFG003 Configuration must contain at least one URL.";
    public const string OutputPathEmpty = "CFG004 OutputPath cannot be empty.";
    public const string MaxConcurrentDownloadsInvalid = "CFG005 MaxConcurrentDownloads must be greater than zero.";
    public const string StartDownloadFailed = "CFG006 Failed to start downloads: {Message}";
    public const string ParseError = "CFG007 Command line parse error: {Message}";

    // Download related errors
    public const string DownloadFailed = "DL001 Failed downloading {Url}: {Message}";

    // ---------------- Fatal ---------------- //
    public const string UnhandledException = "{ExceptionType}: {Message}";
}