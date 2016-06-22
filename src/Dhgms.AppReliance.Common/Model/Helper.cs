using System;
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
            return GetDependencies(assembly, new Stack<string>());
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

            dependencyStack.Pop();

            return result.Count > 0 ? result : null;
        }

        private static System.Reflection.Assembly GetAssembly(System.Reflection.AssemblyName ass)
        {
            var resolverMethods = new Func<System.Reflection.Assembly>[]
                {
                    () => System.Reflection.Assembly.Load(ass.FullName),
                    () => System.Reflection.Assembly.LoadFrom(ass.Name + ".dll"),
                    () => System.Reflection.Assembly.LoadFrom(ass.Name + ".exe")
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
