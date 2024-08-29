using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;

namespace heic_convert.Application.Workers
{
    internal class ImageConversionWorker : BackgroundService, IImageConversionWorker
    {
        private readonly ILogger<ImageConversionWorker> _logger;
        private string _outDirectory;
        private string _errorDirectory;
        private ConcurrentQueue<string> _processingQueue = new();

        public ImageConversionWorker(ILogger<ImageConversionWorker> logger, 
            IConfigDirectoryService configDirectoryService,
            IConfiguration configuration)
        {
            _logger = logger;

            _outDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.OutDirectory,
                Constants.DefaultOutDirectoryName);
            _errorDirectory = configDirectoryService.GetDirectory(
                Constants.ConfigurationKeys.ErrorDirectory,
                Constants.DefaultErrorDirectoryName);

            _logger.LogInformation("Writing to directory {0}", _outDirectory);
            _logger.LogInformation("Writing corrupt HEICs to {0}", _errorDirectory);
        }

        public void EnqueueForConversion(string path)
        {
            _processingQueue.Enqueue(path);
            _logger.LogInformation("Enqueued {0}", path);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConvertQueuedFiles(stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void ConvertQueuedFiles(CancellationToken stoppingToken)
        {
            while (_processingQueue.TryDequeue(out var filePath)
                && !stoppingToken.IsCancellationRequested)
            {
                var subfolder = GetTargetSubfolderFor(filePath);
                string outFile = Path.Combine(_outDirectory, subfolder, $"{Path.GetFileNameWithoutExtension(filePath)}.jpg");

                _logger.LogInformation($"Converting {filePath}.");
                if (ConvertFile(filePath, outFile))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Converted file to {outFile}");
                }
                else
                {
                    MoveToErrorDirectory(filePath, out var directory);
                    _logger.LogError("Error occurred, moved files to error directory {0}", directory);
                }
            }
        }

        private string GetTargetSubfolderFor(string filePath)
        {
            var fileCreationTime = File.GetCreationTime(filePath);
            return fileCreationTime.ToString("yyyyMMdd");
        }

        private void MoveToErrorDirectory(string filePath, out string directory)
        {
            var errorDirectory = Path.Combine(_errorDirectory, GetTargetSubfolderFor(filePath));

            if (!Directory.Exists(errorDirectory))
            {
                Directory.CreateDirectory(errorDirectory);
            }

            var errorFile = Path.Combine(errorDirectory, Path.GetFileName(filePath));

            File.Move(filePath, errorFile);

            directory = errorDirectory;
        }

        private bool ConvertFile(string filePath, string outFile)
        {
            try
            {
                var targetDirectory = Path.GetDirectoryName(outFile);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory!); 
                }

                var process = Process.Start(new ProcessStartInfo("magick")
                {
                    Arguments = $"convert \"{filePath}\" \"{outFile}\""
                });

                process?.WaitForExit();
                _logger.LogInformation($"Exit code {process?.ExitCode}");
                return process?.ExitCode == 0;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }
    }
}
