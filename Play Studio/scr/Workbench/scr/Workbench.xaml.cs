﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Play.Studio.Core;
using Play.Studio.Core.Logging;
using Play.Studio.Module.Resource;
using Play.Studio.Module.Templates;
using Play.Studio.Workbench.Utility;
using Play.Studio.Core.Services;

namespace Play.Studio.Workbench
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    [AopDefine]
    public partial class Workbench : Window
    {
        public Workbench()
        {
            Current = this;


            Resource.Register<MenuTemplate>(".uit");


            CommandCenter.Initialization();

            InitializeComponent();
        }

        #region Show Window

        public void ShowWindow(Type type) 
        {
            // 查找到layout中是否存在
            var layout = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(X => X.Content != null && X.Content.GetType().Equals(type));
            if (layout == null)
            {
                var window = TypeService.CreateInstance(type) as Control;

                layout = new LayoutAnchorable();
                layout.Content  = window;
                layout.Title    = window.Tag.ToString();

              
                layout.AddToLayout(dockManager, AnchorableShowStrategy.Left);
            }

            // 将layout添加到root中
            layout.Float();
        }

        #endregion

        #region Header Methods

        [Logging(AopTypes.Prefixed, LoggingType.Info, "shuai!")]
        public void OnWindowMin(object sender, EventArgs e) 
        {
            var obj = Resource.Read(@"Play.Studio.Workbench;Properties.Resources->test");

            WindowState = System.Windows.WindowState.Minimized;
        }

        public void OnWindowMax(object sender, EventArgs e) 
        {
            // 检查当前状态
            switch (WindowState)
            {
                case System.Windows.WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    thumb.Visibility = Visibility.Visible;
                    break;
                case System.Windows.WindowState.Minimized:
                case System.Windows.WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    thumb.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void OnWindowClose(object sender, EventArgs e) 
        {
            Close(); 
        }

        #endregion

        #region Thumb Methods

        private double HeadWidth { get { return this.Width - WindowBuutonsPlaceholder.Width; } }
        private void Header_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)                         
        {
            if (e.LeftButton == MouseButtonState.Pressed && WindowState == WindowState.Normal)
                this.DragMove();
        }
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)        
        {
            if (WindowState == WindowState.Normal)
            {
                if (this.Width + e.HorizontalChange > 10)
                    this.Width += e.HorizontalChange;
                if (this.Height + e.VerticalChange > 10)
                    this.Height += e.VerticalChange;
            }
        }

        #endregion

        #region TestTimer

        /// <summary>
        /// TestTimer Dependency Property
        /// </summary>
        public static readonly DependencyProperty TestTimerProperty =
            DependencyProperty.Register("TestTimer", typeof(int), typeof(Workbench),
                new FrameworkPropertyMetadata((int)0));

        /// <summary>
        /// Gets or sets the TestTimer property.  This dependency property 
        /// indicates a test timer that elapses evry one second (just for binding test).
        /// </summary>
        public int TestTimer
        {
            get { return (int)GetValue(TestTimerProperty); }
            set { SetValue(TestTimerProperty, value); }
        }

        #endregion

        #region TestBackground

        /// <summary>
        /// TestBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty TestBackgroundProperty =
            DependencyProperty.Register("TestBackground", typeof(Brush), typeof(Workbench),
                new FrameworkPropertyMetadata((Brush)null));

        /// <summary>
        /// Gets or sets the TestBackground property.  This dependency property 
        /// indicates a randomly changing brush (just for testing).
        /// </summary>
        public Brush TestBackground
        {
            get { return (Brush)GetValue(TestBackgroundProperty); }
            set { SetValue(TestBackgroundProperty, value); }
        }

        #endregion

        #region FocusedElement

        /// <summary>
        /// FocusedElement Dependency Property
        /// </summary>
        public static readonly DependencyProperty FocusedElementProperty =
            DependencyProperty.Register("FocusedElement", typeof(string), typeof(Workbench),
                new FrameworkPropertyMetadata((IInputElement)null));

        /// <summary>
        /// Gets or sets the FocusedElement property.  This dependency property 
        /// indicates ....
        /// </summary>
        public string FocusedElement
        {
            get { return (string)GetValue(FocusedElementProperty); }
            set { SetValue(FocusedElementProperty, value); }
        }

        #endregion

        private void OnLayoutRootPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)   
        {
            var activeContent = ((LayoutRoot)sender).ActiveContent;
            if (e.PropertyName == "ActiveContent")
            {
                Debug.WriteLine(string.Format("ActiveContent-> {0}", activeContent));
            }
        }

        private void OnLoadLayout(object sender, RoutedEventArgs e)                                                 
        {
            var currentContentsList = dockManager.Layout.Descendents().OfType<LayoutContent>().Where(c => c.ContentId != null).ToArray();

            string fileName = (sender as MenuItem).Header.ToString();
            var serializer = new XmlLayoutSerializer(dockManager);
            //serializer.LayoutSerializationCallback += (s, args) =>
            //    {
            //        var prevContent = currentContentsList.FirstOrDefault(c => c.ContentId == args.Model.ContentId);
            //        if (prevContent != null)
            //            args.Content = prevContent.Content;
            //    };
            using (var stream = new StreamReader(string.Format(@".\AvalonDock_{0}.config", fileName)))
                serializer.Deserialize(stream);
        }

        private void OnSaveLayout(object sender, RoutedEventArgs e)                                                 
        {
            string fileName = (sender as MenuItem).Header.ToString();
            var serializer = new XmlLayoutSerializer(dockManager);
            using (var stream = new StreamWriter(string.Format(@".\AvalonDock_{0}.config", fileName)))
                serializer.Serialize(stream);
        }

        private void OnShowWinformsWindow(object sender, RoutedEventArgs e)                                         
        {
            var winFormsWindow = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "WinFormsWindow");
            if (winFormsWindow.IsHidden)
                winFormsWindow.Show();
            else if (winFormsWindow.IsVisible)
                winFormsWindow.IsActive = true;
            else
                winFormsWindow.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
        }

        private void AddTwoDocuments_click(object sender, RoutedEventArgs e)                                        
        {
            var firstDocumentPane = dockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            if (firstDocumentPane != null)
            {
                LayoutDocument doc = new LayoutDocument();
                doc.Title = "Test1";
                firstDocumentPane.Children.Add(doc);

                LayoutDocument doc2 = new LayoutDocument();
                doc2.Title = "Test2";
                firstDocumentPane.Children.Add(doc2);
            }

            var leftAnchorGroup = dockManager.Layout.LeftSide.Children.FirstOrDefault();
            if (leftAnchorGroup == null)
            {
                leftAnchorGroup = new LayoutAnchorGroup();
                dockManager.Layout.LeftSide.Children.Add(leftAnchorGroup);
            }

            leftAnchorGroup.Children.Add(new LayoutAnchorable() { Title = "New Anchorable" });

        }

        private void OnShowToolWindow1(object sender, RoutedEventArgs e)                                            
        {
            var toolWindow1 = dockManager.Layout.Descendents().OfType<LayoutAnchorable>().Single(a => a.ContentId == "toolWindow1");
            if (toolWindow1.IsHidden)
                toolWindow1.Show();
            else if (toolWindow1.IsVisible)
                toolWindow1.IsActive = true;
            else
                toolWindow1.AddToLayout(dockManager, AnchorableShowStrategy.Bottom | AnchorableShowStrategy.Most);
        }

        private void dockManager_DocumentClosing(object sender, DocumentClosingEventArgs e)                         
        {
            if (MessageBox.Show("Are you sure you want to close the document?", "AvalonDock Sample", MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true;
        }

        private void OnDumpToConsole(object sender, RoutedEventArgs e)                                              
        {
#if DEBUG
            //dockManager.Layout.ConsoleDump(0);
#endif
        }

        private void OnReloadManager(object sender, RoutedEventArgs e)                                              
        {
        }

        private void OnUnloadManager(object sender, RoutedEventArgs e)                                              
        {
            if (layoutRoot.Children.Contains(dockManager))
                layoutRoot.Children.Remove(dockManager);
        }

        private void OnLoadManager(object sender, RoutedEventArgs e)                                                
        {
            if (!layoutRoot.Children.Contains(dockManager))
                layoutRoot.Children.Add(dockManager);
        }

        private void OnToolWindow1Hiding(object sender, System.ComponentModel.CancelEventArgs e)                    
        {
            if (MessageBox.Show("Are you sure you want to hide this tool?", "AvalonDock", MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true;
        }


        #region Static 

        public static void Startup() 
        {
            App app = new App();
            app.Run(Current = new Workbench());
        }

        public static Workbench Current { get; private set; }

        #endregion
    }
}
