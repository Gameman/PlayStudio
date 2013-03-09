using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Play.Studio.Core.Utility;

namespace Play.Studio.Core.Services
{
	public enum FileErrorPolicy {
		Inform,
		ProvideAlternative
	}
	
	public enum FileOperationResult {
		OK,
		Failed,
		SavedAlternatively
	}
	
	public delegate void FileOperationDelegate();
	
	public delegate void NamedFileOperationDelegate(string fileName);
	
	/// <summary>
	/// A utility class related to file utilities.
	/// </summary>
	public static partial class FileService
	{
		readonly static char[] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };
		static string applicationRootPath = AppDomain.CurrentDomain.BaseDirectory;
		const string fileNameRegEx = @"^([a-zA-Z]:)?[^:]+$";
		
		public static string ApplicationRootPath {
			get {
				return applicationRootPath;
			}
			set {
				applicationRootPath = value;
			}
		}

        public static string DataRootPath 
        {
            get 
            {
                return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ApplicationRootPath))), "data");
            }
        }

        public static string TemplatePath 
        {
            get 
            {
                return Path.Combine(DataRootPath, "templates");
            }
        }

        public static string CacheRootPath 
        {
            get {
                string path = applicationRootPath + "\\Cahce\\";
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }
		
		static string GetPathFromRegistry(string key, string valueName)
		{
			using (RegistryKey installRootKey = Registry.LocalMachine.OpenSubKey(key)) {
				if (installRootKey != null) {
					object o = installRootKey.GetValue(valueName);
					if (o != null) {
						string r = o.ToString();
						if (!string.IsNullOrEmpty(r))
							return r;
					}
				}
			}
			return null;
		}
		
		#region InstallRoot Properties
		
		static string netFrameworkInstallRoot = null;
		/// <summary>
		/// Gets the installation root of the .NET Framework (@"C:\Windows\Microsoft.NET\Framework\")
		/// </summary>
		public static string NetFrameworkInstallRoot {
			get {
				if (netFrameworkInstallRoot == null) {
					netFrameworkInstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\.NETFramework", "InstallRoot") ?? string.Empty;
				}
				return netFrameworkInstallRoot;
			}
		}
		
		static string netSdk20InstallRoot = null;
		/// <summary>
		/// Location of the .NET 2.0 SDK install root.
		/// </summary>
		public static string NetSdk20InstallRoot {
			get {
				if (netSdk20InstallRoot == null) {
					netSdk20InstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\.NETFramework", "sdkInstallRootv2.0") ?? string.Empty;
				}
				return netSdk20InstallRoot;
			}
		}
		
		static string windowsSdk60InstallRoot = null;
		/// <summary>
		/// Location of the .NET 3.0 SDK (Windows SDK 6.0) install root.
		/// </summary>
		public static string WindowsSdk60InstallRoot {
			get {
				if (windowsSdk60InstallRoot == null) {
					windowsSdk60InstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v6.0", "InstallationFolder") ?? string.Empty;
				}
				return windowsSdk60InstallRoot;
			}
		}
		
		static string windowsSdk60aInstallRoot = null;
		/// <summary>
		/// Location of the Windows SDK Components in Visual Studio 2008 (.NET 3.5; Windows SDK 6.0a).
		/// </summary>
		public static string WindowsSdk60aInstallRoot {
			get {
				if (windowsSdk60aInstallRoot == null) {
					windowsSdk60aInstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v6.0a", "InstallationFolder") ?? string.Empty;
				}
				return windowsSdk60aInstallRoot;
			}
		}
		
		static string windowsSdk61InstallRoot = null;
		/// <summary>
		/// Location of the .NET 3.5 SDK (Windows SDK 6.1) install root.
		/// </summary>
		public static string WindowsSdk61InstallRoot {
			get {
				if (windowsSdk61InstallRoot == null) {
					windowsSdk61InstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v6.1", "InstallationFolder") ?? string.Empty;
				}
				return windowsSdk61InstallRoot;
			}
		}
		
		static string windowsSdk70InstallRoot = null;
		/// <summary>
		/// Location of the .NET 3.5 SP1 SDK (Windows SDK 7.0) install root.
		/// </summary>
		public static string WindowsSdk70InstallRoot {
			get {
				if (windowsSdk70InstallRoot == null) {
					windowsSdk70InstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0", "InstallationFolder") ?? string.Empty;
				}
				return windowsSdk70InstallRoot;
			}
		}
		
		static string windowsSdk71InstallRoot = null;
		/// <summary>
		/// Location of the .NET 4.0 SDK (Windows SDK 7.1) install root.
		/// </summary>
		public static string WindowsSdk71InstallRoot {
			get {
				if (windowsSdk71InstallRoot == null) {
					windowsSdk71InstallRoot = GetPathFromRegistry(@"SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.1", "InstallationFolder") ?? string.Empty;
				}
				return windowsSdk71InstallRoot;
			}
		}
		
		#endregion
		
		[Obsolete("Use System.IO.Path.Combine instead")]
		public static string Combine(params string[] paths)
		{
			if (paths == null || paths.Length == 0) {
				return String.Empty;
			}
			
			string result = paths[0];
			for (int i = 1; i < paths.Length; i++) {
				result = Path.Combine(result, paths[i]);
			}
			return result;
		}
		
		public static bool IsUrl(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			return path.IndexOf("://", StringComparison.Ordinal) > 0;
		}
		
		public static string GetCommonBaseDirectory(string dir1, string dir2)
		{
			if (dir1 == null || dir2 == null) return null;
			if (IsUrl(dir1) || IsUrl(dir2)) return null;
			
			dir1 = NormalizePath(dir1);
			dir2 = NormalizePath(dir2);
			
			string[] aPath = dir1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string[] bPath = dir2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			StringBuilder result = new StringBuilder();
			int indx = 0;
			for(; indx < Math.Min(bPath.Length, aPath.Length); ++indx) {
				if (bPath[indx].Equals(aPath[indx], StringComparison.OrdinalIgnoreCase)) {
					if (result.Length > 0) result.Append(Path.DirectorySeparatorChar);
					result.Append(aPath[indx]);
				} else {
					break;
				}
			}
			if (indx == 0)
				return null;
			else
				return result.ToString();
		}
		
		/// <summary>
		/// Searches all the .net sdk bin folders and return the path of the
		/// exe from the latest sdk.
		/// </summary>
		/// <param name="exeName">The EXE to search for.</param>
		/// <returns>The path of the executable, or null if the exe is not found.</returns>
		public static string GetSdkPath(string exeName) {
			string execPath;
			if (!string.IsNullOrEmpty(WindowsSdk71InstallRoot)) {
				execPath = Path.Combine(WindowsSdk71InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath)) { return execPath; }
			}
			if (!string.IsNullOrEmpty(WindowsSdk70InstallRoot)) {
				execPath = Path.Combine(WindowsSdk70InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath)) { return execPath; }
			}
			if (!string.IsNullOrEmpty(WindowsSdk61InstallRoot)) {
				execPath = Path.Combine(WindowsSdk61InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath)) { return execPath; }
			}
			if (!string.IsNullOrEmpty(WindowsSdk60aInstallRoot)) {
				execPath = Path.Combine(WindowsSdk60aInstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath)) { return execPath; }
			}
			if (!string.IsNullOrEmpty(WindowsSdk60InstallRoot)) {
				execPath = Path.Combine(WindowsSdk60InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath)) { return execPath; }
			}
			if (!string.IsNullOrEmpty(NetSdk20InstallRoot)) {
				execPath = Path.Combine(NetSdk20InstallRoot, "bin\\" + exeName);
				if (File.Exists(execPath)) { return execPath; }
			}
			return null;
		}
		
		/// <summary>
		/// Converts a given absolute path and a given base path to a path that leads
		/// from the base path to the absoulte path. (as a relative path)
		/// </summary>
		public static string GetRelativePath(string baseDirectoryPath, string absPath)
		{
			if (string.IsNullOrEmpty(baseDirectoryPath)) {
				return absPath;
			}
			if (IsUrl(absPath) || IsUrl(baseDirectoryPath)){
				return absPath;
			}
			
			baseDirectoryPath = NormalizePath(baseDirectoryPath);
			absPath           = NormalizePath(absPath);
			
			string[] bPath = baseDirectoryPath.Split(separators);
			string[] aPath = absPath.Split(separators);
			int indx = 0;
			for(; indx < Math.Min(bPath.Length, aPath.Length); ++indx){
				if(!bPath[indx].Equals(aPath[indx], StringComparison.OrdinalIgnoreCase))
					break;
			}
			
			if (indx == 0) {
				return absPath;
			}
			
			StringBuilder erg = new StringBuilder();
			
			if(indx == bPath.Length) {
//				erg.Append('.');
//				erg.Append(Path.DirectorySeparatorChar);
			} else {
				for (int i = indx; i < bPath.Length; ++i) {
					erg.Append("..");
					erg.Append(Path.DirectorySeparatorChar);
				}
			}
			erg.Append(String.Join(Path.DirectorySeparatorChar.ToString(), aPath, indx, aPath.Length-indx));
			return erg.ToString();
		}
		
		/// <summary>
		/// Combines baseDirectoryPath with relPath and normalizes the resulting path.
		/// </summary>
		public static string GetAbsolutePath(string baseDirectoryPath, string relPath)
		{
			return NormalizePath(Path.Combine(baseDirectoryPath, relPath));
		}
		
		public static string RenameBaseDirectory(string fileName, string oldDirectory, string newDirectory)
		{
			fileName     = NormalizePath(fileName);
			oldDirectory = NormalizePath(oldDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			newDirectory = NormalizePath(newDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			if (IsBaseDirectory(oldDirectory, fileName)) {
				if (fileName.Length == oldDirectory.Length) {
					return newDirectory;
				}
				return Path.Combine(newDirectory, fileName.Substring(oldDirectory.Length + 1));
			}
			return fileName;
		}
		
		public static void DeepCopy(string sourceDirectory, string destinationDirectory, bool overwrite)
		{
			if (!Directory.Exists(destinationDirectory)) {
				Directory.CreateDirectory(destinationDirectory);
			}
			foreach (string fileName in Directory.GetFiles(sourceDirectory)) {
				File.Copy(fileName, Path.Combine(destinationDirectory, Path.GetFileName(fileName)), overwrite);
			}
			foreach (string directoryName in Directory.GetDirectories(sourceDirectory)) {
				DeepCopy(directoryName, Path.Combine(destinationDirectory, Path.GetFileName(directoryName)), overwrite);
			}
		}
		
		public static List<string> SearchDirectory(string directory, string filemask, bool searchSubdirectories, bool ignoreHidden)
		{
			List<string> collection = new List<string>();
			SearchDirectory(directory, filemask, collection, searchSubdirectories, ignoreHidden);
			return collection;
		}
		
		public static List<string> SearchDirectory(string directory, string filemask, bool searchSubdirectories)
		{
			return SearchDirectory(directory, filemask, searchSubdirectories, true);
		}
		
		public static List<string> SearchDirectory(string directory, string filemask)
		{
			return SearchDirectory(directory, filemask, true, true);
		}
		
		/// <summary>
		/// Finds all files which are valid to the mask <paramref name="filemask"/> in the path
		/// <paramref name="directory"/> and all subdirectories
		/// (if <paramref name="searchSubdirectories"/> is true).
		/// The found files are added to the List&lt;string&gt;
		/// <paramref name="collection"/>.
		/// If <paramref name="ignoreHidden"/> is true, hidden files and folders are ignored.
		/// </summary>
		static void SearchDirectory(string directory, string filemask, List<string> collection, bool searchSubdirectories, bool ignoreHidden)
		{
			// If Directory.GetFiles() searches the 8.3 name as well as the full name so if the filemask is
			// "*.xpt" it will return "Template.xpt~"
			try {
				bool isExtMatch = Regex.IsMatch(filemask, @"^\*\..{3}$");
				string ext = null;
				string[] file = Directory.GetFiles(directory, filemask);
				if (isExtMatch) ext = filemask.Remove(0,1);
				
				foreach (string f in file) {
					if (ignoreHidden && (File.GetAttributes(f) & FileAttributes.Hidden) == FileAttributes.Hidden) {
						continue;
					}
					if (isExtMatch && Path.GetExtension(f) != ext) continue;
					
					collection.Add(f);
				}
				
				if (searchSubdirectories) {
					string[] dir = Directory.GetDirectories(directory);
					foreach (string d in dir) {
						if (ignoreHidden && (File.GetAttributes(d) & FileAttributes.Hidden) == FileAttributes.Hidden) {
							continue;
						}
						SearchDirectory(d, filemask, collection, searchSubdirectories, ignoreHidden);
					}
				}
			} catch (UnauthorizedAccessException) {
				// Ignore exception when access to a directory is denied.
				// Fixes SD2-893.
			}
		}
		
		// This is an arbitrary limitation built into the .NET Framework.
		// Windows supports paths up to 32k length.
		public static readonly int MaxPathLength = 260;
		
		/// <summary>
		/// This method checks if a path (full or relative) is valid.
		/// </summary>
		public static bool IsValidPath(string fileName)
		{
			// Fixme: 260 is the hardcoded maximal length for a path on my Windows XP system
			//        I can't find a .NET property or method for determining this variable.
			
			if (fileName == null || fileName.Length == 0 || fileName.Length >= MaxPathLength) {
				return false;
			}
			
			// platform independend : check for invalid path chars
			
			if (fileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
				return false;
			}
			if (fileName.IndexOf('?') >= 0 || fileName.IndexOf('*') >= 0) {
				return false;
			}
			
			if (!Regex.IsMatch(fileName, fileNameRegEx)) {
				return false;
			}
			
			if(fileName[fileName.Length-1] == ' ') {
				return false;
			}
			
			if(fileName[fileName.Length-1] == '.') {
				return false;
			}
			
			// platform dependend : Check for invalid file names (DOS)
			// this routine checks for follwing bad file names :
			// CON, PRN, AUX, NUL, COM1-9 and LPT1-9
			
			string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			if (nameWithoutExtension != null) {
				nameWithoutExtension = nameWithoutExtension.ToUpperInvariant();
			}
			
			if (nameWithoutExtension == "CON" ||
			    nameWithoutExtension == "PRN" ||
			    nameWithoutExtension == "AUX" ||
			    nameWithoutExtension == "NUL") {
				return false;
			}
			
			char ch = nameWithoutExtension.Length == 4 ? nameWithoutExtension[3] : '\0';
			
			return !((nameWithoutExtension.StartsWith("COM") ||
			          nameWithoutExtension.StartsWith("LPT")) &&
			         Char.IsDigit(ch));
		}
		
		/// <summary>
		/// Checks that a single directory name (not the full path) is valid.
		/// </summary>
		[ObsoleteAttribute("Use IsValidDirectoryEntryName instead")]
		public static bool IsValidDirectoryName(string name)
		{
			return IsValidDirectoryEntryName(name);
		}
		
		/// <summary>
		/// Checks that a single directory name (not the full path) is valid.
		/// </summary>
		public static bool IsValidDirectoryEntryName(string name)
		{
			if (!IsValidPath(name)) {
				return false;
			}
			if (name.IndexOfAny(new char[]{Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar,Path.VolumeSeparatorChar}) >= 0) {
				return false;
			}
			if (name.Trim(' ').Length == 0) {
				return false;
			}
			return true;
		}
		
		public static bool IsDirectory(string filename)
		{
			if (!Directory.Exists(filename)) {
				return false;
			}
			FileAttributes attr = File.GetAttributes(filename);
			return (attr & FileAttributes.Directory) != 0;
		}

		//TODO This code is Windows specific
		static bool MatchN (string src, int srcidx, string pattern, int patidx)
		{
			int patlen = pattern.Length;
			int srclen = src.Length;
			char next_char;

			for (;;) {
				if (patidx == patlen)
					return (srcidx == srclen);
				next_char = pattern[patidx++];
				if (next_char == '?') {
					if (srcidx == src.Length)
						return false;
					srcidx++;
				}
				else if (next_char != '*') {
					if ((srcidx == src.Length) || (src[srcidx] != next_char))
						return false;
					srcidx++;
				}
				else {
					if (patidx == pattern.Length)
						return true;
					while (srcidx < srclen) {
						if (MatchN(src, srcidx, pattern, patidx))
							return true;
						srcidx++;
					}
					return false;
				}
			}
		}

		static bool Match(string src, string pattern)
		{
			if (pattern[0] == '*') {
				// common case optimization
				int i = pattern.Length;
				int j = src.Length;
				while (--i > 0) {
					if (pattern[i] == '*')
						return MatchN(src, 0, pattern, 0);
					if (j-- == 0)
						return false;
					if ((pattern[i] != src[j]) && (pattern[i] != '?'))
						return false;
				}
				return true;
			}
			return MatchN(src, 0, pattern, 0);
		}

		public static bool MatchesPattern(string filename, string pattern)
		{
			filename = filename.ToUpper();
			pattern = pattern.ToUpper();
			string[] patterns = pattern.Split(';');
			foreach (string p in patterns) {
				if (Match(filename, p)) {
					return true;
				}
			}
			return false;
		}
		

        public static bool WriteTo(string fileName, string str)
        {
            return WriteTo(fileName, str, false);
        }

        public static bool WriteTo(string fileName, Stream stream) 
        {
            return WriteTo(fileName, ConvertUtility.ToBytes(stream));
        }

        public static bool WriteTo(string fileName, byte[] data) 
        {
            return WriteTo(fileName, data, false);
        }

        public static bool WriteTo(string fileName, string str, bool createNew) 
        {
            Encoding ec = Encoding.UTF8;
            if (File.Exists(fileName))
                ec = GetEncoding(fileName);

            return WriteTo(fileName, str, createNew, ec);
        }

        public static bool WriteTo(string fileName, Stream stream, bool createNew)
        {
            Encoding ec = Encoding.UTF8;
            if (File.Exists(fileName))
                ec = GetEncoding(fileName);

            return WriteTo(fileName, stream, createNew, ec);
        }

        public static bool WriteTo(string fileName, byte[] data, bool createNew)
        {
            Encoding ec = Encoding.UTF8;
            if (File.Exists(fileName))
                ec = GetEncoding(fileName);

            return WriteTo(fileName, data, createNew, ec);
        }

        public static bool WriteTo(string fileName, string str, bool createNew, Encoding encoding) 
        {
            return WriteTo(fileName, ConvertUtility.ToBytes(str), createNew, encoding);
        }

        public static bool WriteTo(string fileName, Stream stream, bool createNew, Encoding encoding) 
        {
            return WriteTo(fileName, ConvertUtility.ToBytes(stream), createNew, encoding);
        }

        public static bool WriteTo(string fileName, byte[] data, bool createNew, Encoding encoding)
        {
            try {
                using (var sw = new StreamWriter(fileName, !createNew, encoding)) {
                    sw.Write(data.Select(X => (char)X).ToArray());
                    sw.Flush();
                }
                return true;
            }
            catch (Exception error) {
#if DEBUG
                throw error;
#else
                LoggingService.Error(error.Message, error);
                return false;
#endif
            }
        }

        public static string ReadString(string fileName)
        {
            if (File.Exists(fileName))
            {
                Encoding encoding;
                using(var fs = File.OpenRead(fileName))
                {
                    encoding = GetEncoding(fs);
                }

                var sr = new StreamReader(fileName, encoding ?? Encoding.Default);
                string value = sr.ReadToEnd();
                sr.Close();
                return value;
            }
            else
            {
                //LoggingService.Error("FileServer Read Error: File does not exist.");
                return string.Empty;
            }
        }

        /// <summary>
        /// 根据文件名称判断文件的编码格式
        /// </summary>

        public static Encoding GetEncoding(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            Encoding r = GetEncoding(fs);
            fs.Close();
            return r;
        }

        /// <summary>
        /// 根据文件流判断文件编码格式
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(FileStream fs)
        {
            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            byte[] ss = r.ReadBytes((int)fs.Length);
            r.Close();

            if (ss == null || ss.Length <= 0)
                return null;

            //Encoding Representation
            //UTF-8 EF BB BF
            //UTF-16 Big Endian FE FF
            //UTF-16 Little Endian FF FE
            //UTF-32 Big Endian 00 00 FE FF
            //UTF-32 Little Endian FF FE 00 00

            //编码类型 Coding=编码类型.ASCII; 
            if (ss.Length >= 3 &&
                ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
            {//UTF-8 EF BB BF
                return System.Text.Encoding.UTF8;
            }
            else if (ss.Length >= 2 &&
                ss[0] == 0xFE && ss[1] == 0xFF)
            {//UTF-16 Big Endian FE FF
                return System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss.Length >= 2 &&
                ss[0] == 0xFF && ss[1] == 0xFE)
            {//UTF-16 Little Endian FF FE
                if (ss.Length >= 4 && ss[2] == 0x00 && ss[3] == 0x00)
                {//UTF-32 Little Endian FF FE 00 00
                    return System.Text.Encoding.UTF32;
                }
                else
                {//UTF-16 Little Endian FF FE
                    return System.Text.Encoding.Unicode;
                }
            }
            //else if (ss.Length >= 4 &&
            //    ss[0] == 0x00 && ss[1] == 0x00 && ss[2] == 0xFE && ss[3] == 0xFF)
            //{//UTF-32 Big Endian 00 00 FE FF
            //    return System.Text.Encoding.UTF32; //没有找到？
            //}          
            else
            {//无BOM和其他编码
                return GetNoBomType(ss, ss.Length);
            }
        }

        /// <summary>
        /// 针对无BOM的内容做判断,不是分准确--主要是有可能没分析所有的字节
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static Encoding GetNoBomType(byte[] buf, int len)
        {
            //UNICODE               UTF-8
            //00000000 - 0000007F   0xxxxxxx
            //00000080 - 000007FF   110xxxxx 10xxxxxx
            //00000800 - 0000FFFF   1110xxxx 10xxxxxx 10xxxxxx
            //00010000 - 001FFFFF   11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
            //00200000 - 03FFFFFF   111110xx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx
            //04000000 - 7FFFFFFF   1111110x 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx

            for (int i = 0; i < len; i++)
            {
                ////////////////////////////////////////////////
                //另一种判断方法，只判断2个或3个字节的utf8，只要找到一个符合的就返回了
                //0xf0  1111 0000
                //0xe0  1110 0000
                //0xc0  1100 0000
                //0x80  1000 0000
                if (buf[i] < 0x80)
                {//0xxxxxxx
                    //ascii字符 或 一个字节的utf8
                }
                else if (buf.Length >= i + 2 &&     //有后续的字节
                    (buf[i] & 0xE0) == 0xC0         //110xxxxx
                    && (buf[i + 1] & 0xC0) == 0x80) //10xxxxxx
                {//110xxxxx 10xxxxxx
                    return System.Text.Encoding.UTF8;     //2个字节的utf8  
                }
                else if (buf.Length >= i + 3 &&     //有后续的字节
                    (buf[i] & 0xF0) == 0xE0         //1110xxxx
                    && (buf[i + 1] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 2] & 0xC0) == 0x80) //10xxxxxx
                {//1110xxxx 10xxxxxx 10xxxxxx
                    return System.Text.Encoding.UTF8;    //3个字节的utf8  
                }
                else if (buf.Length >= i + 4 &&     //有后续的字节
                    (buf[i] & 0xF8) == 0xF0         //11110xxx
                    && (buf[i + 1] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 2] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 3] & 0xC0) == 0x80) //10xxxxxx
                {//11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
                    return System.Text.Encoding.UTF8;    //4个字节的utf8  
                }
                else if (buf.Length >= i + 5 &&     //有后续的字节
                    (buf[i] & 0xFC) == 0xF8         //111110xx
                    && (buf[i + 1] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 2] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 3] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 4] & 0xC0) == 0x80) //10xxxxxx
                {//111110xx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx
                    return System.Text.Encoding.UTF8;    //5个字节的utf8  
                }
                else if (buf.Length >= i + 6 &&     //有后续的字节
                    (buf[i] & 0xFE) == 0xFC         //1111110x
                    && (buf[i + 1] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 2] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 3] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 4] & 0xC0) == 0x80  //10xxxxxx
                    && (buf[i + 5] & 0xC0) == 0x80) //10xxxxxx
                {//1111110x 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx
                    return System.Text.Encoding.UTF8;    //6个字节的utf8  
                }
                else
                    return System.Text.Encoding.Default;

                ////////////////////////////////////////////////
            }
            return System.Text.Encoding.Default;
        }

        public static byte[] ReadAllBytes(string fileName) 
        {
            if (File.Exists(fileName)) {
                return File.ReadAllBytes(fileName);
            }
            else {
                //LoggingService.Error("FileServer Read Error: File does not exist.");
                return null;
            }
        }

        /// <summary>
        /// Gets the normalized version of fileName.
        /// Slashes are replaced with backslashes, backreferences "." and ".." are 'evaluated'.
        /// </summary>
        public static string NormalizePath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;

            int i;

            bool isWeb = false;
            for (i = 0; i < fileName.Length; i++)
            {
                if (fileName[i] == '/' || fileName[i] == '\\')
                    break;
                if (fileName[i] == ':')
                {
                    if (i > 1)
                        isWeb = true;
                    break;
                }
            }

            char outputSeparator = isWeb ? '/' : System.IO.Path.DirectorySeparatorChar;

            StringBuilder result = new StringBuilder();
            if (isWeb == false && fileName.StartsWith(@"\\") || fileName.StartsWith("//"))
            {
                i = 2;
                result.Append(outputSeparator);
            }
            else
            {
                i = 0;
            }
            int segmentStartPos = i;
            for (; i <= fileName.Length; i++)
            {
                if (i == fileName.Length || fileName[i] == '/' || fileName[i] == '\\')
                {
                    int segmentLength = i - segmentStartPos;
                    switch (segmentLength)
                    {
                        case 0:
                            // ignore empty segment (if not in web mode)
                            // On unix, don't ignore empty segment if i==0
                            if (isWeb || (i == 0 && Environment.OSVersion.Platform == PlatformID.Unix))
                            {
                                result.Append(outputSeparator);
                            }
                            break;
                        case 1:
                            // ignore /./ segment, but append other one-letter segments
                            if (fileName[segmentStartPos] != '.')
                            {
                                if (result.Length > 0) result.Append(outputSeparator);
                                result.Append(fileName[segmentStartPos]);
                            }
                            break;
                        case 2:
                            if (fileName[segmentStartPos] == '.' && fileName[segmentStartPos + 1] == '.')
                            {
                                // remove previous segment
                                int j;
                                for (j = result.Length - 1; j >= 0 && result[j] != outputSeparator; j--) ;
                                if (j > 0)
                                {
                                    result.Length = j;
                                }
                                break;
                            }
                            else
                            {
                                // append normal segment
                                goto default;
                            }
                        default:
                            if (result.Length > 0) result.Append(outputSeparator);
                            result.Append(fileName, segmentStartPos, segmentLength);
                            break;
                    }
                    segmentStartPos = i + 1; // remember start position for next segment
                }
            }
            if (isWeb == false)
            {
                if (result.Length > 0 && result[result.Length - 1] == outputSeparator)
                {
                    result.Length -= 1;
                }
                if (result.Length == 2 && result[1] == ':')
                {
                    result.Append(outputSeparator);
                }
            }
            return result.ToString();
        }

        public static bool IsEqualFileName(string fileName1, string fileName2)
        {
            return string.Equals(NormalizePath(fileName1),
                                 NormalizePath(fileName2),
                                 StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBaseDirectory(string baseDirectory, string testDirectory)
        {
            if (baseDirectory == null || testDirectory == null)
                return false;
            baseDirectory = NormalizePath(baseDirectory) + Path.DirectorySeparatorChar;
            testDirectory = NormalizePath(testDirectory) + Path.DirectorySeparatorChar;

            return testDirectory.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase);
        }

        public static string IsValidFileEntryName(string name)
        {
            if (name.Length == 0 || !char.IsLetter(name[0]) && name[0] != '_')
            {
                return "error:File name must start with letter.";
            }
            if (!IsValidDirectoryEntryName(name))
            {
                return "error:File name error.";
            }
            if (name.EndsWith("."))
            {
                return "error:File name must not end with dot.";
            }

            return null;
        }

        public static void RemoveDirectory(string fileName, bool recover = false) 
        {
            recover = DeleteToRecycleBin;
            FileCancelEventArgs eargs = new FileCancelEventArgs(fileName, true);
            OnFileRemoving(eargs);
            if (eargs.Cancel)
                return;
            if (!eargs.OperationAlreadyDone)
            {
                try
                {
                    if (Directory.Exists(fileName))
                    {
                        if (recover)
                            NativeMethods.DeleteToRecycleBin(fileName);
                        else
                            Directory.Delete(fileName, true);
                    }
                }
                catch (Exception e)
                {
                    //MessageService.ShowHandledException(e, "Can't remove directory " + fileName);
                }
            }
            OnFileRemoved(new FileEventArgs(fileName, true));
        }


        public static void RemoveFile(string fileName, bool recover = false)
        {
            recover = DeleteToRecycleBin;

            bool isDirectory = Directory.Exists(fileName);

            FileCancelEventArgs eargs = new FileCancelEventArgs(fileName, isDirectory);
            OnFileRemoving(eargs);
            if (eargs.Cancel)
                return;
            if (!eargs.OperationAlreadyDone)
            {
                if (isDirectory)
                {
                    try
                    {
                        if (Directory.Exists(fileName))
                        {
                            if (recover)
                            {
                                foreach (var file in Directory.GetFiles(fileName))
                                {
                                    NativeMethods.DeleteToRecycleBin(file);
                                }
                            }
                            else 
                            {
                                foreach (var file in Directory.GetFiles(fileName)) 
                                {
                                    File.Delete(file);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //MessageService.ShowHandledException(e, "Can't remove directory " + fileName);
                    }
                }
                else
                {
                    try
                    {
                        if (File.Exists(fileName))
                        {
                            if (DeleteToRecycleBin)
                                NativeMethods.DeleteToRecycleBin(fileName);
                            else
                                File.Delete(fileName);
                        }
                    }
                    catch (Exception e)
                    {
                        //MessageService.ShowHandledException(e, "Can't remove file " + fileName);
                    }
                }
            }
            OnFileRemoved(new FileEventArgs(fileName, isDirectory));
        }

        /// <summary>
        /// Renames or moves a file, raising the appropriate events. This method may show message boxes.
        /// </summary>
        public static bool RenameFile(string oldName, string newName, bool isDirectory)
        {
            if (IsEqualFileName(oldName, newName))
                return false;
            // FileChangeWatcher.DisableAllChangeWatchers();
            try
            {
                FileRenamingEventArgs eargs = new FileRenamingEventArgs(oldName, newName, isDirectory);
                OnFileRenaming(eargs);
                if (eargs.Cancel)
                    return false;
                if (!eargs.OperationAlreadyDone)
                {
                    try
                    {
                        if (isDirectory && Directory.Exists(oldName))
                        {
                            if (Directory.Exists(newName))
                            {
                                return false;
                            }
                            Directory.Move(oldName, newName);

                        }
                        else if (File.Exists(oldName))
                        {
                            if (File.Exists(newName))
                            {
                                return false;
                            }
                            File.Move(oldName, newName);
                        }
                    }
                    catch (Exception e)
                    {
                        if (isDirectory)
                        {
                            //MessageService.ShowHandledException(e, "Can't rename directory " + oldName);
                        }
                        else
                        {
                            //MessageService.ShowHandledException(e, "Can't rename file " + oldName);
                        }
                        return false;
                    }
                }
                OnFileRenamed(new FileRenameEventArgs(oldName, newName, isDirectory));
                return true;
            }
            finally
            {
                // FileChangeWatcher.EnableAllChangeWatchers();
            }
        }

        /// <summary>
        /// Copies a file, raising the appropriate events. This method may show message boxes.
        /// </summary>
        public static bool CopyFile(string oldName, string newName, bool overwrite)
        {
            if (IsEqualFileName(oldName, newName))
                return false;

            bool isDirectory = Directory.Exists(oldName);

            FileRenamingEventArgs eargs = new FileRenamingEventArgs(oldName, newName, isDirectory);
            OnFileCopying(eargs);
            if (eargs.Cancel)
                return false;
            if (!eargs.OperationAlreadyDone)
            {
                try
                {
                    if (isDirectory && Directory.Exists(oldName))
                    {
                        if (!overwrite && Directory.Exists(newName))
                        {  
                            return false;
                        }
                        DeepCopy(oldName, newName, overwrite);

                    }
                    else if (File.Exists(oldName))
                    {
                        if (!overwrite && File.Exists(newName))
                        {
                            return false;
                        }
                        File.Copy(oldName, newName, overwrite);
                    }
                }
                catch (Exception e)
                {
                    if (isDirectory)
                    {
                        //MessageService.ShowHandledException(e, "Can't copy directory " + oldName);
                    }
                    else
                    {
                        //MessageService.ShowHandledException(e, "Can't copy file " + oldName);
                    }
                    return false;
                }
            }
            OnFileCopied(new FileRenameEventArgs(oldName, newName, isDirectory));
            return true;
        }

        #region Event Handlers

        static void OnFileRemoved(FileEventArgs e)
        {
            if (FileRemoved != null)
            {
                FileRemoved(null, e);
            }
        }

        static void OnFileRemoving(FileCancelEventArgs e)
        {
            if (FileRemoving != null)
            {
                FileRemoving(null, e);
            }
        }

        static void OnFileRenamed(FileRenameEventArgs e)
        {
            if (FileRenamed != null)
            {
                FileRenamed(null, e);
            }
        }

        static void OnFileRenaming(FileRenamingEventArgs e)
        {
            if (FileRenaming != null)
            {
                FileRenaming(null, e);
            }
        }

        static void OnFileCopied(FileRenameEventArgs e)
        {
            if (FileCopied != null)
            {
                FileCopied(null, e);
            }
        }

        static void OnFileCopying(FileRenamingEventArgs e)
        {
            if (FileCopying != null)
            {
                FileCopying(null, e);
            }
        }

        #endregion Event Handlers

        #region Static event firing methods

        /// <summary>
        /// Fires the event handlers for a file being created.
        /// </summary>
        /// <param name="fileName">The name of the file being created. This should be a fully qualified path.</param>
        /// <param name="isDirectory">Set to true if this is a directory</param>
        public static bool FireFileReplacing(string fileName, bool isDirectory)
        {
            FileCancelEventArgs e = new FileCancelEventArgs(fileName, isDirectory);
            if (FileReplacing != null)
            {
                FileReplacing(null, e);
            }
            return !e.Cancel;
        }

        /// <summary>
        /// Fires the event handlers for a file being replaced.
        /// </summary>
        /// <param name="fileName">The name of the file being created. This should be a fully qualified path.</param>
        /// <param name="isDirectory">Set to true if this is a directory</param>
        public static void FireFileReplaced(string fileName, bool isDirectory)
        {
            if (FileReplaced != null)
            {
                FileReplaced(null, new FileEventArgs(fileName, isDirectory));
            }
        }

        /// <summary>
        /// Fires the event handlers for a file being created.
        /// </summary>
        /// <param name="fileName">The name of the file being created. This should be a fully qualified path.</param>
        /// <param name="isDirectory">Set to true if this is a directory</param>
        public static void FireFileCreated(string fileName, bool isDirectory)
        {
            if (FileCreated != null) {
                FileCreated(null, new FileEventArgs(fileName, isDirectory));
            }
        }

        #endregion Static event firing methods

        #region Events

        public static event EventHandler<FileEventArgs> FileCreated;

        public static event EventHandler<FileRenamingEventArgs> FileRenaming;
        public static event EventHandler<FileRenameEventArgs> FileRenamed;

        public static event EventHandler<FileRenamingEventArgs> FileCopying;
        public static event EventHandler<FileRenameEventArgs> FileCopied;

        public static event EventHandler<FileCancelEventArgs> FileRemoving;
        public static event EventHandler<FileEventArgs> FileRemoved;

        public static event EventHandler<FileCancelEventArgs> FileReplacing;
        public static event EventHandler<FileEventArgs> FileReplaced;

        #endregion Events

        public static bool DeleteToRecycleBin
        {
            get {
                //return PropertyService.Get("Play.Studio.Core.Services.DeleteToRecycleBin", true);
            }
            set {
                //PropertyService.Set("Play.Studio.Core.Services.DeleteToRecycleBin", value);
            }
        }

        public static IntPtr DeleteToRecycleBinHandle
        {
            get;
            internal set;
        }
	}
}
