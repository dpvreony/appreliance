using System.Reflection;

namespace Dhgms.AppReliance.Common.Platform
{
    public interface IAssemblyInspector
    {
        AssemblyName GetAssemblyName(string assemblyPath);
        AssemblyName[] GetReferencedAssemblyNames(string assemblyPath);

        /// <summary>
        /// Try to resolve the physical path for a referenced assembly name using the base assembly's probing context.
        /// </summary>
        /// <param name="name">Referenced assembly name.</param>
        /// <param name="baseAssemblyPath">Path of the assembly that references the named assembly.</param>
        /// <param name="resolvedPath">Resolved full path to the referenced assembly, if found. Null if not found.</param>
        /// <param name="isInGac">True when the resolved assembly is located in the Global Assembly Cache (only meaningful on .NET Framework).</param>
        /// <returns>True when the assembly could be resolved to a path or GAC entry.</returns>
        bool TryResolveReferencedAssemblyPath(AssemblyName name, string baseAssemblyPath, out string resolvedPath, out bool isInGac);
    }
}
