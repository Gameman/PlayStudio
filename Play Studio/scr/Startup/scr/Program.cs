using System;

namespace Play.Studio.Startup
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            DebugStartup();
#else
#endif
        }

        static void DebugStartup()
        {
            Workbench.Workbench.Startup();
        }
    }
}
