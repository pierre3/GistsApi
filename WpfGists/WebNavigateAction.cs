using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
namespace WpfGists
{
  public class WebNavigateAction:TriggerAction<WebBrowser>
  {
    public Uri NavigateUri
    {
      get { return (Uri)GetValue(NavigateUriProperty); }
      set { SetValue(NavigateUriProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NavigateString.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty NavigateUriProperty =
        DependencyProperty.Register("NavigateUri", typeof(Uri), typeof(WebNavigateAction), new UIPropertyMetadata());


    protected override void Invoke(object parameter)
    {
      this.AssociatedObject.Navigate(NavigateUri);
    }
  }
}
