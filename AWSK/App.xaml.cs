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

            database.CreateWeaponTable(true);
            var weaponData1 = await downloader.downloadWeaponDataFromDeckBuilderAsync();
            foreach (var weapon in weaponData1) {
                database.Save(weapon, false);
            }

            database.CreateKammusuTable(true);
            var kammusuData1 = await downloader.downloadKammusuDataFromDeckBuilderAsync();
            foreach (var pair in kammusuData1) {
                database.Save(pair.Key, pair.Value, false);
            }

            // アプリの起動
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
