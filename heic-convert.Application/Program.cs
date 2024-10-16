using heic_convert.Application;
using heic_convert.Application.Workers;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfigDirectoryService, ConfigDirectoryService>();
builder.Services.AddSingleton<IConfigValidator, ConfigValidator>();

// We are using this approach for WatchDirectoryWorker to be able to reference IPdfMerger
builder.Services.AddSingleton<ImageConversionWorker>();
builder.Services.AddSingleton<IImageConversionWorker>(x => x.GetRequiredService<ImageConversionWorker>());
builder.Services.AddHostedService(x => x.GetRequiredService<ImageConversionWorker>());
builder.Services.AddHostedService<WatchDirectoryWorker>();

var host = builder.Build();

if(host.Configuration.GetValue<bool>("AddHealthcheckEndpoint"))
{
    host.MapGet("/health", () => "healthy");
}

host.Services
    .GetRequiredService<IConfigValidator>()
    .ThrowOnInvalidConfiguration();

host.Run();
