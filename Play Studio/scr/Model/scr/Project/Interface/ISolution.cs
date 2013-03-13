namespace Play.Studio.Module.Project
{
    /// <summary>
    /// 解决方案接口
    /// </summary>
    public interface ISolution : IProjectDirectory
    {
        bool AddProject(IProject project);
        bool RemoveProject(IProject project);

        bool Load(string fileName);
        bool Save(string fileName);
        bool Close(string fileName);
    }
}
