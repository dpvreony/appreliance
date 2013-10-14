﻿namespace Dhgms.AppReliance.Cmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dhgms.AppReliance.Common.Model.Info;

    /// <summary>
    /// The program entry point logic.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">
        /// Command line arguments.
        /// </param>
        /// <returns>
        /// Program Result Code.
        /// </returns>
        public static int Main(string[] args)
        {
            ShowHeader();

            if (args.Length < 1)
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
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Filename not valid");
            }

            if (!System.IO.File.Exists(fileName))
            {
                Console.Error.WriteLine("File Not Found: {0}", fileName);
                return (int)Model.Info.ResultCode.FileNotFound;
            }

            var fileExtension = System.IO.Path.GetExtension(fileName);
            var validExtensions = new[] { ".dll", ".exe" };
            if (validExtensions.All(x => !x.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)))
            {
                Console.Error.WriteLine("File Must be an exe or dll: {0}", fileName);
                return (int)Model.Info.ResultCode.InvalidFileType;
            }

            var directoryName = System.IO.Path.GetDirectoryName(fileName);
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                throw new InvalidOperationException("Unable to work out the directory name");
            }

            Environment.CurrentDirectory = directoryName;
            var assembly = System.Reflection.Assembly.LoadFrom(fileName);
            var dependencies = Common.Model.Helper.GetDependencies(assembly);

            return OutputDependencies(dependencies, 0);
        }

        private static void ShowHelp()
        {
        }

        private static void ShowHeader()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.Out.WriteLine("AppReliance " + version + " (http://wsussmartapprove.codeplex.com)");
            Console.Out.WriteLine("(C)Copyright 2013 DHGMS Solutions. Some Rights Reserved.\n");
        }

        private static int OutputDependencies(IEnumerable<Dependency> dependencies, int depth)
        {
            var error = false;
            foreach (var dep in dependencies)
            {
                if (dep.Location == Common.Model.Info.Location.NotFound)
                {
                    Console.Error.WriteLine("{0}{1} NOT FOUND!", new string('.', depth * 2), dep.AssemblyName.Name);
                    error = true;
                }
                else
                {
                    Console.WriteLine("{0}{1}", new string('.', depth * 2), dep.AssemblyName.Name);
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
