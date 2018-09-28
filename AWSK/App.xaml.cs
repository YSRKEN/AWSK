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

            // テストコード
            /*var database = DataBaseService.instance;
            var downloader = DownloadService.instance;
            database.CreateWeaponTable(false);
            var weaponData = await downloader.downloadWeaponDataFromWikiaAsync();
            database.SaveAll(weaponData, true);
            /*database.CreateKammusuTable(false);
            var kammusuData = await downloader.downloadKammusuDataFromWikiaAsync();
            database.SaveAll(kammusuData, false);*/

            // アプリの起動
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
