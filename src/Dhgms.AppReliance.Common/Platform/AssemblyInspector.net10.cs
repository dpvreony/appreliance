using System;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Collections.Generic;
using System.Runtime.Loader;

namespace Dhgms.AppReliance.Common.Platform
{
    internal class AssemblyInspector : IAssemblyInspector
    {
        public AssemblyName GetAssemblyName(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath)) throw new ArgumentNullException(nameof(assemblyPath));

            var full = Path.GetFullPath(assemblyPath);
            using (var stream = File.OpenRead(full))
            using (var pe = new PEReader(stream))
            {
                if (!pe.HasMetadata)
                {
                    throw new BadImageFormatException("The file does not contain .NET metadata.", full);
                }

                var reader = pe.GetMetadataReader();
                var definition = reader.GetAssemblyDefinition();
                var name = new AssemblyName
                {
                    Name = reader.GetString(definition.Name),
                    Version = definition.Version
                };

                if (!definition.Culture.IsNil)
                {
                    name.CultureName = reader.GetString(definition.Culture);
                }

                if (!definition.PublicKey.IsNil)
                {
                    var publicKey = reader.GetBlobBytes(definition.PublicKey);
                    if (publicKey.Length > 0)
                    {
                        name.SetPublicKey(publicKey);
                    }
                }

                return name;
            }
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
                    var name = new AssemblyName
                    {
                        Name = reader.GetString(aref.Name),
                        Version = aref.Version
                    };

                    if (!aref.Culture.IsNil)
                    {
                        name.CultureName = reader.GetString(aref.Culture);
                    }

                    if (!aref.PublicKeyOrToken.IsNil)
                    {
                        var publicKeyOrToken = reader.GetBlobBytes(aref.PublicKeyOrToken);
                        if ((aref.Flags & AssemblyFlags.PublicKey) != 0)
                        {
                            name.SetPublicKey(publicKeyOrToken);
                        }
                        else if (publicKeyOrToken.Length == 8)
                        {
                            name.SetPublicKeyToken(publicKeyOrToken);
                        }
                    }

                    refs.Add(name);
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

            try
            {
                var resolver = new AssemblyDependencyResolver(Path.GetFullPath(baseAssemblyPath));
                var path = resolver.ResolveAssemblyToPath(name);
                if (!string.IsNullOrEmpty(path))
                {
                    resolvedPath = path;
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            // Probe local folder next to base assembly
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
