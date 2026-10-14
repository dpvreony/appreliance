using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dhgms.AppReliance.Common.Platform
{
    internal class AssemblyInspector : IAssemblyInspector
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
            try
            {
                var asm = Assembly.ReflectionOnlyLoadFrom(full);
                return asm.GetReferencedAssemblies();
            }
            catch
            {
                // fallback: attempt to use AssemblyName.GetAssemblyName and return empty
                return new AssemblyName[0];
            }
        }

        public bool TryResolveReferencedAssemblyPath(AssemblyName name, string baseAssemblyPath, out string resolvedPath, out bool isInGac)
        {
            resolvedPath = null;
            isInGac = false;

            if (name == null) return false;
            if (string.IsNullOrWhiteSpace(baseAssemblyPath)) return false;

            // First try to load into reflection-only context so fusion and GAC probing can be used
            try
            {
                var asm = Assembly.ReflectionOnlyLoad(name.FullName);
                if (asm != null)
                {
                    resolvedPath = asm.Location;
                    isInGac = asm.GlobalAssemblyCache;
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
