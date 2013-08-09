using GistsApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfGists.Utility;

namespace WpfGists.ViewModel
{
    public class GistsWindowViewModel : INotifyPropertyChanged
    {
        #region Fields
        private Action<string[]> _openFileAction;
        private Action<string[]> _saveFileAction;
        private Action<NavigationResult> _navigatedAction;
        private ICommand _createAGist;
        private ICommand _delete;
        private ICommand _deleteFile;
        private ICommand _editAGist;
        private ICommand _forkAGist;
        private ICommand _listGists;
        private ICommand _listPublicGists;
        private ICommand _listStarredGists;
        private ICommand _listUsersGists;
        private ICommand _starAGist;
        private ICommand _unstarAGist;
        private ICommand _moveFirstList;
        private ICommand _moveLastList;
        private ICommand _moveNextList;
        private ICommand _movePrevList;
        private ICommand _downloadFile;
        private ObservableCollection<GistListItem> _gists;
        private Uri _navigateUri;
        private GistListItem _selectedItem;
        private string[] _selectedFiles;
        private GistClient _gistClient;
        private string _statusMessage;
        private string _text;
        private string _gistContents;
        private bool _browserVisible;
        private string _listName;
        private bool _isProcessing;
        private string _uploadFileDescription;
        private bool _uploadFileIsPublic;
        private string _uploadFileName;
        private string _userName = "user name";
        private bool _showPreview;
        #endregion

        #region Properties
        public ObservableCollection<GistListItem> ListItems
        {
            get { return this._gists; }
            set
            {
                if (this._gists != value)
                {
                    this._gists = value;
                    this.OnPropertyChanged("ListItems");
                }
            }
        }

        public GistListItem SelectedItem
        {
            get { return this._selectedItem; }
            set
            {
                if (this._selectedItem != value)
                {
                    this._selectedItem = value;
                    this.OnPropertyChanged("SelectedItem");
                    if (this.SelectedItem != null)
                    {
                        if (ShowPreview)
                        {
                            //this.UploadFileDescription = this._selectedItem.Description;
                            this.OpenItem(this._selectedItem.SelectedFile);
                        }
                    }
                }
            }
        }

        public bool BrowserVisible
        {
            get { return this._browserVisible; }
            set
            {
                if (this._browserVisible != value)
                {
                    this._browserVisible = value;
                    this.OnPropertyChanged("BrowserVisible");
                }
            }
        }

        public bool IsProcessing
        {
            get
            {
                return this._isProcessing;
            }
            set
            {
                if (this._isProcessing != value)
                {
                    this._isProcessing = value;
                    this.OnPropertyChanged("IsProcessing");
                }
            }
        }

        public string ListName
        {
            get { return this._listName; }
            set
            {
                if (this._listName != value)
                {
                    this._listName = value;
                    this.OnPropertyChanged("ListName");
                }
            }
        }

        public Uri NavigateUri
        {
            get { return this._navigateUri; }
            set
            {
                if (this._navigateUri != value)
                {
                    this._navigateUri = value;
                    this.OnPropertyChanged("NavigateUri");
                }
            }
        }

        public string StatusMessage
        {
            get { return this._statusMessage; }
            set
            {
                if (this._statusMessage != value)
                {
                    this._statusMessage = value;
                    this.OnPropertyChanged("StatusMessage");
                }
            }
        }

        public string Text
        {
            get { return this._text; }
            set
            {
                if (this._text != value)
                {
                    this._text = value;
                    this.OnPropertyChanged("Text");
                }
            }
        }

        public string GistContents
        {
            get { return _gistContents; }
            set 
            {
                if (_gistContents == value)
                { return; }

                _gistContents = value;
                OnPropertyChanged("GistContents");
            }
        }

        public string UploadFileDescription
        {
            get { return this._uploadFileDescription; }
            set
            {
                if (this._uploadFileDescription != value)
                {
                    this._uploadFileDescription = value;
                    this.OnPropertyChanged("UploadFileDescription");
                }
            }
        }

