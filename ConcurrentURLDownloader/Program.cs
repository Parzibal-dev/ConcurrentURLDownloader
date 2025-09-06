using System.CommandLine;
using System.Runtime.Loader;
using ConcurrentURLDownloader;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

// Setup cancellation token source for graceful shutdown
var cts = new CancellationTokenSource();

// Handle Ctrl+C cancellation requests
ConsoleCancelEventHandler cancelHandler = (_, e) =>
{
    e.Cancel = true; // Prevent immediate process termination
    Log.Warning(Messages.CancellationViaCtrlC);
    cts.Cancel();
};
Console.CancelKeyPress += cancelHandler;

// Handle SIGTERM cancellation requests (e.g., from process managers)
Action<object?> unloadHandler = _ =>
{
    Log.Warning(Messages.CancellationViaSigTerm);
    cts.Cancel();
};
AssemblyLoadContext.Default.Unloading += unloadHandler;

try
{
    var rootCommand = new RootCommand("Concurrent URL Downloader");

    // Configure command-line option with multiple aliases for flexibility
    var configOption = new Option<FileInfo>("--config-path",
        "-file", "-f", "-path", "-p", "-config", "-c")
    {
        Description = "Path to the Json config file",
        Required = true
    };

    configOption.AcceptExistingOnly();

    rootCommand.Options.Add(configOption);

    var parseResult = rootCommand.Parse(args);

    // Proceed only if parsing succeeded and we have a valid file
    if (parseResult.Errors.Count == 0 && parseResult.GetValue(configOption) is FileInfo parsedFile)
    {
        try
        {
            var config = await Config.LoadFromFileAsync(parsedFile, cts.Token);
            var downloader = new Downloader(config);
            Log.Information(Messages.StartingDownloads, config.OutputPath, config.MaxConcurrentDownloads);
            await downloader.StartAsync(cts.Token);
            return 0;
        }
        catch (Exception ex)
        {
            Log.Error(Messages.StartDownloadFailed, ex.Message);
            return 1;
        }
    }

    // Log all parsing errors if any occurred
    foreach (var parseError in parseResult.Errors)
    {
        Log.Error(Messages.ParseError, parseError.Message);
    }

    return 1;
}
catch (OperationCanceledException)
{
    Log.Warning(Messages.OperationCancelled);
    return 2; // Different exit code for cancellation
}
catch (Exception ex)
{
    Log.Error(Messages.UnhandledException, ex.GetType().Name, ex.Message);
    return 1;
}
finally
{
    // Clean up event handlers to prevent memory leaks
    Console.CancelKeyPress -= cancelHandler;
    AssemblyLoadContext.Default.Unloading -= unloadHandler;
    Log.CloseAndFlush();
}