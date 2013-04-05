using System;

namespace WpfSample.ViewModel
{
  public class NavigationResult
  {
    public Uri Uri { get; set; }
    public System.Net.WebResponse Response { get; set; }
    public object Content { get; set; }
    public object ExtraData { get; set; }
  }
}
