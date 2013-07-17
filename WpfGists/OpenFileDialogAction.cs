using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Interactivity;

namespace WpfGists
{
  public class FileDialogAction : TriggerAction<DependencyObject>
  {

    public Action<string[]> Callback
    {
      get { return (Action<string[]>)GetValue(CallbackProperty); }
      set { SetValue(CallbackProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FileOpenedAction.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CallbackProperty =
        DependencyProperty.Register("Callback", typeof(Action<string[]>), typeof(FileDialogAction), new PropertyMetadata(null));

    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(FileDialogAction), new PropertyMetadata(null));
    
    public string DefaultExt
    {
      get { return (string)GetValue(DefaultExtProperty); }
      set { SetValue(DefaultExtProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DefaultExt.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DefaultExtProperty =
        DependencyProperty.Register("DefaultExt", typeof(string), typeof(FileDialogAction), new PropertyMetadata(null));
        
    public string Filter
    {
      get { return (string)GetValue(FilterProperty); }
      set { SetValue(FilterProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register("Filter", typeof(string), typeof(FileDialogAction), new PropertyMetadata(null));



    public bool MultiSelect
    {
      get { return (bool)GetValue(MultiSelectProperty); }
      set { SetValue(MultiSelectProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsMultiSelect.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MultiSelectProperty =
        DependencyProperty.Register("MultiSelect", typeof(bool), typeof(FileDialogAction), new PropertyMetadata(false));



    public FileDialogType DialogType
    {
        get { return (FileDialogType)GetValue(DialogTypeProperty); }
        set { SetValue(DialogTypeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DialogType.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DialogTypeProperty =
        DependencyProperty.Register("DialogType", typeof(FileDialogType), typeof(FileDialogAction), new PropertyMetadata(FileDialogType.OpenFile));


    public int FilterIndex
    {
        get { return (int)GetValue(FilterIndexProperty); }
        set { SetValue(FilterIndexProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FilterIndex.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FilterIndexProperty =
        DependencyProperty.Register("FilterIndex", typeof(int), typeof(FileDialogAction), new PropertyMetadata(1));


    
    protected override void Invoke(object parameter)
    {
        switch (DialogType)
        { 
            case FileDialogType.OpenFile:
                ShowOpenFileDialog();
                break;
            case FileDialogType.SaveFile:
                ShowSaveFileDialog();
                break;
        }
    }

    private void ShowOpenFileDialog()
    {
        var dialog = new OpenFileDialog();
        if (Title != null) { dialog.Title = Title; }
        if (DefaultExt != null) { dialog.DefaultExt = DefaultExt; }
        if (Filter != null) { dialog.Filter = Filter; }
        
        dialog.Multiselect = MultiSelect;
        dialog.FilterIndex = FilterIndex;

        if (true == dialog.ShowDialog())
        {
            Callback(dialog.FileNames);
        }
    }

    private void ShowSaveFileDialog()
    {
        var dialog = new SaveFileDialog();
        if (Title != null) { dialog.Title = Title; }
        if (DefaultExt != null) { dialog.DefaultExt = DefaultExt; }
        if (Filter != null) { dialog.Filter = Filter; }
        dialog.FilterIndex = FilterIndex;
        if (true == dialog.ShowDialog())
        {
            Callback(dialog.FileNames);
        }
    }

    public enum FileDialogType
    { 
        OpenFile,
        SaveFile
    }
  }
}
