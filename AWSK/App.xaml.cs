using AWSK.Models;
using AWSK.Service;
using AWSK.Stores;
using Prism;
using System.Collections.Generic;
using System.Windows;
using static AWSK.Constant;

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
            foreach(var weapon in weaponData1) {
                database.Save(weapon);
            }

            database.CreateKammusuTable(false);
            var kammusuData1 = await downloader.downloadKammusuDataFromDeckBuilderAsync();
            foreach(var pair in kammusuData1) {
                database.Save(pair.Key, pair.Value);
            }

            // アプリの起動
            var bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}
	}
}
