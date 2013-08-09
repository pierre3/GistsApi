using GistsApi;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows.Input;

namespace WpfGists.ViewModel
{
    public class GistListItem
    {
        private GistObject _source;
        private File _selectedFile;
        private Action<File> _selectedFileChanged;
        private ICommand _copyPlainUrl;
        private ICommand _copyMarkdownLinkUrl;

        public ICommand CopyPlainUrlCommand
        {
            get
            {
                if (this._copyPlainUrl == null)
                {
                    this._copyPlainUrl = new DelegateCommand(
                        _ => System.Windows.Clipboard.SetText(HtmlUrl));
                }
                return this._copyPlainUrl;
            }
        }

        public ICommand CopyMarkdownLinkUrlCommand
        {
            get
            {
                if (this._copyMarkdownLinkUrl == null)
                {
                    this._copyMarkdownLinkUrl = new DelegateCommand(
                        _ => System.Windows.Clipboard.SetText(string.Format("[{0}]({1})",Description, HtmlUrl)));
                }
                return this._copyMarkdownLinkUrl;
            }
        }

        public IList<File> Files
        {
            get { return _source.files; }
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
