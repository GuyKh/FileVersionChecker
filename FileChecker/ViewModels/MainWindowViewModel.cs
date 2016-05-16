using System.Linq;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Input;
using System.Windows;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Text;
using FileVersionChecker.Helpers;
using FileVersionChecker.Models;
using Microsoft.Win32;

namespace FileVersionChecker.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Properties

        #region Path

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value;
                    RaisePropertyChanged(() => Path);
                    var delegateCommand = GetCommand as DelegateCommand;
                    delegateCommand?.CanExecute(null);
                }
            }
        }

        #endregion

        #region ExpectedVersion

        private const string DefaultExpectedVersion = "1.0.0.0";

        private string _expectedVersion = DefaultExpectedVersion;

        public string ExpectedVersion
        {
            get { return _expectedVersion; }
            set
            {
                if (_expectedVersion != value)
                {
               
                        _expectedVersion = value;
                   
                    RaisePropertyChanged(() => ExpectedVersion);
                }
            }
        }

        #endregion

        #region FilesCollection
        private ObservableCollection<VersionedFile> _files;

        public ObservableCollection<VersionedFile> Files
        {
            get { return _files; }
            set
            {
                if (_files != value)
                {
                    _files = value;
                    RaisePropertyChanged(() => Files);
                }
            }
        }


        #endregion

        #region IsBusy
        private bool _isBusy = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }
        #endregion

        #endregion

        #region Commands

        public ICommand BrowseCommand { get { return new DelegateCommand(OnBrowseCommand); } }
        public ICommand ExportCommand { get { return new DelegateCommand(OnExport, CanExecuteExport); } }
        public ICommand GetCommand { get { return new DelegateCommand(OnGetFilesCommand, CanExecuteGet); } }

        #endregion

        #region Ctor
        public MainWindowViewModel()
        {
            _prefixes = ConfigurationManager.AppSettings["Prefixes"].Split(',');
            _extentions = ConfigurationManager.AppSettings["Extentions"].Split(',');
        }
        #endregion

        #region Command Handlers

        private void OnBrowseCommand()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            bool? ret = dlg.ShowDialog();
            if (ret != null && ret.Value){
                Path = dlg.SelectedPath;
            }
        }




        private readonly string[] _prefixes;
        private readonly string[] _extentions;

        private void OnGetFilesCommand()
        {
            Task.Factory.StartNew(path =>
            {
                IsBusy = true;
                string pathToCheck = (string)path;
                var files = Directory.EnumerateFiles(pathToCheck, "*.*", SearchOption.AllDirectories)
                    .Where(
                        file =>
                        {
                            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);
                            return fileNameWithoutExtension != null && (Regex.IsMatch(fileNameWithoutExtension.ToLower(),
                                $"({string.Join("|", _prefixes)}).*") && _extentions.Contains(System.IO.Path.GetExtension(file)));
                        })
                    .ToArray();
                return files;
            }, Path).ContinueWith(result =>
            {
                string[] files = result.Result;

                if (files == null)
                {
                    return;
                }

                ObservableCollection<VersionedFile> versionedFiles = new ObservableCollection<VersionedFile>();

                foreach (string str in files)
                {
                    string version = FileVersionInfo.GetVersionInfo(str).FileVersion;
                    var machineType = CheckDLLArchitecture.DoesDllIs64Bit(str);

                    if (string.IsNullOrEmpty(ExpectedVersion) || version != ExpectedVersion)
                    {
                        versionedFiles.Add(new VersionedFile(str, version, machineType.ToString()));
                    }
                }

                if (versionedFiles.Any())
                {
                    Files = versionedFiles;
                }
                else
                {
                    MessageBox.Show("No relevant files were found");
                }
                IsBusy = false;
            }, TaskContinuationOptions.OnlyOnRanToCompletion).ContinueWith(exception =>
            {
                IsBusy = false;
                MessageBox.Show("Error Getting the files");
            }, TaskContinuationOptions.OnlyOnFaulted);

            }

        private void OnExport()
        {
            // Load up the save file dialog with the default option as saving as a .csv file.
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";

            bool? result = sfd.ShowDialog();
            if (result != null && result.Value)
            {
                Task.Factory.StartNew(() =>
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("Filename,Directory,File Version,Architecture,Extension");

                    foreach (VersionedFile versionedFile in Files)
                    {
                        sb.AppendLine(string.Join(",", versionedFile.Filename, versionedFile.Directory,
                            versionedFile.FileVersion, versionedFile.Architecture, versionedFile.Extension));
                    }

                    // If they've selected a save location...
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false))
                    {
                        // Write the stringbuilder text to the the file.
                        sw.WriteLine(sb.ToString());
                    }

                }).ContinueWith(state =>
                {
                    MessageBox.Show("CSV file saved.");
                }, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(ex =>
                {
                    if (ex.Exception != null)
                        MessageBox.Show("CSV file failed+\n" + ex.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        MessageBox.Show("CSV file failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            
        }


        private bool CanExecuteGet()
        {
            return !string.IsNullOrEmpty(Path);
        }

        private bool CanExecuteExport()
        {
            return Files != null && Files.Any();
        }


        #endregion

        
    }
}