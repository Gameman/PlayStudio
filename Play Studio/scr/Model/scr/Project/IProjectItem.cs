using System;

namespace Play.Studio.Module.Project
{
    public interface IProjectItem : ICloneable, IDisposable
    {
        ProjectItemType         ItemType        { get; }

        bool                    Exists          { get; }
        bool                    Builded         { get; }
        IProject                Project         { get; set; }

        string                  Include         { get; set; }
        string                  Name            { get; }
        string                  FileName        { get; set; }
        string                  Extension       { get; }
        object                  SyncRoot        { get; }

        IProjectItem            CloneFor(IProject targetProject);
    }
}
