using System.Collections.Generic;

namespace Play.Studio.Module.Project
{
    /// <summary>
    /// 项目项类型
    /// </summary>
    public sealed class ProjectItemType
    {
        // file
        public static ProjectItemType   None                = new ProjectItemType("None", ProjectItemBuildType.None);
        public static ProjectItemType   Compile             = new ProjectItemType("Compile", ProjectItemBuildType.Compile);
        public static ProjectItemType   Content             = new ProjectItemType("Content", ProjectItemBuildType.Content);
        public static ProjectItemType   EmbeddedResource    = new ProjectItemType("EmbeddedResource", ProjectItemBuildType.EmbeddedResource);

        // directory
        public static ProjectItemType   Folder              = new ProjectItemType("Folder", ProjectItemBuildType.Folder);
        public static ProjectItemType   ReferneceFolder     = new ProjectItemType("ReferneceFolder", ProjectItemBuildType.ReferneceFolder);
        public static ProjectItemType   Project             = new ProjectItemType("Project", ProjectItemBuildType.Project);
        public static ProjectItemType   Solution            = new ProjectItemType("Solution", ProjectItemBuildType.Solution);

        // reference
        public static ProjectItemType   GacReference        = new ProjectItemType("GacReference", ProjectItemBuildType.GacReference);
        public static ProjectItemType   FileReference       = new ProjectItemType("FileReference", ProjectItemBuildType.FileReference);
        public static ProjectItemType   WebReference        = new ProjectItemType("WebReference", ProjectItemBuildType.WebReference);
        public static ProjectItemType   ProjectReference    = new ProjectItemType("ProjectReference", ProjectItemBuildType.ProjectReference);

        /// <summary>
        /// 文件类型合集
        /// </summary>
        public static ReadOnlyCollectionWrapper<ProjectItemType> Files = new ReadOnlyCollectionWrapper<ProjectItemType>(new ProjectItemType[4] 
        {
            None,
            Compile,
            Content,
            EmbeddedResource
        });

        /// <summary>
        /// 目录类型合集
        /// </summary>
        public static ReadOnlyCollectionWrapper<ProjectItemType> Folders = new ReadOnlyCollectionWrapper<ProjectItemType>(new ProjectItemType[4] 
        {
            Folder,
            ReferneceFolder,
            Project,
            Solution
        });

        /// <summary>
        /// 引用类型合集
        /// </summary>
        public static ReadOnlyCollectionWrapper<ProjectItemType> References = new ReadOnlyCollectionWrapper<ProjectItemType>(new ProjectItemType[4] 
        {
            GacReference,
            FileReference,
            WebReference,
            ProjectReference
        });

        /// <summary>
        /// 获得类型
        /// </summary>
        public static ProjectItemType   GetType(string type)                    
        {
            foreach (var item in Files)
                if (item.Header.Equals(type))
                    return item;

            foreach (var item in Folders)
                if (item.Header.Equals(type))
                    return item;

            foreach (var item in References)
                if (item.Header.Equals(type))
                    return item;

            return None;
            
        }

        public string                   Header      { get; private set; }
        public ProjectItemBuildType     BuildType   { get; private set; }

        private ProjectItemType(string header, ProjectItemBuildType buildType) 
        {
            Header      = header;
            BuildType   = buildType;
        }
    }
}
