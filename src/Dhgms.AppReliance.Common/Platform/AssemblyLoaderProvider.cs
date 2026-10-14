using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    public static class AssemblyLoaderProvider
    {
        // Shared logger factory for loaders; uses Console for simplicity
        public static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            _ = builder.AddConsole();
        });

        // Implementation class AssemblyLoader is provided per-targetframework in
        // AssemblyLoader.net10.cs and AssemblyLoader.net481.cs
        public static readonly IAssemblyLoader Instance = new AssemblyLoader();
    }
}
