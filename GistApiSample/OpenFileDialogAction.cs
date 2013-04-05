using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Interactivity;
namespace WpfSample
{
  public class OpenFileDialogAction : TriggerAction<DependencyObject>
  {

    public Action<string[]> Callback
    {
      get { return (Action<string[]>)GetValue(CallbackProperty); }
      set { SetValue(CallbackProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FileOpenedAction.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CallbackProperty =
        DependencyProperty.Register("Callback", typeof(Action<string[]>), typeof(OpenFileDialogAction), new PropertyMetadata(null));

    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(OpenFileDialogAction), new PropertyMetadata(null));
    
    public string DefaultExt
    {
      get { return (string)GetValue(DefaultExtProperty); }
      set { SetValue(DefaultExtProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DefaultExt.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DefaultExtProperty =
        DependencyProperty.Register("DefaultExt", typeof(string), typeof(OpenFileDialogAction), new PropertyMetadata(null));
        
    public string Filter
    {
      get { return (string)GetValue(FilterProperty); }
      set { SetValue(FilterProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register("Filter", typeof(string), typeof(OpenFileDialogAction), new PropertyMetadata(null));



    public bool MultiSelect
    {
      get { return (bool)GetValue(MultiSelectProperty); }
      set { SetValue(MultiSelectProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsMultiSelect.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MultiSelectProperty =
        DependencyProperty.Register("MultiSelect", typeof(bool), typeof(OpenFileDialogAction), new PropertyMetadata(false));


    
    protected override void Invoke(object parameter)
    {
      var dialog = new OpenFileDialog();
      if(Title !=null){dialog.Title = Title;}
      if(DefaultExt != null){dialog.DefaultExt = DefaultExt;}
      if (Filter != null) { dialog.Filter = Filter; }
      dialog.Multiselect = MultiSelect;
      if (true == dialog.ShowDialog())
      {
        Callback(dialog.FileNames);
      }
    }
  }
}
