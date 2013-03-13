using System.ComponentModel;

namespace Play.Studio.Module.Project
{
    public enum ProjectItemType
    {
        [Description("None")]
        None                            = 0,
        [Description("Reference")]
        Reference                       = 1,
        [Description("Compile")]
        Compile                         = 2,
        [Description("Content")]
        Content                         = 3,
        [Description("Directory")]
        Directory                       = 4,
    }
}
