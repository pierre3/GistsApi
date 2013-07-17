using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfGists.ViewModel;
using WpfGists.Properties;

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
            string clientID = "";
            string clientSecret = "";

            if (e.Args.Length >= 2)
            {
                clientID = e.Args[0];
                clientSecret = e.Args[1];
            }
            else
            {
                clientID = Settings.Default.ClientID;
                clientSecret = Settings.Default.ClientSecret;
            }
            if ((string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret)) || ((clientID == "your clientID") || (clientSecret == "your clientSecret")))
            {
                MessageBox.Show("Set your \"ClientID\" and \"ClientSecret\" to \"WpfGists.exe.config\" file.", "WpfGists", MessageBoxButton.OK, MessageBoxImage.Information);
                App.Current.Shutdown();
                return;
            }
            MainWindow window = new MainWindow();
            GistsWindowViewModel vm = new GistsWindowViewModel(clientID, clientSecret);
            window.DataContext = vm;
            window.Show();
            vm.NavigateToGistLogin();
        }
    }
}
