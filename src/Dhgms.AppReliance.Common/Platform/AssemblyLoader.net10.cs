using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    // Included only for net10.0 target framework
    internal class AssemblyLoader : IAssemblyLoader
    {
        private readonly ILogger _logger;

        public AssemblyLoader()
        {
            _logger = AssemblyLoaderProvider.LoggerFactory.CreateLogger("AssemblyLoader.net10");
        }

        private class PluginLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;
            private readonly ILogger _logger;

            public PluginLoadContext(string pluginPath, ILogger logger)
                : base(isCollectible: true)
            {
                _resolver = new AssemblyDependencyResolver(pluginPath);
                _logger = logger;
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                try
                {
                    var path = _resolver.ResolveAssemblyToPath(assemblyName);
                    if (path != null)
                    {
                        _logger.LogDebug("Resolving managed dependency {Assembly} -> {Path}", assemblyName, path);
                        return LoadFromAssemblyPath(path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to resolve assembly {AssemblyName}", assemblyName);
                }

                return null;
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                try
                {
                    var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                    if (path != null)
                    {
                        _logger.LogDebug("Resolving native dependency {Name} -> {Path}", unmanagedDllName, path);
                        return NativeLibrary.Load(path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to resolve unmanaged dll {Name}", unmanagedDllName);
                }

                return IntPtr.Zero;
            }
        }

        public Assembly? LoadFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var full = Path.GetFullPath(path);
            try
            {
                _logger.LogInformation("Loading assembly from path {Path}", full);
                var plc = new PluginLoadContext(full, _logger);
                return plc.LoadFromAssemblyPath(full);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly from path {Path}", full);
                return null;
            }
        }

        public Assembly? LoadByFullName(string assemblyFullName)
        {
            if (string.IsNullOrWhiteSpace(assemblyFullName)) return null;
            try
            {
                _logger.LogDebug("Loading by full name {Name}", assemblyFullName);
                return Assembly.Load(assemblyFullName);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to load by full name {Name}", assemblyFullName);
                return null;
            }
        }

        public Assembly? LoadFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            try
            {
                var full = Path.GetFullPath(fileName);
                _logger.LogDebug("Loading from file name {File}", full);
                var plc = new PluginLoadContext(full, _logger);
                return plc.LoadFromAssemblyPath(full);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to load from file name {File}", fileName);
                return null;
            }
        }
    }
}
