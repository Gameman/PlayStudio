using System;
using System.Diagnostics;
using System.IO;

namespace Play.Studio.AOP
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                AOP.Resolve(@"..\..\bin\Debug");
                Console.ReadLine();
            }
            else 
            {
                try
                {
                    AOP.Resolve(@"..\..\bin\Debug");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
