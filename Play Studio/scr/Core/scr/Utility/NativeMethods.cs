using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Play.Studio.Core.Services;

namespace Play.Studio.Core.Utility
{
    /// <summary>
    /// Contains P/Invoke methods for functions in the Windows API.
    /// </summary>
    static class NativeMethods
    {
        static readonly IntPtr FALSE = new IntPtr(0);
        static readonly IntPtr TRUE = new IntPtr(1);

        public const int WM_SETREDRAW = 0x00B;
        public const int WM_USER = 0x400;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        #region SHFileOperation
        enum FO_FUNC : uint
        {
            FO_MOVE = 0x0001,
            FO_COPY = 0x0002,
            FO_DELETE = 0x0003,
            FO_RENAME = 0x0004,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FO_FUNC wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FILEOP_FLAGS fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        [Flags]
        private enum FILEOP_FLAGS : ushort
        {
            None = 0,
            FOF_MULTIDESTFILES = 0x0001,
            FOF_CONFIRMMOUSE = 0x0002,
            FOF_SILENT = 0x0004,  // don't create progress/report
            FOF_RENAMEONCOLLISION = 0x0008,
            FOF_NOCONFIRMATION = 0x0010,  // Don't prompt the user.
            FOF_WANTMAPPINGHANDLE = 0x0020,  // Fill in SHFILEOPSTRUCT.hNameMappings
            // Must be freed using SHFreeNameMappings
            FOF_ALLOWUNDO = 0x0040,
            FOF_FILESONLY = 0x0080,  // on *.*, do only files
            FOF_SIMPLEPROGRESS = 0x0100,  // means don't show names of files
            FOF_NOCONFIRMMKDIR = 0x0200,  // don't confirm making any needed dirs
            FOF_NOERRORUI = 0x0400,  // don't put up error UI
            FOF_NOCOPYSECURITYATTRIBS = 0x0800,  // dont copy NT file Security Attributes
            FOF_NORECURSION = 0x1000,  // don't recurse into directories.
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,  // don't operate on connected elements.
            FOF_WANTNUKEWARNING = 0x4000,  // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            FOF_NORECURSEREPARSE = 0x8000,  // treat reparse points as objects, not containers
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);


        public static void DeleteToRecycleBin(string fileName)
        {
            //DeleteToRecycleBin(PropertyService.Get<IntPtr>("Play.Studio.Core.Services.DeleteToRecycleBin.Handle", IntPtr.Zero), fileName);
        }

        public static void DeleteToRecycleBin(IntPtr windowHandle, string fileName)
        {
            if (!File.Exists(fileName) && !Directory.Exists(fileName))
                throw new FileNotFoundException("File not found.", fileName);
            SHFILEOPSTRUCT info = new SHFILEOPSTRUCT();
            info.hwnd = windowHandle;
            info.wFunc = FO_FUNC.FO_DELETE;
            info.fFlags = FILEOP_FLAGS.FOF_ALLOWUNDO | FILEOP_FLAGS.FOF_NOCONFIRMATION;
            info.lpszProgressTitle = "Delete " + Path.GetFileName(fileName);
            info.pFrom = fileName + "\0"; // pFrom is double-null-terminated
            int result = SHFileOperation(ref info);
            if (result != 0)
                throw new IOException("Could not delete file " + fileName + ". Error " + result, result);
        }
        #endregion

        [DllImport("kernel32.dll")]
        static extern int GetOEMCP();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr handle);

        public static Encoding OemEncoding
        {
            get
            {
                try
                {
                    return Encoding.GetEncoding(GetOEMCP());
                }
                catch (ArgumentException)
                {
                    return Encoding.Default;
                }
                catch (NotSupportedException)
                {
                    return Encoding.Default;
                }
            }
        }

        #region RAM

        /// <summary>
        /// 在指定进程的虚拟地址空间中保留或开辟一段区域..除非MEM_RESET被使用，否则将该内存区域初始化为0.
        /// </summary>
        /// <param name="process">需要在其中分配空间的进程的句柄.这个句柄必须拥有PROCESS_VM_OPERATION访问权限</param>
        /// <param name="pAddress">想要获取的地址区域.一般用NULL自动分配</param>
        /// <param name="size">要分配的内存大小.字节单位.注意实际分 配的内存大小是页内存大小的整数倍</param>
        /// <param name="type">内存分配的类型</param>
        /// <param name="protect">内存页保护</param>
        /// <returns>执行成功就返回分配内存的首地址，失败返回0。</returns>
        [DllImport("kernel32.dll")] //声明API函数
        public static extern int VirtualAllocEx(IntPtr process, int pAddress, int size, int type, int protect);

