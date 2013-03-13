namespace Play.Studio.Module.Addins
{
    /// <summary>
    /// 插件状态
    /// </summary>
    public enum AddinState
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow = 0x00,

        /// <summary>
        /// 已载入
        /// </summary>
        Loaded = 0x01,

        /// <summary>
        /// 关闭的
        /// </summary>
        Closed = 0x02
    }
}
