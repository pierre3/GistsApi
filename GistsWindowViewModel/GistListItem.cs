using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using GistsApi;

namespace WpfSample.ViewModel
{
  public class GistListItem : INotifyPropertyChanged
  {
    private GistObject _source;

    public string Filename
    {
      get { return _source.files.First().filename; }
      set
      {
        var file = _source.files.First();
        if (file.filename == value) 
        { return; }
        
        file.filename = value;
        OnPropertyChanged("Filename");
      }
    }

    public string Description
    {
      get { return _source.description; }
      set
      {
        if (_source.description == value)
        { return; }

        _source.description = value;
        OnPropertyChanged("Description");
      }
    }

    public File File { get { return _source.files.First(); } }

    public string HtmlUrl { get { return _source.html_url; } }
    public string RawUrl { get { return _source.files.First().raw_url; } }
    public string ID { get { return _source.id; } }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(string name)
    {
      var handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(name));
      }
    }

    public GistListItem()
    {
      this._source = new GistObject();
    }

    public GistListItem(GistObject source)
    {
      this._source = source;
    }
  }
}
