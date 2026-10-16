using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    internal static class GacFolderResolver
    {
        private static readonly Microsoft.Extensions.Logging.ILogger _logger = AssemblyLoaderProvider.LoggerFactory.CreateLogger("GacFolderResolver.net10");

        public static bool TryFindInGac(AssemblyName name, out string path)
        {
            path = null;
            if (name == null) return false;

            try
            {
                var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                if (string.IsNullOrEmpty(windows)) return false;

                var token = GetPublicKeyTokenString(name);
                var version = name.Version?.ToString() ?? "";
                var candidates = new[] { "GAC_MSIL", "GAC_32", "GAC_64" };

                foreach (var gac in candidates)
                {
                    var baseDir = Path.Combine(windows, "Microsoft.NET", "assembly", gac);
                    var asmDir = Path.Combine(baseDir, name.Name);
                    if (!Directory.Exists(asmDir)) continue;

                    // Look for version__token folder
                    var entries = Directory.GetDirectories(asmDir);
                        foreach (var entry in entries)
                        {
                            var file = Path.Combine(entry, name.Name + ".dll");
                            if (File.Exists(file))
                            {
                                _logger.LogDebug("Found {Name} in GAC folder {Path}", name, file);
                                path = file;
                                return true;
                            }
                        }
                }

                // Legacy GAC locations
                var legacy = Path.Combine(windows, "assembly");
                if (Directory.Exists(legacy))
                {
                    // try GAC_MSIL legacy
                    var g = Path.Combine(legacy, "GAC_MSIL", name.Name);
                    if (Directory.Exists(g))
                    {
                        var versions = Directory.GetDirectories(g);
                        foreach (var v in versions)
                        {
                            var dll = Path.Combine(v, name.Name + ".dll");
                            if (File.Exists(dll))
                            {
                                _logger.LogDebug("Found {Name} in legacy GAC folder {Path}", name, dll);
                                path = dll;
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static string GetPublicKeyTokenString(AssemblyName name)
        {
            try
            {
                var pkt = name.GetPublicKeyToken();
                if (pkt == null || pkt.Length == 0) return string.Empty;
                return string.Concat(pkt.Select(b => b.ToString("x2")));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
