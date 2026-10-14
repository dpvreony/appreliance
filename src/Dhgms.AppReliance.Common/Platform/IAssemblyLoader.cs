using System.Reflection;

namespace Dhgms.AppReliance.Common.Platform
{
    public interface IAssemblyLoader
    {
        Assembly? LoadFromPath(string path);
        Assembly? LoadByFullName(string assemblyFullName);
        Assembly? LoadFromFileName(string fileName);
    }
}
