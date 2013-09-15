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
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            var result = new List<Info.Dependency>();

            var refass = assembly.GetReferencedAssemblies();

            foreach (var ass in refass)
            {
                List<Info.Dependency> subDeps = null;
                Info.Location location = Info.Location.NotFound;

                try
                {
                    var actualDependentAssembly = System.Reflection.Assembly.Load(ass.FullName);
                    location = actualDependentAssembly.GlobalAssemblyCache ? Info.Location.GlobalAssemblyCache : Info.Location.FileSystem;
                    subDeps = GetDependencies(actualDependentAssembly);
                }
                catch
                {
                }

                result.Add(new Info.Dependency { AssemblyName = ass, Dependencies = subDeps, Location = location });
            }

            return result;
        }
    }
}
