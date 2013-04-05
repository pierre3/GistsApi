using GistsApi;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfSample.ViewModel
{
  public class GistsWindowViewModel : INotifyPropertyChanged
  {
    #region Fields
    private GistClient _gistClient;
    private ObservableCollection<GistListItem> _gists;
    private GistListItem _selectedItem;
    private Action<NavigationResult> _navigatedAction;
    private Action<string[]> _fileOpenedAction;
    private Uri _navigateUri;
    private ICommand _delete;
    private ICommand _updateList;
    private ICommand _createAGist;
    private ICommand _editAGist;
    private string _uploadFileName;
    private string _uploadFileDescription;
    private bool _uploadFileIsPublic;
    private bool _browserVisible;
    private string _text;
    private string _statusMessage;
    #endregion

    #region Properties
    public Uri NavigateUri
    {
      set
      {
        if (_navigateUri == value)
        { return; }
        _navigateUri = value;
        OnPropertyChanged("NavigateUri");
      }
      get { return _navigateUri; }
    }

    public Action<NavigationResult> NavigatedAction
    {
      get
      {
        if (_navigatedAction == null)
        { _navigatedAction = new Action<NavigationResult>(NavigateCompleted); }
        return _navigatedAction;
      }
    }

    public Action<string[]> FileOpenedAction
    {
      get
      {
        if (_fileOpenedAction == null)
        {
          _fileOpenedAction = new Action<string[]>(FileOpened);
        }
        return _fileOpenedAction;
      }

    }

    public ObservableCollection<GistListItem> ListItems
    {
      get { return _gists; }
      set
      {
        if (_gists == value)
        { return; }
        _gists = value;
        OnPropertyChanged("ListItems");
      }
    }

    public GistListItem SelectedItem
    {
      get { return _selectedItem; }
      set
      {
        if (_selectedItem == value)
        { return; }
        _selectedItem = value;
        _gistClient.Cancel();
        OpenItem();
        OnPropertyChanged("SelectedItem");
      }
    }

    public string UploadFileName
    {
      get { return _uploadFileName; }
      set
      {
        if (_uploadFileName == value)
        { return; }
        _uploadFileName = value;
        OnPropertyChanged("UploadFileName");
      }
    }

    public string UploadFileDescription
    {
      get { return _uploadFileDescription; }
      set
      {
        if (_uploadFileDescription == value)
        { return; }
        _uploadFileDescription = value;
        OnPropertyChanged("UploadFileDescription");
      }
    }

    public bool UploadFileIsPublic
    {
      get { return _uploadFileIsPublic; }
      set
      {
        if (_uploadFileIsPublic == value)
        { return; }
        _uploadFileIsPublic = value;
        OnPropertyChanged("UploadFileIsPublic");
      }
    }

    public bool BrowserVisible
    {
      get { return _browserVisible; }
      set
      {
        if (_browserVisible == value)
        { return; }
        _browserVisible = value;
        OnPropertyChanged("BrowserVisible");
      }
    }

    public string Text
    {
      get { return _text; }
      set
      {
        if (_text == value)
        { return; }
        _text = value;
        OnPropertyChanged("Text");
      }
    }

    public string StatusMessage
    {
      get { return _statusMessage; }
      set
      {
        if (_statusMessage == value)
        { return; }
        _statusMessage = value;
        OnPropertyChanged("StatusMessage");
      }
    }

    public ICommand DeleteCommand
    {
      get
      {
        if (_delete == null)
        {
          _delete = new DelegateCommand(_ => DeleteAGists(), _ => CanDelete());
        }
        return _delete;
      }
    }

    public ICommand UpdateListCommand
    {
      get
      {
        if (_updateList == null)
        {
          _updateList = new DelegateCommand(_ => UpdateList());
        }
        return _updateList;
      }
    }

    public ICommand CreateAGistCommand
    {
      get
      {
        if (_createAGist == null)
        {
          _createAGist = new DelegateCommand(_ => CreateAGist(), _ => CanCreateAGist());
        }
        return _createAGist;
      }
    }

    public ICommand EditAGistCommand
    {
      get 
      {
        if (_editAGist == null)
        {
          _editAGist = new DelegateCommand(_ => EditAGist(), _ => CanEditAGist());
        }
        return _editAGist;
      }
    }

    public GistsWindowViewModel(string clientId, string clientSecret)
    {
      this._gistClient = new GistClient(clientId, clientSecret);
      this.ListItems = new ObservableCollection<GistListItem>();

    }

    public void NavigateToGistLogin()
    {
      this.BrowserVisible = true;
      this.NavigateUri = this._gistClient.AuthorizeUrl;
    }
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

    #region Methods
    private async void NavigateCompleted(NavigationResult result)
    {
      if (result.Uri == null)
      { return; }

      if (result.Uri.AbsoluteUri.Contains("code="))
      {
        var authCode = Regex.Split(result.Uri.AbsoluteUri, "code=")[1];

        await TryAsyncApi("Authorize",
         async () => await _gistClient.Authorize(authCode));

        await ListGists();
        BrowserVisible = false;
      }
    }

    private async void FileOpened(string[] fileNames)
    {
      var filename = fileNames.FirstOrDefault();
      if (string.IsNullOrEmpty(filename))
      { return; }

      UploadFileName = System.IO.Path.GetFileName(filename);
      using (var reader = new System.IO.StreamReader(filename))
      {
        Text = await reader.ReadToEndAsync();
      }
    }

    private async void OpenItem()
    {
      if (SelectedItem == null)
      { return; }

      await TryAsyncApi("Opening " + SelectedItem.Filename,
        async () =>Text = await _gistClient.DownloadRawText(new Uri(SelectedItem.RawUrl)));

      UploadFileName = SelectedItem.Filename;
      UploadFileDescription = SelectedItem.Description;
    }

    private async void EditAGist()
    {
      await TryAsyncApi("Edit " + SelectedItem.Filename,
        async () => await _gistClient.EditAGist(SelectedItem.ID, UploadFileDescription, UploadFileName, Text));

      await ListGists();
    }

    private bool CanEditAGist()
    {
      return SelectedItem != null
        && !string.IsNullOrWhiteSpace(UploadFileName)
        && !string.IsNullOrWhiteSpace(UploadFileDescription);
    }

    private async void DeleteAGists()
    {
      await TryAsyncApi("Delete Gists",
        async () => await _gistClient.DeleteAGist(SelectedItem.ID));

      await ListGists();
    }

    private bool CanDelete()
    {
      return SelectedItem != null;
    }

    private async void UpdateList()
    {
      await ListGists();
    }

    private async void CreateAGist()
    {
      await TryAsyncApi("Create A Gist",
        async () => await _gistClient.CreateAGist(UploadFileDescription, UploadFileName, UploadFileIsPublic, Text));

      await ListGists();
    }

    private bool CanCreateAGist()
    {
      return !string.IsNullOrWhiteSpace(UploadFileDescription)
        && !string.IsNullOrWhiteSpace(UploadFileName)
        && !string.IsNullOrWhiteSpace(Text);
    }

    private async Task ListGists()
    {
      await TryAsyncApi("List Gists",
        async () =>
        {
          var gists = await _gistClient.ListGists();
          ListItems = new ObservableCollection<GistListItem>(gists.Select(item => new GistListItem(item)));
        });
    }

    private async Task TryAsyncApi(string apiName, Func<Task> callApi)
    {
      try
      {
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
    }
    #endregion
  }
}
