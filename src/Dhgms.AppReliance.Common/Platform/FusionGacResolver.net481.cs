using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Dhgms.AppReliance.Common.Platform
{
    internal static class FusionGacResolver
    {
        private static readonly Microsoft.Extensions.Logging.ILogger _logger = AssemblyLoaderProvider.LoggerFactory.CreateLogger("FusionGacResolver.net481");

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ASSEMBLY_INFO
        {
            public int cbAssemblyInfo;
            public int dwAssemblyFlags;
            public long uliAssemblySizeInKB;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCurrentAssemblyPathBuf;
            public int cchBuf;
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        private interface IAssemblyCache
        {
            [PreserveSig]
            int Unused0();

            [PreserveSig]
            int QueryAssemblyInfo(int flags, [MarshalAs(UnmanagedType.LPWStr)] string assemblyName, ref ASSEMBLY_INFO assemblyInfo);

            // Other methods not required
        }

        [DllImport("fusion.dll")]
        private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);

        public static bool TryFindInGac(AssemblyName name, out string path)
        {
            path = null;
            if (name == null) return false;

            try
            {
                var hr = CreateAssemblyCache(out var cache, 0);
                if (hr != 0 || cache == null)
                {
                    return false;
                }

                var displayName = name.FullName;

                var info = new ASSEMBLY_INFO();
                info.cchBuf = 260;
                info.pszCurrentAssemblyPathBuf = new string('\0', info.cchBuf);
                info.cbAssemblyInfo = Marshal.SizeOf(typeof(ASSEMBLY_INFO));

                var res = cache.QueryAssemblyInfo(0, displayName, ref info);
                if (res == 0)
                {
                    var p = info.pszCurrentAssemblyPathBuf.TrimEnd('\0');
                    _logger.LogDebug("Fusion located {Name} -> {Path}", displayName, p);
                    path = p;
                    return true;
                }
            }
            catch
            {
                _logger.LogDebug("Fusion GAC lookup failed for {Name}", name);
            }

            return false;
        }
    }
}
