using System.IO;

namespace Play.Studio.Module.Addins
{
    class AddinResource : Resource.Resource
    {
        protected internal override object OnRead(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        protected internal override void OnSave(Stream stream, params object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}
