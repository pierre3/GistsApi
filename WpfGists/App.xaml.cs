using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfGists.ViewModel;

namespace WpfGists
{
  /// <summary>
  /// App.xaml の相互作用ロジック
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      if (e.Args.Length < 2)
      {
        throw new ArgumentException("Command-line args are required. \"ClientID\" \"ClientSecret\".");
      }
      var clientId = e.Args[0];
      var clientSecret = e.Args[1];

      var mainWindow = new MainWindow();
      var vm = new GistsWindowViewModel(clientId, clientSecret);
      mainWindow.DataContext = vm;
      mainWindow.Show();
      vm.NavigateToGistLogin();
    }
  }
}
