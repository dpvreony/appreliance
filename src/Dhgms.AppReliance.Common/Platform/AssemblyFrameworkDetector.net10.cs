using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Dhgms.AppReliance.Common.Platform
{
    public static class AssemblyFrameworkDetector
    {
        public static bool IsDotNetFrameworkAssembly(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath)) throw new ArgumentNullException(nameof(assemblyPath));

            var full = Path.GetFullPath(assemblyPath);

            try
            {
                using (var stream = File.OpenRead(full))
                using (var pe = new PEReader(stream))
                {
                    if (!pe.HasMetadata) return false;

                    var reader = pe.GetMetadataReader();

                    // If the assembly references mscorlib it's almost certainly .NET Framework
                    foreach (var aref in reader.AssemblyReferences)
                    {
                        var arefDef = reader.GetAssemblyReference(aref);
                        var name = reader.GetString(arefDef.Name);
                        if (string.Equals(name, "mscorlib", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }
    }
}
