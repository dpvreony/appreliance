namespace Dhgms.AppReliance.Common.Platform
{
    public static class AssemblyInspectorProvider
    {
        // Implementation AssemblyInspector is provided per-targetframework
        public static readonly IAssemblyInspector Instance = new AssemblyInspector();
    }
}