        public bool UploadFileIsPublic
        {
            get { return this._uploadFileIsPublic; }
            set
            {
                if (this._uploadFileIsPublic != value)
                {
                    this._uploadFileIsPublic = value;
                    this.OnPropertyChanged("UploadFileIsPublic");
                }
            }
        }

        public string UploadFileName
        {
            get { return this._uploadFileName; }
            set
            {
                if (this._uploadFileName != value)
                {
                    this._uploadFileName = value;
                    this.OnPropertyChanged("UploadFileName");
                }
            }
        }

        public string UserName
        {
            get { return this._userName; }
            set
            {
                if (this._userName != value)
                {
                    this._userName = value;
                    this.OnPropertyChanged("UserName");
                }
            }
        }

        public bool ShowPreview
        {
            get { return this._showPreview; }
            set
            {
                if (this._showPreview != value)
                {
                    this._showPreview = value;
                    this.OnPropertyChanged("ShowPreview");
                }
            }
        }

        public Action<string[]> OpenFileAction
        {
            get
            {
                if (this._openFileAction == null)
                {
                    this._openFileAction = new Action<string[]>(this.FileOpened);
                }
                return this._openFileAction;
            }
        }

        public Action<string[]> SaveFileAction
        {
            get
            {
                if (this._saveFileAction == null)
                {
                    this._saveFileAction = new Action<string[]>(this.SaveToFile);
                }
                return this._saveFileAction;
            }
        }

        public Action<NavigationResult> NavigatedAction
        {
            get
            {
                if (this._navigatedAction == null)
                {
                    this._navigatedAction = new Action<NavigationResult>(this.NavigateCompleted);
                }
                return this._navigatedAction;
            }
        }

        #region Commands
        public ICommand CreateAGistCommand
        {
            get
            {
                if (this._createAGist == null)
                {
                    this._createAGist = new DelegateCommand(_ => this.CreateAGist(), _ => this.CanCreateAGist());
                }
                return this._createAGist;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if (this._delete == null)
                {
                    this._delete = new DelegateCommand(_ => this.DeleteAGist(), _ => this.IsSelected());
                }
                return this._delete;
            }
        }

        public ICommand DeleteFileCommand
        {
            get
            {
                if (this._deleteFile == null)
                {
                    this._deleteFile = new DelegateCommand(_ => this.DeleteFile(), _ => this.CanDeleteFile());
                }
                return this._deleteFile;
            }
        }

        public ICommand EditAGistCommand
        {
            get
            {
                if (this._editAGist == null)
                {
                    this._editAGist = new DelegateCommand(_ => this.EditAGist(), _ => this.CanEditAGist());
                }
                return this._editAGist;
            }
        }

        public ICommand ForkAGistCommand
        {
            get
            {
                if (this._forkAGist == null)
                {
                    this._forkAGist = new DelegateCommand(_ => this.ForkAGist(), _ => this.IsSelected());
                }
                return this._forkAGist;
            }
        }

        public ICommand ListGistsCommand
        {
            get
            {
                if (this._listGists == null)
                {
                    this._listGists = new DelegateCommand(_ => this.ListGists(GistClient.ListMode.AuthenticatedUserGists));
                }
                return this._listGists;
            }
        }

        public ICommand ListPublicGistsCommand
        {
            get
            {
                if (this._listPublicGists == null)
                {
                    this._listPublicGists = new DelegateCommand(_ => this.ListGists(GistClient.ListMode.PublicGists));
                }
                return this._listPublicGists;
            }
        }

        public ICommand ListStarredGistsCommand
        {
            get
            {
                if (this._listStarredGists == null)
                {
                    this._listStarredGists = new DelegateCommand(_ => this.ListGists(GistClient.ListMode.AuthenticatedUserStarredGists));
                }
                return this._listStarredGists;
            }
        }

        public ICommand ListUsersGistsCommand
        {
            get
            {
                if (this._listUsersGists == null)
                {
                    this._listUsersGists = new DelegateCommand(_ => this.ListGists(GistClient.ListMode.UsersGists),
                        _ => !string.IsNullOrEmpty(this.UserName));
                }
                return this._listUsersGists;
            }
        }

