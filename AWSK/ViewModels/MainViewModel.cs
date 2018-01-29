using AWSK.Stores;
using Codeplex.Data;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Windows;

namespace AWSK.ViewModels
{
	class MainViewModel
	{
		#region コマンド
		// クリップボードからインポート
		public ReactiveCommand ImportClipboardTextCommand { get; } = new ReactiveCommand();
		#endregion

		// JSONデータを分析する
		private FleetData ParseFleetData(dynamic obj) {
			var fleetData = new FleetData();
			for (int fi = 1; fi <= 4; ++fi) {
				// 定義されていない艦隊は飛ばす
				if (!obj.IsDefined($"f{fi}"))
					continue;
				var kammusuList = new List<KammusuData>();
				for (int si = 1; si <= 6; ++si) {
					// 定義されていない艦隊は飛ばす
					if (!obj[$"f{fi}"].IsDefined($"s{si}"))
						continue;
					// 中身が定義されていないようなら飛ばす
					if (!obj[$"f{fi}"][$"s{si}"].IsDefined("id"))
						continue;
					// idを読み取り、そこから艦名を出力
					int k_id = int.Parse(obj[$"f{fi}"][$"s{si}"].id);
					int lv = (int)(obj[$"f{fi}"][$"s{si}"].lv);
					var kammusuData = DataStore.KammusuDataById(k_id);
					kammusuData.Level = lv;
					for (int ii = 1; ii <= 5; ++ii) {
						string key = (ii == 5 ? "ix" : $"i{ii}");
						// 定義されていない装備は飛ばす
						if (!obj[$"f{fi}"][$"s{si}"].items.IsDefined(key))
							continue;
						// idを読み取り、そこから装備名を出力
						int w_id = (int)(obj[$"f{fi}"][$"s{si}"].items[key].id);
						var weaponData = DataStore.WeaponDataById(w_id);
						// 艦載機熟練度を出力
						// 「装備改修度が0なら『0』、1以上なら『"1"』」といった
						// 恐ろしい仕様があるので対策が面倒だった
						if (obj[$"f{fi}"][$"s{si}"].items[key].IsDefined("mas")) {
							var rawMas = obj[$"f{fi}"][$"s{si}"].items[key].mas;
							int mas = 0;
							if (rawMas.GetType() == typeof(string)) {
								mas = int.Parse(rawMas);
							} else {
								mas = (int)(rawMas);
							}
							weaponData.Mas = mas;
						}
						//
						// 装備改修度を出力
						// ヤバさは艦載機熟練度と同様
						if (obj[$"f{fi}"][$"s{si}"].items[key].IsDefined("rf")) {
							var rawRf = obj[$"f{fi}"][$"s{si}"].items[key].rf;
							int rf = 0;
							if(rawRf.GetType() == typeof(string)) {
								rf = int.Parse(rawRf);
							} else {
								rf = (int)(rawRf);
							}
							weaponData.Rf =rf;
						}
						kammusuData.Weapon.Add(weaponData);
					}
					kammusuList.Add(kammusuData);
				}
				fleetData.Kammusu.Add(kammusuList);
			}
			return fleetData;
		}

		// クリップボードからインポート
		public void ImportClipboardText() {
			try {
				// クリップボードから文字列を取得する
				string clipboardString = Clipboard.GetText();
				if (clipboardString == null)
					throw new Exception();
				// クリップボードの文字列をJSONとしてパースし、艦隊データに変換する
				var obj = DynamicJson.Parse(clipboardString);
				var fleetData = ParseFleetData(obj);
				MessageBox.Show(fleetData.ToString(), "AWSK");
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				MessageBox.Show("クリップボードから艦隊データを取得できませんでした。", "AWSK");
			}
		}

		// コンストラクタ
		public MainViewModel() {
			DataStore.Initialize();
			ImportClipboardTextCommand.Subscribe(ImportClipboardText);
		}
	}
}

/* 入力サンプル：
{"version":4,"f1":{"s1":{"id":"426","lv":99,"luck":-1,"items":{"i1":{"id":122,"rf":"10"},"i2":{"id":122,"rf":"10"},"i3":{"id":106,"rf":"10"},"i4":{"id":43,"rf":0}}},"s2":{"id":"149","lv":99,"luck":-1,"items":{"i1":{"id":103,"rf":"10"},"i2":{"id":103,"rf":"10"},"i3":{"id":59,"rf":"10","mas":7},"i4":{"id":36,"rf":"10"},"ix":{"id":43,"rf":0}}},"s3":{"id":"118","lv":99,"luck":-1,"items":{"i1":{"id":41,"rf":0},"i2":{"id":50,"rf":"10"},"i3":{"id":50,"rf":"10"}}},"s4":{"id":"119","lv":99,"luck":-1,"items":{"i1":{"id":41,"rf":0},"i2":{"id":50,"rf":"10"},"i3":{"id":50,"rf":"10"}}},"s5":{"id":"278","lv":99,"luck":-1,"items":{"i3":{"id":157,"rf":"10","mas":"7"},"i2":{"id":110,"rf":0,"mas":"7"},"i1":{"id":144,"rf":0,"mas":"7"},"i4":{"id":212,"rf":0,"mas":"7"}}},"s6":{"id":"467","lv":99,"luck":-1,"items":{"i1":{"id":144,"rf":0,"mas":"7"},"i2":{"id":93,"rf":0,"mas":"7"},"i3":{"id":100,"rf":0,"mas":"7"},"i4":{"id":110,"rf":0,"mas":"7"}}}},"f2":{"s1":{}}}
*/
