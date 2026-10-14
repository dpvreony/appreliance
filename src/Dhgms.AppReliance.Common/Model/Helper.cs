using System;
using Dhgms.AppReliance.Common.Platform;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhgms.AppReliance.Common.Model
{
    public static class Helper
    {
        public static List<Info.Dependency> GetDependencies(System.Reflection.Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return GetDependencies(assembly, new Stack<string>());
        }

        public static List<Info.Dependency> GetDependenciesFromPath(string assemblyPath)
        {
            return GetDependenciesFromPath(assemblyPath, new Stack<string>());
        }

        public static List<Info.Dependency> GetDependenciesFromPath(string assemblyPath, bool usePeReader)
        {
            return GetDependenciesFromPath(assemblyPath, new Stack<string>(), usePeReader);
        }

        private static List<Info.Dependency> GetDependencies(System.Reflection.Assembly assembly, Stack<string> dependencyStack)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            var assemblyName = assembly.GetName().Name;
            if (dependencyStack.Contains(assemblyName))
            {
                return null;
            }

            dependencyStack.Push(assemblyName);

            var refass = assembly.GetReferencedAssemblies().OrderBy(x => x.Name);

            var result = new List<Info.Dependency>();
            foreach (var ass in refass)
            {
                if (result.Any(x => x.AssemblyName.Equals(ass)))
                {
                    continue;
                }

                List<Info.Dependency> subDeps = null;
                Info.Location location = Info.Location.NotFound;

                System.Reflection.Assembly actualDependentAssembly = GetAssembly(ass);

                if (actualDependentAssembly != null)
                {
                    location = actualDependentAssembly.GlobalAssemblyCache ? Info.Location.GlobalAssemblyCache : Info.Location.FileSystem;

                    // if we've already walked this assembly don't do it again.
                    // prevents stackoverflow where there are circular references.
                    // also don't walk into the Core .NET libraries, there is no point.
                    subDeps = !dependencyStack.Contains(ass.Name) && !ass.Name.StartsWith("System.") ? GetDependencies(actualDependentAssembly, dependencyStack) : null;
                }

                result.Add(new Info.Dependency { AssemblyName = ass, Dependencies = subDeps, Location = location });
            }

            _ = dependencyStack.Pop();

            return result.Count > 0 ? result : null;
        }

        private static List<Info.Dependency> GetDependenciesFromPath(string assemblyPath, Stack<string> dependencyStack)
        {
            return GetDependenciesFromPath(assemblyPath, dependencyStack, false);
        }

        private static List<Info.Dependency> GetDependenciesFromPath(string assemblyPath, Stack<string> dependencyStack, bool usePeReader)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException("assemblyPath");
            }

            IAssemblyInspector inspector;
            if (usePeReader)
            {
                // prefer PEReader-based inspector when requested
    #if NETFRAMEWORK
                inspector = new Dhgms.AppReliance.Common.Platform.AssemblyInspectorPeReader();
    #else
                inspector = Dhgms.AppReliance.Common.Platform.AssemblyInspectorProvider.Instance;
    #endif
            }
            else
            {
                inspector = Dhgms.AppReliance.Common.Platform.AssemblyInspectorProvider.Instance;
            }
            var assemblyName = inspector.GetAssemblyName(assemblyPath);
            var assemblyShortName = assemblyName.Name;

            if (dependencyStack.Contains(assemblyShortName))
            {
                return null;
            }

            dependencyStack.Push(assemblyShortName);

            var refs = inspector.GetReferencedAssemblyNames(assemblyPath).OrderBy(x => x.Name);

            var result = new List<Info.Dependency>();
            foreach (var ass in refs)
            {
                if (result.Any(x => x.AssemblyName.Equals(ass)))
                {
                    continue;
                }

                List<Info.Dependency> subDeps = null;
                Info.Location location = Info.Location.NotFound;

                string resolvedPath;
                bool isInGac;
                var found = inspector.TryResolveReferencedAssemblyPath(ass, assemblyPath, out resolvedPath, out isInGac);

                if (found)
                {
                    if (isInGac)
                    {
                        location = Info.Location.GlobalAssemblyCache;
                    }
                    else
                    {
                        location = Info.Location.FileSystem;
                    }

                    // don't walk into system assemblies or already-walked names
                    subDeps = !dependencyStack.Contains(ass.Name) && !ass.Name.StartsWith("System.") && location == Info.Location.FileSystem
                        ? GetDependenciesFromPath(resolvedPath, dependencyStack)
                        : null;
                }

                result.Add(new Info.Dependency { AssemblyName = ass, Dependencies = subDeps, Location = location });
            }

            _ = dependencyStack.Pop();

            return result.Count > 0 ? result : null;
        }

        private static System.Reflection.Assembly GetAssembly(System.Reflection.AssemblyName ass)
        {
            var loader = Dhgms.AppReliance.Common.Platform.AssemblyLoaderProvider.Instance;
            var resolverMethods = new Func<System.Reflection.Assembly>[]
                {
                    () => loader.LoadByFullName(ass.FullName),
                    () => loader.LoadFromFileName(ass.Name + ".dll"),
                    () => loader.LoadFromFileName(ass.Name + ".exe")
                };

            foreach (var resolverMethod in resolverMethods)
            {
                var a = resolverMethod();

                if (a != null)
                {
                    return a;
                }
            }
            /*
            var a = System.Reflection.Assembly.Load(ass.FullName);

            if (a != null)
            {
                return a;
            }
            
            a = System.Reflection.Assembly.LoadFrom(ass.Name + ".dll");

            if (a != null)
            {
                return a;
            }

            a = System.Reflection.Assembly.LoadFrom(ass.Name + ".exe");

            if (a != null)
            {
                return a;
            }
             * */

            return null;
        }
    }
}
