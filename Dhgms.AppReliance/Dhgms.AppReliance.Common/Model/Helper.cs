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

            if (dependencyStack.Contains(assembly.FullName))
            {
                return null;
            }

            dependencyStack.Push(assembly.FullName);

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

                try
                {
                    var actualDependentAssembly = System.Reflection.Assembly.Load(ass.FullName);
                    location = actualDependentAssembly.GlobalAssemblyCache ? Info.Location.GlobalAssemblyCache : Info.Location.FileSystem;

                    // if we've already walked this assembly don't do it again.
                    // prevents stackoverflow where there are circular references.
                    subDeps = !dependencyStack.Contains(ass.FullName) ? GetDependencies(actualDependentAssembly, dependencyStack) : null;
                }
                catch
                {
                }

                result.Add(new Info.Dependency { AssemblyName = ass, Dependencies = subDeps, Location = location });
            }

            dependencyStack.Pop();

            return result.Count > 0 ? result : null;
        }
    }
}
