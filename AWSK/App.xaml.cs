using AWSK.Service;
using System;
using System.Windows;

namespace AWSK {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // アプリの起動
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
