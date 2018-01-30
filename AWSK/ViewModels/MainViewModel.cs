using AWSK.Stores;
using Reactive.Bindings;
using System;
using System.Windows;

namespace AWSK.ViewModels
{
	class MainViewModel
	{
		#region プロパティ
		// データベースからデータを拾ったか？
		public ReactiveProperty<bool> ReadyDataStoreFlg = new ReactiveProperty<bool>(false);
		// 基地航空隊を飛ばしたか？
		public ReactiveProperty<bool> BasedAirUnit1Flg = new ReactiveProperty<bool>(false);
		public ReactiveProperty<bool> BasedAirUnit2Flg = new ReactiveProperty<bool>(false);
		public ReactiveProperty<bool> BasedAirUnit3Flg = new ReactiveProperty<bool>(false);
		// 基地航空隊の装備の選択番号
		public ReactiveProperty<int> BasedAirUnitIndex11 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex12 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex13 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex14 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex21 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex22 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex23 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex24 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex31 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex32 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex33 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex34 = new ReactiveProperty<int>(0);
		// 基地航空隊の艦載機熟練度
		public ReactiveProperty<int> BasedAirUnitMas11 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas12 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas13 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas14 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas21 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas22 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas23 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas24 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas31 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas32 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas33 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitMas34 = new ReactiveProperty<int>(0);
		// 基地航空隊の装備改修度
		public ReactiveProperty<int> BasedAirUnitRf11 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf12 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf13 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf14 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf21 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf22 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf23 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf24 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf31 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf32 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf33 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf34 = new ReactiveProperty<int>(0);
		// 敵艦隊の艦の選択番号
		public ReactiveProperty<int> EnemyUnitIndex1 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex2 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex3 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex4 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex5 = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex6 = new ReactiveProperty<int>(0);
		#endregion

		#region コマンド
		// シミュレーションを実行
		public ReactiveCommand RunSimulationCommand { get; } = new ReactiveCommand();
		#endregion

		// その他初期化用コード
		private async void Initialize() {
			// データベースの初期化
			var status = await DataStore.Initialize();
			switch (status) {
			case DataStoreStatus.Exist:
				ReadyDataStoreFlg.Value = true;
				break;
			case DataStoreStatus.Success:
				ReadyDataStoreFlg.Value = true;
				MessageBox.Show("ダウンロードに成功しました。", "AWSK");
				break;
			case DataStoreStatus.Failed:
				ReadyDataStoreFlg.Value = false;
				MessageBox.Show("ダウンロードに失敗しました。", "AWSK");
				return;
			}
			// 画面表示の初期化
			var kammusuNameList = DataStore.KammusuNameList();
			var weaponNameList = DataStore.WeaponNameList();
			return;
		}

		// クリップボードからインポート
		public void ImportClipboardText() {
			try {
				// クリップボードから文字列を取得する
				string clipboardString = Clipboard.GetText();
				if (clipboardString == null)
					throw new Exception();
				// クリップボードの文字列をJSONとしてパースし、艦隊データに変換する
				var fleetData = DataStore.ParseFleetData(clipboardString);
				MessageBox.Show(fleetData.ToString(), "AWSK");
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				MessageBox.Show("クリップボードから艦隊データを取得できませんでした。", "AWSK");
			}
		}

		// コンストラクタ
		public MainViewModel() {
			// プロパティ・コマンドを設定
			RunSimulationCommand.Subscribe(ImportClipboardText);	//スタブ
			// その他初期化
			Initialize();
		}
	}
}

/* 入力サンプル：
{"version":4,"f1":{"s1":{"id":"426","lv":99,"luck":-1,"items":{"i1":{"id":122,"rf":"10"},"i2":{"id":122,"rf":"10"},"i3":{"id":106,"rf":"10"},"i4":{"id":43,"rf":0}}},"s2":{"id":"149","lv":99,"luck":-1,"items":{"i1":{"id":103,"rf":"10"},"i2":{"id":103,"rf":"10"},"i3":{"id":59,"rf":"10","mas":7},"i4":{"id":36,"rf":"10"},"ix":{"id":43,"rf":0}}},"s3":{"id":"118","lv":99,"luck":-1,"items":{"i1":{"id":41,"rf":0},"i2":{"id":50,"rf":"10"},"i3":{"id":50,"rf":"10"}}},"s4":{"id":"119","lv":99,"luck":-1,"items":{"i1":{"id":41,"rf":0},"i2":{"id":50,"rf":"10"},"i3":{"id":50,"rf":"10"}}},"s5":{"id":"278","lv":99,"luck":-1,"items":{"i3":{"id":157,"rf":"10","mas":"7"},"i2":{"id":110,"rf":0,"mas":"7"},"i1":{"id":144,"rf":0,"mas":"7"},"i4":{"id":212,"rf":0,"mas":"7"}}},"s6":{"id":"467","lv":99,"luck":-1,"items":{"i1":{"id":144,"rf":0,"mas":"7"},"i2":{"id":93,"rf":0,"mas":"7"},"i3":{"id":100,"rf":0,"mas":"7"},"i4":{"id":110,"rf":0,"mas":"7"}}}},"f2":{"s1":{}}}
*/
