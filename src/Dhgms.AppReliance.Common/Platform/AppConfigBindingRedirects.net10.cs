using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    internal static class AppConfigBindingRedirects
    {
        private static readonly ILogger _logger = AssemblyLoaderProvider.LoggerFactory.CreateLogger("AppConfigBindingRedirects");

        public static AssemblyName ApplyRedirects(AssemblyName reference, string baseAssemblyPath)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (string.IsNullOrWhiteSpace(baseAssemblyPath)) return reference;

            try
            {
                var configPath = baseAssemblyPath + ".config";
                if (!File.Exists(configPath)) return reference;

                var doc = XDocument.Load(configPath);
                // look for runtime/assemblyBinding/ dependentAssembly entries
                XNamespace ns = "urn:schemas-microsoft-com:asm.v1";
                var dependent = doc.Descendants(ns + "dependentAssembly")
                    .FirstOrDefault(d =>
                    {
                        var id = d.Element(ns + "assemblyIdentity");
                        if (id == null) return false;
                        var name = id.Attribute("name")?.Value;
                        return string.Equals(name, reference.Name, StringComparison.OrdinalIgnoreCase);
                    });

                if (dependent != null)
                {
                    var redirect = dependent.Element(ns + "bindingRedirect");
                    if (redirect != null)
                    {
                        var newVersion = redirect.Attribute("newVersion")?.Value;
                        var oldVersion = redirect.Attribute("oldVersion")?.Value;

                        if (string.IsNullOrWhiteSpace(newVersion)) return reference;

                        // If oldVersion is provided, only apply redirect when reference.Version falls within the range
                        var apply = false;
                        if (string.IsNullOrWhiteSpace(oldVersion))
                        {
                            apply = true;
                        }
                        else
                        {
                            try
                            {
                                // oldVersion format is usually 'lowVersion-highVersion'
                                var parts = oldVersion.Split('-');
                                if (parts.Length == 2)
                                {
                                    var low = new Version(parts[0].Trim());
                                    var high = new Version(parts[1].Trim());
                                    var v = reference.Version ?? new Version(0, 0, 0, 0);
                                    if (v >= low && v <= high)
                                    {
                                        apply = true;
                                    }
                                }
                                else
                                {
                                    // single version
                                    var single = new Version(oldVersion.Trim());
                                    var v = reference.Version ?? new Version(0, 0, 0, 0);
                                    if (v == single)
                                    {
                                        apply = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, "Failed to parse oldVersion range {Old}", oldVersion);
                                apply = false;
                            }
                        }

                        if (apply)
                        {
                            _logger.LogDebug("Applying binding redirect for {Assembly} -> {NewVersion} (oldVersion={Old})", reference.Name, newVersion, oldVersion);
                            var newName = new AssemblyName(reference.FullName);
                            try
                            {
                                newName.Version = new Version(newVersion);
                                return newName;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, "Failed to parse redirect version {Version}", newVersion);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to apply app.config binding redirects for base {Base}", baseAssemblyPath);
            }

            return reference;
        }
    }
}
