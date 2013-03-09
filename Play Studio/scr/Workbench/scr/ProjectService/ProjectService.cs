namespace Play.Studio.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Play.Studio.Core.Command.Project;

    public delegate void ProjectDelegate(IProject project);
    public delegate void ProjectItemDelegate(IProjectItem projectItem);

    /// <summary>
    /// 项目服务
    /// </summary>
    public static class ProjectService
    {
        static string   _userConfig;

        /// <summary>
        /// 获取当前项目
        /// </summary>
        public static IProject CurrentProject
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取或设置项目类型
        /// </summary>
        internal static Type ProjectType                
        {
            get;
            set;
        }

        /// <summary>
        /// 返回目标类型的项目资产
        /// </summary>
        public static IEnumerable<IProjectItem> GetItemsOfType(ItemType type) 
        {
            if (CurrentProject != null)
            {
                foreach (var item in CurrentProject.GetItemsOfType(type)) 
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 添加项到项目
        /// </summary>
        public static bool AppendItem(IProjectItem item) 
        {
            return AppendItem(item, false);            
        }

        /// <summary>
        /// 添加项到项目
        /// </summary>
        public static bool AppendItem(IProjectItem item, bool performMove) 
        {
            string folderName = ProjectService.CurrentProject.Root;
            

            if (item.ItemType != ItemType.Reference) 
            {
                var copiedFileName = Path.Combine(folderName, Path.GetFileName(item.FileName));
            }

            if (!AppendItem(item, folderName, performMove)) 
            {
                //throw new ProjectServiceException("添加文件到项目失败.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 应用项到项目
        /// </summary>
        public static bool AppendItem(IProjectItem item, string folderName, bool performMove)
        {
            try
            {
                // Path.GetDirectoryName(item.FileName) == folderName; 
                bool originalPath = FileService.IsEqualFileName(folderName, Path.GetDirectoryName(item.FileName));
                string copiedFileName = originalPath ? item.FileName : Path.Combine(folderName, Path.GetFileName(item.FileName));
                // file  dose not exsit in project
                if (!originalPath)
                {
                    if (performMove)
                    {
                        FileService.RenameFile(item.FileName, copiedFileName, item is IDirectoryItem);
                    }
                    else
                    {
                        // 如果不在文件夹内则新建
                        FileService.CopyFile(item.FileName, copiedFileName, true);
                    }
                }

                item.FileName = copiedFileName;

                if (ProjectItemAdded != null)
                    ProjectItemAdded(item);


                Save(CurrentProject.FileName);

                return true;
            }
            catch (Exception ex) 
            {
#if DEBUG
                throw ex;
#else
                LoggingService.Info(string.Format("file not found '{0}'", item.Name));
                return false;
#endif
            }
        }

        /// <summary>
        /// 移除项从项目
        /// </summary>
        public static bool RemoveItem(IProjectItem projectItem)             
        {
            var item = FindProjectItem(projectItem.FileName);
            if (item != null)
            {
                FileService.RemoveFile(projectItem.FileName, false);

                Save(CurrentProject.FileName);

                if (ProjectItemRemoved != null)
                    ProjectItemRemoved(item);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 重命名项
        /// </summary>
        public static bool RenameItem(IProjectItem item, string newName)    
        {
            string error = FileService.IsValidFileEntryName(newName);
            if (error != null)
            {
                return false;
            }

            string oName = item.FileName;
            string nName = Path.Combine(Path.GetDirectoryName(oName), newName);
            if (FileService.RenameFile(oName, nName, item is IDirectoryItem))
            {
                item.FileName = nName;

                Save(CurrentProject.FileName);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 移动项到文件夹
        /// </summary>
        public static bool MoveItem(IProjectItem item, IDirectoryItem directory) 
        {
            string newName = Path.Combine(directory.FileName, item.Name);

            if (FileService.RenameFile(item.FileName, newName, item is IDirectoryItem))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 查找项
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IProjectItem FindProjectItem(string fileName)
        {
            return FindProjectItem(fileName, CurrentProject);
        }

        /// <summary>
        /// 查找项
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IProjectItem FindProjectItem(string fileName, IProject project)         
        {
            if (project != null)
            {
                foreach (var item in project.ProjectItems)
                    if (!string.IsNullOrEmpty(item.FileName) && item.FileName.Equals(fileName))
                        return item;
            }

            return null;
        }

        /// <summary>
        /// 从include查找项
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IProjectItem FindProjectItemByInclude(string include) 
        {
            return FindProjectItemByInclude(include, CurrentProject);
        }

        /// <summary>
        /// 从include查找项
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IProjectItem FindProjectItemByInclude(string include, IProject project)
        {
            if (project != null)
            {
                foreach (var item in project.ProjectItems)
                    if (item.Include.Equals(include))
                        return item;
            }

            return null;
        }

        /// <summary>
        /// 创建新项目
        /// </summary>
        public static IProject New(Type type)                               
        {
            return TypeService.CreateInstance(type) as IProject;
        }

        /// <summary>
        /// 加载项目
        /// </summary>
        /// <param name="fileName"></param>
        public static IProject Load(string fileName)                        
        {
            if (ProjectService.CurrentProject != null)
            {
                if (!ProjectService.UnLoad()) 
                {
                    return ProjectService.CurrentProject;
                }
            }

            IProject project = New(ProjectType);

            if (project.Load(fileName))
            {
                CurrentProject = project;

                if (CurrentProjectChanged != null)
                    CurrentProjectChanged(project);

                return project;
            }
            else
            {
#if DEBUG
                throw new ProjectServiceException(string.Format("项目文件'{0}'读取失败", fileName));
#else
                return null;
#endif
            }
        }

        /// <summary>
        /// 卸载项目
        /// </summary>
        public static bool UnLoad()                             
        {
            if (!CurrentProject.Unload())
            {
                throw new ProjectServiceException("项目卸载失败");
                return false;
            }
            else 
            {
                return true;
            }
        }

        /// <summary>
        /// 储存项目
        /// </summary>
        public static void Save(string fileName)                
        {
            if (!CurrentProject.Save(fileName)) 
            {
                throw new ProjectServiceException("项目储存失败");
            }
        }

        /// <summary>
        /// 生成项目
        /// </summary>
        public static void Build() 
        {
            if (CurrentProject != null) 
            {
                // TODO 未实现
                throw new NotImplementedException();
            }
        }

        public static event ProjectDelegate     CurrentProjectChanged;
        public static event ProjectItemDelegate ProjectItemAdded;
        public static event ProjectItemDelegate ProjectItemRemoved;
    }
}