        public ICommand MoveFirstListCommand
        {
            get
            {
                if (this._moveFirstList == null)
                {
                    this._moveFirstList = new DelegateCommand(_ => this.MoveList(this._gistClient.FirstLinkUrl),
                        _ => !string.IsNullOrEmpty(this._gistClient.FirstLinkUrl));
                }
                return this._moveFirstList;
            }
        }

        public ICommand MoveLastListCommand
        {
            get
            {
                if (this._moveLastList == null)
                {
                    this._moveLastList = new DelegateCommand(_ => this.MoveList(this._gistClient.LastLinkUrl),
                        _ => !string.IsNullOrEmpty(this._gistClient.LastLinkUrl));
                }
                return this._moveLastList;
            }
        }

        public ICommand MoveNextListCommand
        {
            get
            {
                if (this._moveNextList == null)
                {
                    this._moveNextList = new DelegateCommand(_ => this.MoveList(this._gistClient.NextLinkUrl),
                        _ => !string.IsNullOrEmpty(this._gistClient.NextLinkUrl));
                }
                return this._moveNextList;
            }
        }

        public ICommand MovePrevListCommand
        {
            get
            {
                if (this._movePrevList == null)
                {
                    this._movePrevList = new DelegateCommand(_ => this.MoveList(this._gistClient.PrevLinkUrl),
                        _ => !string.IsNullOrEmpty(this._gistClient.PrevLinkUrl));
                }
                return this._movePrevList;
            }
        }

        public ICommand StarAGistCommand
        {
            get
            {
                if (this._starAGist == null)
                {
                    this._starAGist = new DelegateCommand(_ => this.StarAGist(), _ => this.IsSelected());
                }
                return this._starAGist;
            }
        }

        public ICommand UnstarAGistCommand
        {
            get
            {
                if (this._unstarAGist == null)
                {
                    this._unstarAGist = new DelegateCommand(_ => this.UnstarAGist());
                }
                return this._unstarAGist;
            }
        }

        public ICommand DownloadSelectedFile
        {
            get
            {
                if (this._downloadFile == null)
                {
                    this._downloadFile = new DelegateCommand(
                        _ => 
                        {
                            UploadFileDescription = SelectedItem.Description;
                            DownloadItem(SelectedItem.SelectedFile);
                        },
                        _ => IsSelected());
                }
                return this._downloadFile;
            }
        }
        #endregion

        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Constructors
        public GistsWindowViewModel(string clientId, string clientSecret)
        {
            this._gistClient = new GistClient(clientId, clientSecret, "WpfGists/0.5");
            this.ListItems = new ObservableCollection<GistListItem>();
            this.ShowPreview = true;
        }
        #endregion

        #region Methods
        public void NavigateToGistLogin()
        {
            this.BrowserVisible = true;
            this.NavigateUri = this._gistClient.AuthorizeUrl;
        }

        private bool IsSelected()
        {
            return (this.SelectedItem != null);
        }

        private async void NavigateCompleted(NavigationResult result)
        {
            if (result.Uri == null)
            { return; }

            if (result.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = Regex.Split(result.Uri.AbsoluteUri, "code=")[1];

                await TryAsyncApi("Authorize",
                 async () => await _gistClient.Authorize(authCode));

                await this.ListMyGists();
                BrowserVisible = false;
            }
        }

        private async void FileOpened(string[] fileNames)
        {
            if (fileNames.Length > 1)
            {
                _selectedFiles = fileNames;

                var names = fileNames.Select(s => "\"" + System.IO.Path.GetFileName(s) + "\"");
                UploadFileName = names.Aggregate((a, b) => a + " " + b);

                Text = "*** Selected Files ***" + Environment.NewLine
                  + names.Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine;
                UploadFileDescription = "gist description...";
                return;
            }

            _selectedFiles = null;
            var filename = fileNames.FirstOrDefault();
            if (string.IsNullOrEmpty(filename))
            { return; }

            UploadFileName = System.IO.Path.GetFileName(filename);
            using (var stream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var bytes = await stream.ReadBytesAsync().ConfigureAwait(false);
                Text = bytes.GetCode().GetString(bytes);
            }
        }

