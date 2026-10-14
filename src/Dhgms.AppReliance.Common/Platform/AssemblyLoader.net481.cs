using System;
using System.IO;
using System.Reflection;

namespace Dhgms.AppReliance.Common.Platform
{
    // Included only for net481 target framework
    internal class AssemblyLoader : IAssemblyLoader
    {
        public Assembly? LoadFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var full = Path.GetFullPath(path);
            try
            {
                return Assembly.LoadFrom(full);
            }
            catch
            {
                return null;
            }
        }

        public Assembly? LoadByFullName(string assemblyFullName)
        {
            if (string.IsNullOrWhiteSpace(assemblyFullName)) return null;
            try
            {
                return Assembly.Load(assemblyFullName);
            }
            catch
            {
                return null;
            }
        }

        public Assembly? LoadFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            try
            {
                var full = Path.GetFullPath(fileName);
                return Assembly.LoadFrom(full);
            }
            catch
            {
                return null;
            }
        }
    }
}
