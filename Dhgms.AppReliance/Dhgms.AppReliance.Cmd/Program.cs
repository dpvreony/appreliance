using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhgms.AppReliance.Cmd
{
    class Program
    {
        static int Main(string[] args)
        {
            ShowHeader();

            if (args.Length != 1)
            {
                ShowHelp();
                return (int)Model.Info.ResultCode.InvalidNumberOfArguments;
            }

            if (args[0].Equals("/?"))
            {
                ShowHelp();
                return (int)Model.Info.ResultCode.Success;
            }

            var fileName = args[0];
            if (!System.IO.File.Exists(fileName))
            {
                Console.Error.WriteLine(string.Format("File Not Found: {0}", fileName));
                return (int)Model.Info.ResultCode.FileNotFound;
            }

            var fileExtension = System.IO.Path.GetExtension(fileName);
            var validExtensions = new string[] { ".dll", ".exe" };
            if (validExtensions.All(x => !x.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)))
            {
                Console.Error.WriteLine(string.Format("File Must be an exe or dll: {0}", fileName));
                return (int)Model.Info.ResultCode.InvalidFileType;
            }

            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(fileName);
            var assembly = System.Reflection.Assembly.LoadFrom(fileName);
            var dependencies = Common.Model.Helper.GetDependencies(assembly);

            return OutputDependencies(dependencies, 0);
        }

        private static void ShowHelp()
        {
        }

        private static void ShowHeader()
        {
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine(string.Format("DHGMS AppReliance V{0}", ver));
            Console.WriteLine();
        }

        static int OutputDependencies(List<Common.Model.Info.Dependency> dependencies, int depth)
        {
            var error = false;
            foreach (var dep in dependencies)
            {
                if (dep.Location == Common.Model.Info.Location.NotFound)
                {
                    Console.Error.WriteLine(string.Format("{0}{1} NOT FOUND!", new String('.', depth * 2), dep.AssemblyName.Name));
                    error = true;
                }
                else
                {
                    Console.WriteLine(string.Format("{0}{1}", new String('.', depth * 2), dep.AssemblyName.Name));
                    var subDeps = dep.Dependencies;
                    if (subDeps != null && subDeps.Count > 0)
                    {
                        OutputDependencies(subDeps, ++depth);
                    }
                }
            }

            return error ? (int)Model.Info.ResultCode.DependencyIssue : 0;
        }
    }
}