        /// <summary>
        /// 写入某一进程的内存区域。入口区必须可以访问，否则操作将失败
        /// </summary>
        /// <param name="process">进程句柄</param>
        /// <param name="baseAddress">要写的内存首地址</param>
        /// <param name="buffer">指向要写的数据的指针(数据当前存放地址)。</param>
        /// <param name="nSize">要写入的字节数。</param>
        /// <param name="lpNumberOfBytesWritten">实际数据的长度</param>
        /// <returns>非零表示成功，零表示失败</returns>
        [DllImport("kernel32.dll")]
        public static extern int WriteProcessMemory(IntPtr process, int baseAddress, string buffer, int nSize, int lpNumberOfBytesWritten);

        /// <summary>
        /// 检索指定的动态链接库(DLL)中的输出库函数地址
        /// </summary>
        /// <param name="hModule"> DLL模块句柄 包含此函数的DLL模块的句柄。LoadLibrary或者GetModuleHandle函数可以返回此句柄。</param>
        /// <param name="lpProcName">函数名 包含函数名的以NULL结尾的字符串，或者指定函数的序数值。如果此参数是一个序数值，它必须在一个字的底字节，高字节必须为0。</param>  
        /// <returns>调用成功，返回DLL中的输出函数地址，调用失败，返回0。得到进一步的错误信息，调用函数GetLastError。</returns>
        [DllImport("kernel32.dll")]
        public static extern int GetProcAddress(int hModule, string lpProcName);

        /// <summary>
        /// 获取一个应用程序或动态链接库的模块句柄
        /// </summary>
        /// <param name="moduleName">指定模块名，这通常是与模块的文件名相同的一个名字</param>
        /// <returns>如执行成功成功，则返回模块句柄。零表示失败</returns>
        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandleA(string moduleName);

        /// <summary>
        ///  创建一个在其它进程地址空间中运行的线程(也称:创建远程线程).
        /// </summary>
        /// <param name="process">目标进程的句柄</param>
        /// <param name="threadAttributes">指向线程的安全描述结构体的指针，一般设置为0，表示使用默认的安全级别</param>
        /// <param name="stackSize">线程堆栈大小，一般设置为0，表示使用默认的大小，一般为1M</param>
        /// <param name="startAddress">线程函数的地址</param>
        /// <param name="parameter">传给线程函数的参数</param>
        /// <param name="creationFlags">线程的创建方式(0表示线程创建后﻿立即运行 CREATE_SUSPENDED 0x00000004以挂起方式创建 创建不会运行，直到调用 ResumeThread函数)</param>
        /// <param name="threadid">指向所创建线程句柄的指针,如果创建失败,该参数为0</param>
        /// <returns>如果调用成功,返回新线程句柄,失败返回0</returns>
        [DllImport("kernel32.dll")]
        public static extern int CreateRemoteThread(IntPtr process, int threadAttributes, int stackSize, int startAddress, int parameter, int creationFlags, int threadid);

        /*
        /// <summary>
        /// 根据进程名称获取进程
        /// </summary>
        /// <param name="ProcessName">进程名称</param>
        /// <returns></returns>
        public Process GetProcessByName(string ProcessName)
        {
            Process[] pname = Process.GetProcesses(); //取得所有进程
            foreach (Process name in pname) //遍历进程
            {
                //如果查找到进程名称 返回
                if (name.ProcessName.ToLower().IndexOf(ProcessName) != -1) return name;
            }
            return null;
        }

        public void RamTest()
        {
            string dllName = "c://text.dll";
            int dlllength = dllName.Length + 1;

            //这里以记事本为例   
            Process processName = GetProcessByName("notepad");
            //如果查找到记事本进程，那么下面开始注入   
            if (processName != null)
            {
                //申请内存空间,执行成功就返回分配内存的首地址，不成功就是0。   
                int baseaddress = VirtualAllocEx(processName.Handle, 0, dlllength, 4096, 4);

                if (baseaddress == 0)
                {
                    MessageBox.Show("申请内存空间失败！");
                    return;
                }

                //写内存   
                int result = WriteProcessMemory(processName.Handle, baseaddress, dllName, dlllength, 0);

                if (result == 0)
                {
                    MessageBox.Show("写内存失败！");
                    return;
                }

                //取得loadlibarary在kernek32.dll地址   
                int procAddress = GetProcAddress(GetModuleHandleA("Kernel32"), "LoadLibraryA");

                if (procAddress == 0)
                {
                    MessageBox.Show("无法取得函数的入口点！");
                    return;
                }

                //创建远程线程。   
                result = CreateRemoteThread(processName.Handle, 0, 0, 0, baseaddress, 0, 0);

                if (result == 0)
                {
                    MessageBox.Show("创建远程线程失败！");
                    return;
                }

                else
                    MessageBox.Show("已成功注入dll!");

            }

        }
        */

        #endregion
    }
}
