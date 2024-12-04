using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Tenon.Serilog.AspNetCore.Extensions.Configuration;

namespace Tenon.Serilog.AspNetCore.Extensions.Extensions;

public static class ServiceCollectionExtension
{
    public static IHostBuilder ConfigureSerilogLogging(this IHostBuilder builder, IConfiguration configuration)
    {
        var serilogOptions = new SerilogOptions();
        configuration.GetSection("Serilog").Bind(serilogOptions);

        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            serilogOptions.LogFolder
        );

        var filePath = Path.Combine(logFolder, serilogOptions.File.Path);

        return builder.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: serilogOptions.Console.OutputTemplate)
            .WriteTo.File(filePath,
                rollingInterval: serilogOptions.File.RollingInterval,
                retainedFileCountLimit: serilogOptions.File.RetainedFileCountLimit,
                outputTemplate: serilogOptions.File.OutputTemplate));
    }
}