using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;

namespace Dhgms.AppReliance.Common.Platform
{
    // PEReader-based inspector for .NET Framework to avoid loading assemblies into AppDomain
    internal class AssemblyInspectorPeReader : IAssemblyInspector
    {
        public AssemblyName GetAssemblyName(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath)) throw new ArgumentNullException(nameof(assemblyPath));
            var full = Path.GetFullPath(assemblyPath);
            return AssemblyName.GetAssemblyName(full);
        }

        public AssemblyName[] GetReferencedAssemblyNames(string assemblyPath)
        {
            var full = Path.GetFullPath(assemblyPath);

            using (var stream = File.OpenRead(full))
            using (var pe = new PEReader(stream))
            {
                if (!pe.HasMetadata)
                {
                    return Array.Empty<AssemblyName>();
                }

                var reader = pe.GetMetadataReader();
                var refs = new List<AssemblyName>();

                foreach (var handle in reader.AssemblyReferences)
                {
                    var aref = reader.GetAssemblyReference(handle);
                    var name = reader.GetString(aref.Name);
                    var version = aref.Version;
                    var an = new AssemblyName { Name = name, Version = version };

                    if (!aref.Culture.IsNil)
                    {
                        an.CultureName = reader.GetString(aref.Culture);
                    }

                    if (!aref.PublicKeyOrToken.IsNil)
                    {
                        var blob = reader.GetBlobBytes(aref.PublicKeyOrToken);
                        if (blob != null && blob.Length == 8)
                        {
                            an.SetPublicKeyToken(blob);
                        }
                    }

                    refs.Add(an);
                }

                return refs.ToArray();
            }
        }

        public bool TryResolveReferencedAssemblyPath(AssemblyName name, string baseAssemblyPath, out string resolvedPath, out bool isInGac)
        {
            resolvedPath = null;
            isInGac = false;

            if (name == null) return false;
            if (string.IsNullOrWhiteSpace(baseAssemblyPath)) return false;
            // First, try to locate in the Global Assembly Cache using Fusion
            try
            {
                if (FusionGacResolver.TryFindInGac(name, out var gacPath))
                {
                    resolvedPath = gacPath;
                    isInGac = true;
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            // Next probe local folder next to base assembly
            try
            {
                var baseDir = Path.GetDirectoryName(Path.GetFullPath(baseAssemblyPath));
                var probeDll = Path.Combine(baseDir, name.Name + ".dll");
                if (File.Exists(probeDll))
                {
                    resolvedPath = probeDll;
                    return true;
                }

                var probeExe = Path.Combine(baseDir, name.Name + ".exe");
                if (File.Exists(probeExe))
                {
                    resolvedPath = probeExe;
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
