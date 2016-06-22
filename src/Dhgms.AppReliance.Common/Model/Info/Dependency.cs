using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dhgms.AppReliance.Common.Model.Info
{
    public class Dependency
    {
        public AssemblyName AssemblyName { get; set; }

        public Location Location { get; set; }

        public List<Dependency> Dependencies { get; set; }
    }
}
