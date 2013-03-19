using System;
using System.Windows;

namespace Play.Studio.View
{
    /// <summary>
    /// 窗体类型
    /// </summary>
    public enum ViewShowType 
    {
        Float,
        Fixed
    }

    /// <summary>
    /// 观察层显示位置
    /// </summary>
    [Flags]
    public enum ViewShowStrategy
    {
        Most = 1,
        Left = 2,
        Right = 4,
        Top = 16,
        Bottom = 32
    }

    /// <summary>
    /// 观察层接口
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// 显示类型
        /// </summary>
        ViewShowType        ShowType        { get; }

        /// <summary>
        /// 显示策略
        /// </summary>
        ViewShowStrategy    ShowStrategy    { get; }

        /// <summary>
        /// 标题
        /// </summary>
        string              Title           { get; }

        /// <summary>
        /// 浮动大小
        /// </summary>
        Size                FloatSize       { get; }

        /// <summary>
        /// 当关闭后
        /// </summary>
        event EventHandler OnClosed;
    }
}
