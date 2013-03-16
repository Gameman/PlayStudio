using System.ComponentModel;

namespace Play.Studio.Module.Project
{
    /// <summary>
    /// 项目项生成类型
    /// </summary>
    public enum ProjectItemBuildType 
    {
        [Description("None")]
        None                            = 0,

        [Description("Compile")]
        Compile                         = 1,

        [Description("Content")]
        Content                         = 2,

        [Description("EmbeddedResource")]
        EmbeddedResource                = 3,

        [Description("Folder")]
        Folder                          = 4,

        [Description("Folder")]
        ReferneceFolder                 = 5,

        [Description("Project")]
        Project                         = 6,

        [Description("Solution")]
        Solution                        = 7,

        [Description("GacReference")]
        GacReference                    = 8,

        [Description("FileReference")]
        FileReference                   = 9,

        [Description("WebReference")]
        WebReference                    = 10,

        [Description("ProjectReference")]
        ProjectReference                = 11

    }
}
