using AWSK.Service;
using System.Windows;

namespace AWSK {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // テストコード
            var database = DataBaseService.instance;
            var downloader = DownloadService.instance;

            database.CreateWeaponTable(false);
            var weaponData1 = await downloader.downloadWeaponDataFromDeckBuilderAsync();
            database.SaveAll(weaponData1, false);

            database.CreateKammusuTable(false);
            var kammusuData1 = await downloader.downloadKammusuDataFromDeckBuilderAsync();
            database.SaveAll(kammusuData1, false);

            // アプリの起動
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
