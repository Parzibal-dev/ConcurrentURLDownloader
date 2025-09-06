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

        // Semaphore controls concurrent download limit
        using var semaphore = new SemaphoreSlim(config.MaxConcurrentDownloads);
        var successCount = 0;

        // Create tasks for all downloads - they'll be limited by the semaphore
        var tasks = config.Urls.Select(async url =>
        {
            var accessedResource = false;
            try
            {
                // Wait for available slot in semaphore (blocks if limit reached)
                await semaphore.WaitAsync(cancellationToken);
                accessedResource = true;

                var singleTime = Stopwatch.StartNew();
                Log.Information(Messages.DownloadingUrl, url, config.OutputPath);
                await DownloadFileAsync(url, httpClient, cancellationToken);

                singleTime.Stop();
                Log.Information(Messages.DownloadCompleted, url, singleTime.ElapsedMilliseconds);
                // Thread-safe increment for concurrent access
                Interlocked.Increment(ref successCount);
            }
            catch (OperationCanceledException)
            {
                Log.Warning(Messages.DownloadCancelled, url);
                throw; // Re-throw to bubble up cancellation
            }
            catch (Exception ex)
            {
                Log.Error(Messages.DownloadFailed, url, ex.Message);
                // Don't re-throw - continue with other downloads
            }
            finally
            {
                // Always release semaphore if we acquired it
                if (accessedResource) semaphore.Release();
            }
        }).ToList();

        try
        {
            // Wait for all downloads to complete (or fail)
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
        // Extract filename from URL path for local storage
        var outputPath = Path.Combine(config.OutputPath, Path.GetFileName(new Uri(url).LocalPath));

        await using var responseStream = await httpClient.GetStreamAsync(url, cancellationToken);
        await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        // Stream directly to file to avoid loading entire content into memory
        await responseStream.CopyToAsync(fileStream, cancellationToken);
    }
}