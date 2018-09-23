using AWSK.Models;
using AWSK.Service;
using AWSK.Stores;
using Prism;
using System.Windows;
using static AWSK.Constant;

namespace AWSK {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

            // テストコード
            var database = DataBaseService.instance;
            database.Save(new Weapon(2, "12.7cm連装砲", WeaponType.Other, 2, 0, 0, true));
            database.Save(new Weapon(3, "61cm三連装魚雷", WeaponType.Other, 0, 0, 0, true));

            // アプリの起動
            var bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}
	}
}
