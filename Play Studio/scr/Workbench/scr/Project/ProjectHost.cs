using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using Play.Studio.Module.Project;

namespace Play.Studio.Workbench.Project
{
    /// <summary>
    /// 项目委托
    /// </summary>
    /// <param name="project"></param>
    public delegate void ProjectDelegate(IProject project);

    /// <summary>
    /// 项目物体委托
    /// </summary>
    /// <param name="projectItem"></param>
    public delegate void ProjectItemDelegate(IProjectItem projectItem);

    /// <summary>
    /// 项目host异常
    /// </summary>
    public class ProjectHostException : Exception                                               
    {
        public ProjectHostException()
            : base()
        {
        }

        public ProjectHostException(Type serviceType)
            : base("Required service not found: " + serviceType.FullName)
        {
        }

        public ProjectHostException(string message)
            : base(message)
        {
        }

        public ProjectHostException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ProjectHostException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// 项目服务
    /// </summary>
    public static class ProjectHost
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
        /// 返回目标类型的项目资产
        /// </summary>
        public static IEnumerable<IProjectItem> GetItemsOfType(ProjectItemType type)            
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
            string folderName = ProjectHost.CurrentProject.Root;

            if (!ProjectItemType.References.Contains(item.ItemType))
            {
                var copiedFileName = Path.Combine(folderName, Path.GetFileName(item.FileName));
            }

            if (!AppendItem(item, folderName, performMove))
            {
                throw new ProjectHostException("添加文件到项目失败.");
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
                        FileService.RenameFile(item.FileName, copiedFileName, item is IProjectFolder);
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
            if (FileService.RenameFile(oName, nName, item is IProjectFolder))
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
        public static bool MoveItem(IProjectItem item, IProjectFolder directory)                
        {
            string newName = Path.Combine(directory.FileName, item.Name);

            if (FileService.RenameFile(item.FileName, newName, item is IProjectFolder))
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
                foreach (var item in project.Items)
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
                foreach (var item in project.Items)
                    if (item.Include.Equals(include))
                        return item;
            }

            return null;
        }

        /// <summary>
        /// 创建新项目
        /// </summary>
        public static Project New()                                                             
        {
            return new Project();
        }

        /// <summary>
        /// 加载项目
        /// </summary>
        /// <param name="fileName"></param>
        public static IProject Load(string fileName)                                            
        {
            if (ProjectHost.CurrentProject != null)
            {
                if (!ProjectHost.UnLoad()) 
                {
                    return ProjectHost.CurrentProject;
                }
            }

            // 加载项目
            try
            {
                CurrentProject = LoadItem(XElement.Load(fileName)) as IProject;

                if (CurrentProjectChanged != null)
                    CurrentProjectChanged(CurrentProject);

            }
            catch (Exception ex) 
            {
                throw new ProjectHostException(string.Format("项目文件'{0}'读取失败", fileName));
            }

            return CurrentProject;
        }

        /// <summary>
        /// 卸载项目
        /// </summary>
        public static bool UnLoad()                                                             
        {
            return true;
        }

        /// <summary>
        /// 储存项目
        /// </summary>
        public static void Save(string fileName)                                                
        {
            try
            {
                XmlDocument xmlDoc          = new XmlDocument();
                var project                 = xmlDoc.CreateElement("Project");

                SaveItem(CurrentProject, project); 

                xmlDoc.Save(fileName);
            }
            catch (Exception ex) 
            {
                throw new ProjectHostException("项目储存失败");
            }
        }

        /// <summary>
        /// 保存项目项
        /// </summary>
        /// <param name="item"></param>
        public static void SaveItem(IProjectItem item, XmlElement pre)                          
        {
            var itemElement     = pre.OwnerDocument.CreateElement(item.ItemType.Header);
            var itemAttribute   = pre.OwnerDocument.CreateAttribute("Include");
            itemAttribute.Value = item.Include;
            itemElement.AppendChild(itemAttribute);

            if(ProjectItemType.Folders.Contains(item.ItemType))
            {
                foreach (var subItem in (item as IProjectFolder).Items) 
                {
                    SaveItem(subItem, itemElement);
                }
            }

            if (pre != null)
                pre.AppendChild(itemElement);
        }

        /// <summary>
        /// 项目文件项
        /// </summary>
        public static IProjectItem LoadItem(XElement xel)                                       
        {
            ProjectItemType itemType = ProjectItemType.GetType(xel.Name.LocalName);
            ProjectItem item = null;

            if (ProjectItemType.Files.Contains(itemType))
            {
                item = new ProjectFile() { m_itemType = itemType };
            }
            else if (ProjectItemType.References.Contains(itemType)) 
            {
                item = new ProjectReference() { m_itemType = itemType };
            }
            else if (ProjectItemType.Folders.Contains(itemType)) 
            {
                if (itemType == ProjectItemType.Project)
                {
                    item = new Project();
                }
                else if (itemType == ProjectItemType.Folder)
                {
                    item = new ProjectFolder();
                }
                else if (itemType == ProjectItemType.ReferneceFolder)
                {
                    item = new ProjectReferenceFolder();
                }

                foreach (var subXel in xel.Elements()) 
                {
                    (item as ProjectFolder).Items.Add(LoadItem(subXel));
                }
            }
            else
            {
                item = new ProjectFile() { m_itemType = itemType };
            }

            return item;
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
