namespace Play.Studio.Module.Templates
{
    /// <summary>
    /// 模板文件
    /// </summary>
    public struct ProjectTemplateFile
    {
        /// <summary>
        /// 完整路径名
        /// </summary>
        public string FullName  { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public string Language  { get; set; }

        /// <summary>
        /// 模板文件内容
        /// </summary>
        public string Content   { get; set; }
    }
}
