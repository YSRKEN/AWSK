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
            database.CreateWeaponTable(false);
            database.Save(new Weapon(2, "12.7cm連装砲", WeaponType.Other, 2, 0, 0, true));
            database.Save(new Weapon(13, "61cm三連装魚雷", WeaponType.Other, 0, 0, 0, true));
            var hoge = database.findByWeaponId(3);
            var fuga = database.findByWeaponId(1);
            database.CreateKammusuTable(false);
            database.Save(new Kammusu(9, "吹雪", KammusuType.DD, 39, new List<int>{ 0, 0 }, true), new List<int> { 2, 13 });
            var foo = database.findByKammusuId(9, true);
            var foo2 = database.findByKammusuId(9, false);
            var bar = database.findByKammusuId(1, true);

            var downloader = DownloadService.instance;
            var weaponData1 = await downloader.downloadWeaponDataFromDeckBuilderAsync();

            // アプリの起動
            var bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}
	}
}
