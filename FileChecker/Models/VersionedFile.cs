using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileVersionChecker.Helpers;

namespace FileVersionChecker.Models
{
    public class VersionedFile : NotificationObject
    {
        private string fullPath;

        #region Filename
        private string filename;

        public string Filename
        {
            get { return filename; }
            set
            {
                if (filename != value)
                {
                    filename = value;
                    RaisePropertyChanged(() => Filename);
                }
            }
        }
        #endregion

        #region Directory
        private string directory;

        public string Directory
        {
            get { return directory; }
            set
            {
                if (directory != value)
                {
                    directory = value;
                    RaisePropertyChanged(() => Directory);
                }
            }
        }
        #endregion

        #region FileVersion

        private string fileVersion;

        public string FileVersion
        {
            get { return fileVersion; }
            set
            {
                if (fileVersion != value)
                {
                    fileVersion = value;
                    RaisePropertyChanged(() => FileVersion);
                }
            }
        }

        #endregion

        #region Architecture
        private string architecture;

        public string Architecture
        {
            get { return architecture; }
            set
            {
                if (architecture != value)
                {
                    architecture = value;
                    RaisePropertyChanged(() => Architecture);
                }
            }
        }
        #endregion

        #region Extension
        private string extension;

        public string Extension
        {
            get { return extension; }
            set
            {
                if (extension != value)
                {
                    extension = value;
                    RaisePropertyChanged(() => Extension);
                }
            }
        }
        #endregion

        #region Constructor
        public VersionedFile(string fullpath, string version, string architecture)
        {
            this.fullPath = fullpath;
            FillFileData();
            FileVersion = version;
            Architecture = architecture;
        }
        #endregion


        #region Fill File Data Method
        private void FillFileData()
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            Directory = Path.GetDirectoryName(fullPath);
            Filename = Path.GetFileName(fullPath);
            Extension = Path.GetExtension(fullPath);
        }
        #endregion
    }
}
