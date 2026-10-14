using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    // Included only for net481 target framework
    internal class AssemblyLoader : IAssemblyLoader
    {
        private readonly ILogger _logger;

        public AssemblyLoader()
        {
            _logger = AssemblyLoaderProvider.LoggerFactory.CreateLogger("AssemblyLoader.net481");
        }

        public Assembly? LoadFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var full = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(full);

            ResolveEventHandler handler = (sender, args) =>
            {
                try
                {
                    var name = new AssemblyName(args.Name).Name + ".dll";
                    var probe = Path.Combine(dir, name);
                    _logger.LogDebug("AssemblyResolve probing for {Name} -> {Probe}", args.Name, probe);
                    if (File.Exists(probe))
                    {
                        return Assembly.LoadFrom(probe);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "AssemblyResolve handler failed for {Name}", args.Name);
                }

                return null;
            };

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += handler;

                _logger.LogInformation("Loading assembly from path {Path}", full);

                // Ensure native dependencies in the plugin directory can be found by the loader
                var set = SetDllDirectory(dir);

                try
                {
                    return Assembly.LoadFrom(full);
                }
                finally
                {
                    if (set)
                    {
                        // restore default search behavior
                        _ = SetDllDirectory(null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load assembly from path {Path}", full);
                return null;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= handler;
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
                return LoadFromPath(full);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to load from file name {File}", fileName);
                return null;
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
