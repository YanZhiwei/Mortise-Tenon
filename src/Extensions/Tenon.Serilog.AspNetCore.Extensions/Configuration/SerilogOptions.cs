using Serilog;

namespace Tenon.Serilog.AspNetCore.Extensions.Configuration;

public class SerilogOptions
{
    public string LogFolder { get; set; } = "tenon";

    public ConsoleOptions Console { get; set; } = new();

    public FileOptions File { get; set; } = new();

    public class ConsoleOptions
    {
        public string OutputTemplate { get; set; } =
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
    }

    public class FileOptions
    {
        public string Path { get; set; } = "logs/.log";
        public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
        public int RetainedFileCountLimit { get; set; } = 7;

        public string OutputTemplate { get; set; } =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
    }
}