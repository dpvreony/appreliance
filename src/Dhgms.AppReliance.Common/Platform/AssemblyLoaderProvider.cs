namespace Dhgms.AppReliance.Common.Platform
{
    public static class AssemblyLoaderProvider
    {
        // Implementation class AssemblyLoader is provided per-targetframework in
        // AssemblyLoader.net10.cs and AssemblyLoader.net481.cs
        public static readonly IAssemblyLoader Instance = new AssemblyLoader();
    }
}
