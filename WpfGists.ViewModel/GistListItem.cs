using GistsApi;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System;

namespace WpfGists.ViewModel
{
  public class GistListItem
  {
    private GistObject _source;
    private File _selectedFile;
    private Action<File> _selectedFileChanged;

    public IList<File> Files
    {
      get {return _source.files; }
    }

    public string Description
    {
      get { return _source.description; }
    }

    public File SelectedFile
    { 
      get { return _selectedFile; }
      set 
      {
        if (_selectedFile == value)
        { return; }
        _selectedFile = value;
        _selectedFileChanged(value);
      }
    }
    
    public string HtmlUrl 
    { 
      get { return _source.html_url; } 
    }
    
    public string ID 
    { 
      get { return _source.id; } 
    }

    public bool IsPublic
    {
      get { return _source.@public; }
    }

    public string Author
    {
        get { return "@" + this._source.user.login; }
    }

    public string CreatedAt
    {
        get { return DateTime.Parse(this._source.created_at).ToLocalTime().ToShortDateString(); }
    }

    public GistListItem()
    {
      this._source = new GistObject();
    }

    public GistListItem(GistObject source, Action<File> selectedFileChanged)
    {
      this._source = source;
      this._selectedFileChanged = selectedFileChanged;
    }

  }
}
