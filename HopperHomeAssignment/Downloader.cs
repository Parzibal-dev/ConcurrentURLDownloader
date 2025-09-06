namespace HopperHomeAssignment;

using Serilog;
using System.Diagnostics;

public class Downloader(Config config)
{
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var totalTime = Stopwatch.StartNew();
        Directory.CreateDirectory(config.OutputPath);

        using var httpClient = new HttpClient();
        httpClient.Timeout = config.MaxDownloadTime;

        using var semaphore = new SemaphoreSlim(config.MaxConcurrentDownloads);
        var successCount = 0;

        var tasks = config.Urls.Select(async url =>
        {
            var accessedResource = false;
            try
            {
                await semaphore.WaitAsync(cancellationToken);
                accessedResource = true;

                var singleTime = Stopwatch.StartNew();
                Log.Information(Messages.DownloadingUrl, url, config.OutputPath);
                await DownloadFileAsync(url, httpClient, cancellationToken);

                singleTime.Stop();
                Log.Information(Messages.DownloadCompleted, url, singleTime.ElapsedMilliseconds);
                Interlocked.Increment(ref successCount);
            }
            catch (OperationCanceledException)
            {
                Log.Warning(Messages.DownloadCancelled, url);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(Messages.DownloadFailed, url, ex.Message);
            }
            finally
            {
                if (accessedResource) semaphore.Release();
            }
        }).ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Log.Warning(Messages.AllDownloadsCancelled);
            throw;
        }

        totalTime.Stop();
        Log.Information(Messages.TotalDownloadsSummary, successCount, tasks.Count, totalTime.ElapsedMilliseconds);
    }

    private async Task DownloadFileAsync(string url, HttpClient httpClient, CancellationToken cancellationToken)
    {
        var outputPath = Path.Combine(config.OutputPath, Path.GetFileName(new Uri(url).LocalPath));

        await using var responseStream = await httpClient.GetStreamAsync(url, cancellationToken);
        await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        await responseStream.CopyToAsync(fileStream, cancellationToken);
    }
}