        private async void SaveToFile(string[] fileNames)
        {
            var filename = fileNames.FirstOrDefault();
            if (string.IsNullOrEmpty(filename))
            { return; }
            using (var stream = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(this.Text);
                await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
        }

        private IObservable<Tuple<string, string>> OpenFiles(string[] fileNames)
        {
            return ObservableEx.Create<Tuple<string, string>>(async (observer, ct) =>
            {
                try
                {
                    IsProcessing = true;
                    foreach (var name in fileNames)
                    {
                        if (ct.IsCancellationRequested)
                        { return; }

                        using (var stream = new System.IO.FileStream(name, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                        {
                            var bytes = await stream.ReadBytesAsync().ConfigureAwait(false);
                            var text = bytes.GetCode().GetString(bytes);
                            observer.OnNext(Tuple.Create(System.IO.Path.GetFileName(name), text));
                        }

                    }
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                finally
                { IsProcessing = false; }

            });
        }

        private async void OpenItem(File file)
        {
            if (file == null)
            { return; }

            //UploadFileName = file.filename;
            await TryAsyncApi("Open " + file.filename,
              async () => GistContents = await _gistClient.DownloadRawText(new Uri(file.raw_url)));
        }

        private async void DownloadItem(File file)
        {
            if (file == null)
            { return; }

            UploadFileName = file.filename;
            await TryAsyncApi("Download " + file.filename,
              async () => Text = await _gistClient.DownloadRawText(new Uri(file.raw_url)));
        }

        private async void EditAGist()
        {
            await TryAsyncApi("Edit " + UploadFileName,
              async () => await _gistClient.EditAGist(SelectedItem.ID, UploadFileDescription, UploadFileName, Text));

            await ListMyGists();
        }

        private bool CanEditAGist()
        {
            return SelectedItem != null
              && !string.IsNullOrWhiteSpace(UploadFileName)
              && !string.IsNullOrWhiteSpace(UploadFileDescription);
        }

        private async void DeleteAGist()
        {
            await TryAsyncApi("Delete a Gist",
              async () => await _gistClient.DeleteAGist(SelectedItem.ID));

            await ListMyGists();
        }

        private bool CanDelete()
        {
            return SelectedItem != null;
        }

        private async void DeleteFile()
        {
            await TryAsyncApi("Delete a File",
              async () => await _gistClient.DeleteAFile(SelectedItem.ID,
                SelectedItem.Description, SelectedItem.SelectedFile.filename));

            await ListMyGists();
        }

        private bool CanDeleteFile()
        {
            return (SelectedItem != null)
              && (SelectedItem.SelectedFile != null);
        }

        private async void CreateAGist()
        {
            if (_selectedFiles != null)
            {
                CreateAMultiFileGist();
            }
            else
            {
                await CreateASingleFileGist();
                await ListMyGists();
            }

        }

        private async Task CreateASingleFileGist()
        {
            await TryAsyncApi("Create A Gist",
                  async () => await _gistClient.CreateAGist(UploadFileDescription, UploadFileIsPublic,
                    new[] { Tuple.Create(UploadFileName, Text) }));
        }

        private void CreateAMultiFileGist()
        {
            var fileList = new List<Tuple<string, string>>();

            var disposable = OpenFiles(_selectedFiles).Subscribe(file =>
            {
                StatusMessage = "[Open File] " + file.Item1;
                fileList.Add(file);
            },
            e => StatusMessage = "[Open File] Error: " + e.Message,
            async () =>
            {
                await TryAsyncApi("Create A Gist",
                  async () => await _gistClient.CreateAGist(UploadFileDescription, UploadFileIsPublic, fileList));
                await ListMyGists();
                _selectedFiles = null;
            });
        }

        private bool CanCreateAGist()
        {
            return !string.IsNullOrWhiteSpace(UploadFileDescription)
              && !string.IsNullOrWhiteSpace(UploadFileName)
              && !string.IsNullOrWhiteSpace(Text);
        }

        private async void ListGists(GistClient.ListMode mode)
        {
            switch (mode)
            {
                case GistClient.ListMode.PublicGists:
                    await this.ListPublicGists();
                    break;

                case GistClient.ListMode.UsersGists:
                    await this.ListUsersGists(this.UserName);
                    break;

                case GistClient.ListMode.AuthenticatedUserGists:
                    await this.ListMyGists();
                    break;

                case GistClient.ListMode.AuthenticatedUserStarredGists:
                    await this.ListStarredGists();
                    break;
            }
        }

        private async void ForkAGist()
        {
            await this.TryAsyncApi("Fork a Gist", async () => await this._gistClient.ForkAGist(this.SelectedItem.ID));
            await this.ListMyGists();
        }

        private async Task ListMyGists()
        {
            await this.TryAsyncApi("List My Gists", async delegate
            {
                IEnumerable<GistObject> gists = await this._gistClient.ListGists();
                this.ListItems = new ObservableCollection<GistListItem>(gists.Select(item =>
                    new GistListItem(item, GistListItem_SelectedFileChanged)));
                this.ListName = "My Gists";
            });
        }

        private async Task ListPublicGists()
        {
            await this.TryAsyncApi("List Public Gists", async delegate
            {
                IEnumerable<GistObject> gists = await this._gistClient.ListGists(GistClient.ListMode.PublicGists);
                this.ListItems = new ObservableCollection<GistListItem>(gists.Select(item =>
                    new GistListItem(item, GistListItem_SelectedFileChanged)));
                this.ListName = "All Public Gists";
            });
        }

        private async Task ListStarredGists()
        {
            await this.TryAsyncApi("List Starred Gists", async delegate
            {
                IEnumerable<GistObject> gists = await this._gistClient.ListGists(GistClient.ListMode.AuthenticatedUserStarredGists);
                this.ListItems = new ObservableCollection<GistListItem>(gists.Select(item =>
                    new GistListItem(item, GistListItem_SelectedFileChanged)));
                this.ListName = "My Starred Gists";
            });
        }

        private async Task ListUsersGists(string userName)
        {
            await this.TryAsyncApi(string.Format("List {0}'s Gists", userName), async delegate
            {
                IEnumerable<GistObject> gists = await this._gistClient.ListGists(this.UserName);
                this.ListItems = new ObservableCollection<GistListItem>(gists.Select(item =>
                    new GistListItem(item, GistListItem_SelectedFileChanged)));
                this.ListName = string.Format("{0}'s Gists", userName);
            });
        }

        private async void MoveList(string linkUrl)
        {
            await this.TryAsyncApi("Turn a Page", async delegate
            {
                IEnumerable<GistObject> gists = await this._gistClient.ListGists(new Uri(linkUrl));
                this.ListItems = new ObservableCollection<GistListItem>(gists.Select(item =>
                    new GistListItem(item, GistListItem_SelectedFileChanged)));
            });
        }

        private async void StarAGist()
        {
            await this.TryAsyncApi("Star a Gist", async () => await this._gistClient.StarAGist(this.SelectedItem.ID));
            await this.ListStarredGists();
        }

        private async void UnstarAGist()
        {
            await this.TryAsyncApi("Unstar a Gist", async () => await this._gistClient.UnstarAGist(this.SelectedItem.ID));
            await this.ListStarredGists();
        }

        private async Task TryAsyncApi(string apiName, Func<Task> callApi)
        {
            _gistClient.Cancel();
            try
            {
                IsProcessing = true;
                StatusMessage = string.Format("[{0}]: Processing...", apiName);
                await callApi();
                StatusMessage = string.Format("[{0}]: Completed.", apiName);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                StatusMessage = string.Format("[{0}]: Error. {1}", apiName, e.Message);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = string.Format("[{0}]: Canceled.", apiName);
            }
            finally
            {
                IsProcessing = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void GistListItem_SelectedFileChanged(File file)
        {
            if (_selectedItem.Files.Contains(file))
            {
                if (ShowPreview)
                {
                    OpenItem(file);
                }
            }
        }
        #endregion
    }
}
