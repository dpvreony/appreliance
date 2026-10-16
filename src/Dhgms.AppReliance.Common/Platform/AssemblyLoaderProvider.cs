using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    public static class AssemblyLoaderProvider
    {
        private static ILoggerFactory _loggerFactory = CreateFactory(false);

        public static ILoggerFactory LoggerFactory => _loggerFactory;

        public static readonly IAssemblyLoader Instance = new AssemblyLoader();

        public static void ConfigureLogging(bool verbose)
        {
            try
            {
                var old = _loggerFactory;
                _loggerFactory = CreateFactory(verbose);
                old?.Dispose();
            }
            catch
            {
                // ignore
            }
        }

        private static ILoggerFactory CreateFactory(bool verbose)
        {
            return Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                _ = builder.AddConsole();
                builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
            });
        }
    }
}
