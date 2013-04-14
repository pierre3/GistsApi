using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using WpfGists.ViewModel;

namespace WpfGists
{
  public class LoadCompletedBehavior : Behavior<WebBrowser>
  {
    public Action<NavigationResult> LoadCompletedAction
    {
      get { return (Action<NavigationResult>)GetValue(LoadCompletedActionProperty); }
      set { SetValue(LoadCompletedActionProperty, value); }
    }

    // Using a DependencyProperty as the backing store for NavigatedAction.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LoadCompletedActionProperty =
        DependencyProperty.Register("LoadCompletedAction", typeof(Action<NavigationResult>), typeof(LoadCompletedBehavior), new PropertyMetadata(null));

    protected override void OnAttached()
    {
      base.OnAttached();
      this.AssociatedObject.LoadCompleted += AssociatedObject_LoadCompleted;
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.LoadCompleted -= AssociatedObject_LoadCompleted;
      base.OnDetaching();
    }

    void AssociatedObject_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {
      if (LoadCompletedAction == null)
      { return; }
      LoadCompletedAction(new NavigationResult()
        {
          Uri = e.Uri,
          Response = e.WebResponse,
          Content = e.Content,
          ExtraData = e.ExtraData
        });
    }

  } 
}
