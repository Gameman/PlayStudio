using System.IO;
using System.Reflection;
using Play.Studio.Core.Utility;

namespace Play.Studio.Core
{
    public class AssemblyResource : Resource<Assembly, AssemblyResource>
    {
        protected internal override Assembly OnRead(Stream stream)
        {
            return Assembly.Load(stream.ToBytes());
        }
    }
}
