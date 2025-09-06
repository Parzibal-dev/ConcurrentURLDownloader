# Concurrent URL Downloader

Run the downloader from a Docker image with a single command.

## 1. Prepare `config.json`
This includes:
- Urls - The list of Urls do download
- MaxDownloadTime - A timespan of the maximum download time for each url
- OutputPath - The folder where the files will be downloaded to
- MaxConcurrentDownloads - How many files can the program download at once

Here's an example json:
```json
{
  "Urls": [
    "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
    "https://example-files.online-convert.com/document/txt/example.txt"
  ],
  "MaxDownloadTime": "00:00:30",
  "OutputPath": "/app/Downloads",
  "MaxConcurrentDownloads": 4
}
```

Save this file in the directory where you will execute `docker run`.  
Create an empty folder named **Downloads** in the same directory, if one doesn't exist already

## 2. Run the container

### Windows PowerShell

```cmd
docker run --rm ^
  -v "%cd%\config.json:/app/config.json:ro" ^
  -v "%cd%\Downloads:/app/Downloads" ^
  parzibaldev/concurrent-url-downloader:1.0 ^
  --config-path /app/config.json
```

### macOS / Linux (bash)

```bash
docker run --rm -v "$(pwd)/config.json:/app/config.json:ro" -v "$(pwd)/Downloads:/app/Downloads" parzibaldev/concurrent-url-downloader:1.0 --config-path /app/config.json
```

Downloaded files will appear in the **Downloads** folder on your host.

**Important! Whenever changing one of the paths (config.json or Downloads) you also need to change the mounting in the docker run, or it won't work - Proceed with caution!**
