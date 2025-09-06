using System.CommandLine;
using System.Runtime.Loader;
using HopperHomeAssignment;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var cts = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, e) =>
{
    e.Cancel = true;
    Log.Warning(Messages.CancellationViaCtrlC);
    cts.Cancel();
};
Console.CancelKeyPress += cancelHandler;

Action<object?> unloadHandler = _ =>
{
    Log.Warning(Messages.CancellationViaSigTerm);
    cts.Cancel();
};
AssemblyLoadContext.Default.Unloading += unloadHandler;

try
{
    var rootCommand = new RootCommand("Concurrent URL Downloader");

    var configOption = new Option<FileInfo>("--config-path",
        "-file", "-f", "-path", "-p", "-config", "-c")
    {
        Description = "Path to the Json config file",
        Required = true
    };

    configOption.AcceptExistingOnly();

    rootCommand.Options.Add(configOption);

    var parseResult = rootCommand.Parse(args);

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

    foreach (var parseError in parseResult.Errors)
    {
        Log.Error(Messages.ParseError, parseError.Message);
    }

    return 1;
}
catch (OperationCanceledException)
{
    Log.Warning(Messages.OperationCancelled);
    return 2;
}
catch (Exception ex)
{
    Log.Error(Messages.UnhandledException, ex.GetType().Name, ex.Message);
    return 1;
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
    AssemblyLoadContext.Default.Unloading -= unloadHandler;
    Log.CloseAndFlush();
}