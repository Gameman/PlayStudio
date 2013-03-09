using System;

namespace Play.Studio.Core.Services
{
    public class FileEventArgs : EventArgs
    {
        string fileName = null;

        bool isDirectory;

        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return isDirectory;
            }
        }

        public FileEventArgs(string fileName, bool isDirectory)
        {
            this.fileName = fileName;
            this.isDirectory = isDirectory;
        }
    }

    public class FileCancelEventArgs : FileEventArgs
    {
        bool cancel;

        public bool Cancel
        {
            get
            {
                return cancel;
            }
            set
            {
                cancel = value;
            }
        }

        bool operationAlreadyDone;

        public bool OperationAlreadyDone
        {
            get
            {
                return operationAlreadyDone;
            }
            set
            {
                operationAlreadyDone = value;
            }
        }

        public FileCancelEventArgs(string fileName, bool isDirectory)
            : base(fileName, isDirectory)
        {
        }
    }

    public class FileRenamingEventArgs : FileRenameEventArgs
    {
        bool cancel;

        public bool Cancel
        {
            get
            {
                return cancel;
            }
            set
            {
                cancel = value;
            }
        }

        bool operationAlreadyDone;

        public bool OperationAlreadyDone
        {
            get
            {
                return operationAlreadyDone;
            }
            set
            {
                operationAlreadyDone = value;
            }
        }

        public FileRenamingEventArgs(string sourceFile, string targetFile, bool isDirectory)
            : base(sourceFile, targetFile, isDirectory)
        {
        }
    }

    public class FileRenameEventArgs : EventArgs
    {
        bool isDirectory;

        string sourceFile = null;
        string targetFile = null;

        public string SourceFile
        {
            get
            {
                return sourceFile;
            }
        }

        public string TargetFile
        {
            get
            {
                return targetFile;
            }
        }


        public bool IsDirectory
        {
            get
            {
                return isDirectory;
            }
        }

        public FileRenameEventArgs(string sourceFile, string targetFile, bool isDirectory)
        {
            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
            this.isDirectory = isDirectory;
        }
    }

    /// <summary>
    /// EventArgs with a file name.
    /// </summary>
    public class FileNameEventArgs : System.EventArgs
    {
        string fileName;

        public string FileName
        {
            get
            {
                return fileName;
            }
        }

        public FileNameEventArgs(string fileName)
        {
            this.fileName = fileName;
        }
    }
}
