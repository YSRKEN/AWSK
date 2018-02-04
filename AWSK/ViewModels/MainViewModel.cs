using AWSK.Models;
using AWSK.Stores;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace AWSK.ViewModels
{
	class MainViewModel
	{
		private List<int> enemyIdList = new List<int>();

		#region プロパティ(ReactiveProperty)
		// trueにすると画面を閉じる
		public ReactiveProperty<bool> CloseWindow { get; } = new ReactiveProperty<bool>(false);
		// 基地航空隊をどれほど送り込むか？
		// 基地航空隊を飛ばしたか？
		public ReactiveProperty<int> BasedAirUnit1Mode { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnit2Mode { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnit3Mode { get; } = new ReactiveProperty<int>(0);
		// 基地航空隊を飛ばしたか？
		public ReadOnlyReactiveProperty<bool> BasedAirUnit1Flg { get; }
		public ReadOnlyReactiveProperty<bool> BasedAirUnit2Flg { get; }
		public ReadOnlyReactiveProperty<bool> BasedAirUnit3Flg { get; }
		// 基地航空隊の装備の選択番号
		public ReactiveProperty<int> BasedAirUnitIndex11 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex12 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex13 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex14 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex21 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex22 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex23 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex24 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex31 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex32 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex33 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitIndex34 { get; } = new ReactiveProperty<int>(0);
		// 基地航空隊の艦載機熟練度
		public ReactiveProperty<int> BasedAirUnitMas11 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas12 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas13 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas14 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas21 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas22 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas23 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas24 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas31 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas32 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas33 { get; } = new ReactiveProperty<int>(7);
		public ReactiveProperty<int> BasedAirUnitMas34 { get; } = new ReactiveProperty<int>(7);
		// 基地航空隊の装備改修度
		public ReactiveProperty<int> BasedAirUnitRf11 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf12 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf13 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf14 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf21 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf22 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf23 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf24 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf31 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf32 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf33 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> BasedAirUnitRf34 { get; } = new ReactiveProperty<int>(0);
		// 敵艦隊の艦の選択番号
		public ReactiveProperty<int> EnemyUnitIndex1 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex2 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex3 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex4 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex5 { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<int> EnemyUnitIndex6 { get; } = new ReactiveProperty<int>(0);
		// 基地航空隊・敵艦隊の制空値
		public ReactiveProperty<string> BasedAirUnit1AAV { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> BasedAirUnit2AAV { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> BasedAirUnit3AAV { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> EnemyUnitAAV { get; } = new ReactiveProperty<string>("");
		// シミュレーションの反復回数
		public ReactiveProperty<int> SimulationCountIndex { get; } = new ReactiveProperty<int>(1);
		#endregion
		#region プロパティ(ReadOnlyReactiveCollection)
		// 艦載機熟練度の一覧
		public ReadOnlyReactiveCollection<string> MasList { get; }
		// 装備改修度の一覧
		public ReadOnlyReactiveCollection<string> RfList { get; }
		// 深海棲艦の艦名一覧
		public ReadOnlyReactiveCollection<string> EnemyList { get; }
		// 基地航空隊に使用できる装備の一覧
		public ReadOnlyReactiveCollection<string> BasedAirUnitList { get; }
		#endregion

		#region コマンド
		// シミュレーションを実行
		public ReactiveCommand RunSimulationCommand { get; }
		public ReactiveCommand UpdateDatabaseCommand { get; } = new ReactiveCommand();
		#endregion

		// その他初期化用コード
		private async void Initialize() {
			// データベースの初期化
			var status = await DataStore.Initialize();
			switch (status) {
			case DataStoreStatus.Exist:
				break;
			case DataStoreStatus.Success:
				MessageBox.Show("ダウンロードに成功しました。", "AWSK");
				break;
			case DataStoreStatus.Failed:
				MessageBox.Show("ダウンロードに失敗しました。", "AWSK");
				CloseWindow.Value = true;
				return;
			}
		}
		// 基地航空隊のデータを取得
		private BasedAirUnitData GetBasedAirUnitData() {
			// 準備
			var basedAirUnitFlgList = new[] { BasedAirUnit1Flg.Value, BasedAirUnit2Flg.Value, BasedAirUnit3Flg.Value };
			var basedAirUnitModeList = new[] { BasedAirUnit1Mode.Value, BasedAirUnit2Mode.Value, BasedAirUnit3Mode.Value };
			var basedAirUnitIndex = new[] {
					BasedAirUnitIndex11.Value, BasedAirUnitIndex12.Value, BasedAirUnitIndex13.Value, BasedAirUnitIndex14.Value,
					BasedAirUnitIndex21.Value, BasedAirUnitIndex22.Value, BasedAirUnitIndex23.Value, BasedAirUnitIndex24.Value,
					BasedAirUnitIndex31.Value, BasedAirUnitIndex32.Value, BasedAirUnitIndex33.Value, BasedAirUnitIndex34.Value,
				};
			var basedAirUnitMas = new[] {
					BasedAirUnitMas11.Value, BasedAirUnitMas12.Value, BasedAirUnitMas13.Value, BasedAirUnitMas14.Value,
					BasedAirUnitMas21.Value, BasedAirUnitMas22.Value, BasedAirUnitMas23.Value, BasedAirUnitMas24.Value,
					BasedAirUnitMas31.Value, BasedAirUnitMas32.Value, BasedAirUnitMas33.Value, BasedAirUnitMas34.Value,
				};
			var basedAirUnitRf = new[] {
					BasedAirUnitRf11.Value, BasedAirUnitRf12.Value, BasedAirUnitRf13.Value, BasedAirUnitRf14.Value,
					BasedAirUnitRf21.Value, BasedAirUnitRf22.Value, BasedAirUnitRf23.Value, BasedAirUnitRf24.Value,
					BasedAirUnitRf31.Value, BasedAirUnitRf32.Value, BasedAirUnitRf33.Value, BasedAirUnitRf34.Value,
				};
			// 作成
			var basedAirUnitData = new BasedAirUnitData();
			for (int i = 0; i < basedAirUnitFlgList.Count(); ++i) {
				// チェックを入れてない編成は無視する
				if (!basedAirUnitFlgList[i])
					continue;
				//
				var temp = new List<WeaponData>();
				for (int j = 0; j < 4; ++j) {
					int index = i * 4 + j;
					// 「なし」が選択されている装備は無視する
					if (basedAirUnitIndex[index] == 0)
						continue;
					// 装備名を取り出す
					string name = BasedAirUnitList[basedAirUnitIndex[index]];
					// 装備名から装備情報を得る
					var weapon = DataStore.WeaponDataByName(name);
					weapon.Mas = basedAirUnitMas[index];
					weapon.Rf = basedAirUnitRf[index];
					temp.Add(weapon);
				}
				if (temp.Count > 0) {
					basedAirUnitData.Weapon.Add(temp);
					basedAirUnitData.SallyCount.Add(basedAirUnitModeList[i]);
				}
			}
			return basedAirUnitData;
		}
		// 敵艦隊のデータを取得
		private FleetData GetEnemyData() {
			var fleetData = new FleetData();
			// 艦隊データを格納するインスタンスを作成
			var kammusuList = new List<KammusuData>();
			// 準備
			var enemyUnitIndex = new[] {
				EnemyUnitIndex1.Value, EnemyUnitIndex2.Value, EnemyUnitIndex3.Value,
				EnemyUnitIndex4.Value, EnemyUnitIndex5.Value, EnemyUnitIndex6.Value,
				};
			// 作成
			foreach(int enemyIndex in enemyUnitIndex) {
				// 「なし」が選択されている敵艦は無視する
				if (enemyIndex == 0)
					continue;
				// idを算出する
				int enemyId = enemyIdList[enemyIndex];
				// idから敵艦情報を得る
				var enemy = DataStore.KammusuDataById(enemyId, true);
				// 追加
				kammusuList.Add(enemy);
			}
			// インスタンスを代入
			if (kammusuList.Count > 0)
				fleetData.Kammusu.Add(kammusuList);
			return fleetData;
		}
		// 装備名から、基地航空隊の中隊を選択する(デバッグ用)
		// LINQにおけるIndexの取り方：
		// http://shirakamisauto.hatenablog.com/entry/2016/06/27/080017
		private int BAUIndex(string name) {
			return BasedAirUnitList
				.Select((c, i) => new { Content = c, Index = i })
				.Where(pair => pair.Content == name)
				.Select(pair => pair.Index)
				.First();
		}
		// 艦名から、敵艦を選択する(デバッグ用)
		private int EUIndex(string name) {
			return EnemyList
				.Select((c, i) => new { Content = c, Index = i })
				.Where(pair => pair.Content == name)
				.Select(pair => pair.Index)
				.First();
		}
		// 基地航空隊のうち、どのデータが選択されているかを検出する
		// 例：戻り値が{0, -1, 1}だった場合、インデックス0が第1航空隊、
		// インデックス1が第3航空隊で、第2航空隊は有効になっていない
		private List<int> GetBasedAirUnitIndex() {
			var output = new List<int> { -1, -1, -1 };
			var basedAirUnitFlgList = new[] { BasedAirUnit1Flg.Value, BasedAirUnit2Flg.Value, BasedAirUnit3Flg.Value };
			var basedAirUnitIndex = new[] {
				BasedAirUnitIndex11.Value, BasedAirUnitIndex12.Value, BasedAirUnitIndex13.Value, BasedAirUnitIndex14.Value,
				BasedAirUnitIndex21.Value, BasedAirUnitIndex22.Value, BasedAirUnitIndex23.Value, BasedAirUnitIndex24.Value,
				BasedAirUnitIndex31.Value, BasedAirUnitIndex32.Value, BasedAirUnitIndex33.Value, BasedAirUnitIndex34.Value,
			};
			int sum = 0;
			for (int i = 0; i < basedAirUnitFlgList.Count(); ++i) {
				if (!basedAirUnitFlgList[i])
					continue;
				bool enableFlg = false;
				for (int j = 0; j < 4; ++j) {
					int index = i * 4 + j;
					// 「なし」が選択されている装備は無視する
					if (basedAirUnitIndex[index] == 0)
						continue;
					enableFlg = true;
					break;
				}
				if (enableFlg) {
					output[i] = sum;
					++sum;
				}
			}
			return output;
		}
		// 基地航空隊の指定されたインデックスにおける制空値を返す
		private int GetBasedAirUnitAAV(int index) {
			var bauData = GetBasedAirUnitData();
			var bauIndex = GetBasedAirUnitIndex();
			if (bauIndex[index] < 0) {
				return 0;
			}
			return Simulator.CalcAntiAirValue(bauData.Weapon[bauIndex[index]], bauData.GetSlotData()[bauIndex[index]]);
		}
		// 基地航空隊の指定されたインデックスにおける戦闘行動半径を返す
		private int GetBasedAirUnitRange(int index) {
			var bauData = GetBasedAirUnitData();
			var bauIndex = GetBasedAirUnitIndex();
			if (bauIndex[index] < 0) {
				return 0;
			}
			return Simulator.CalcBAURange(bauData.Weapon[bauIndex[index]]);
		}
		// 敵艦隊の制空値を返す
		private int GetEnemyUnitAAV() {
			var enemyData = GetEnemyData();
			return Simulator.CalcAntiAirValue(enemyData, enemyData.GetSlotData());
		}
		// 基地航空隊の制空値変更処理
		private void ReCalcBasedAirUnit1AAV() {
			int aav1 = GetBasedAirUnitAAV(0);
			int aav2 = GetEnemyUnitAAV();
			int range = GetBasedAirUnitRange(0);
			BasedAirUnit1AAV.Value = $"制空値：{aav1}({Simulator.JudgeAirWarStatusStr(aav1, aav2)})　戦闘行動半径：{range}";
		}
		private void ReCalcBasedAirUnit2AAV() {
			int aav1 = GetBasedAirUnitAAV(1);
			int aav2 = GetEnemyUnitAAV();
			int range = GetBasedAirUnitRange(1);
			BasedAirUnit2AAV.Value = $"制空値：{aav1}({Simulator.JudgeAirWarStatusStr(aav1, aav2)})　戦闘行動半径：{range}";
		}
		private void ReCalcBasedAirUnit3AAV() {
			int aav1 = GetBasedAirUnitAAV(2);
			int aav2 = GetEnemyUnitAAV();
			int range = GetBasedAirUnitRange(2);
			BasedAirUnit3AAV.Value = $"制空値：{aav1}){Simulator.JudgeAirWarStatusStr(aav1, aav2)})　戦闘行動半径：{range}";
		}
		// 敵艦隊の制空値変更処理
		private void ReCalcEnemyUnitAAV() {
			EnemyUnitAAV.Value = $"制空値：{GetEnemyUnitAAV()}";
			ReCalcBasedAirUnit1AAV();
			ReCalcBasedAirUnit2AAV();
			ReCalcBasedAirUnit3AAV();
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
		// 航空戦をシミュレートする
		public void RunSimulation() {
			// 基地航空隊のデータを取得
			var basedAirUnitData = GetBasedAirUnitData();
			// 敵艦隊のデータを取得
			var enemyData = GetEnemyData();
			// シミュレーションを行う
			var simulationCount = new[] { 1000, 10000, 100000, 1000000 };
			{
				Dictionary<int, int> finalAAV;
				List<List<List<int>>> awsCount;
				Simulator.BasedAirUnitSimulation(basedAirUnitData, enemyData, simulationCount[SimulationCountIndex.Value], out finalAAV, out awsCount);
				var vm = new ResultViewModel(finalAAV, awsCount);
				var view = new Views.ResultView { DataContext = vm };
				view.Show();
			}
		}
		// データベースを更新する
		public async void UpdateDatabase() {
			// データベースの初期化
			var status = await DataStore.Initialize(true);
			switch (status) {
			case DataStoreStatus.Success:
				MessageBox.Show("ダウンロードに成功しました。", "AWSK");
				break;
			case DataStoreStatus.Failed:
				MessageBox.Show("ダウンロードに失敗しました。", "AWSK");
				break;
			}
		}

		// コンストラクタ
		public MainViewModel() {
			// その他初期化
			Initialize();
			// プロパティ・コマンドを設定
			BasedAirUnit1Flg = BasedAirUnit1Mode.Select(x => x != 0).ToReadOnlyReactiveProperty();
			BasedAirUnit2Flg = BasedAirUnit2Mode.Select(x => x != 0).ToReadOnlyReactiveProperty();
			BasedAirUnit3Flg = BasedAirUnit3Mode.Select(x => x != 0).ToReadOnlyReactiveProperty();
			#region 各種ReactiveCollection
			{
				var oc = new ObservableCollection<string>(new List<string> {
					"--", "|", "||", "|||", "/", "//", "///", ">>"
				});
				MasList = oc.ToReadOnlyReactiveCollection();
			}
			{
				var oc = new ObservableCollection<string>(new List<string> {
					"0", "1", "2", "3", "4", "5",
					"6", "7", "8", "9", "10"});
				RfList = oc.ToReadOnlyReactiveCollection();
			}
			{
				var list = DataStore.EnemyNameList();
				// 敵艦名のリストから、新たに表示用リストを作成
				var list2 = new List<string>();
				enemyIdList = new List<int>();
				list2.Add("なし");
				enemyIdList.Add(0);
				foreach (var pair in list) {
					int count = list.Values.Where(p => p == pair.Value).Count();
					int count2 = list2.Where(p => p.Contains(pair.Value)).Count();
					if (count > 1) {
						list2.Add($"{pair.Value}-{count2 + 1}");
					} else {
						list2.Add(pair.Value);
					}
					enemyIdList.Add(pair.Key);
				}
				var oc = new ObservableCollection<string>(list2);
				EnemyList = oc.ToReadOnlyReactiveCollection();
			}
			{
				var oc = new ObservableCollection<string>(DataStore.BasedAirUnitNameList());
				BasedAirUnitList = oc.ToReadOnlyReactiveCollection();
			}
			#endregion
			#region 設定変更により制空値を自動計算する処理
			BasedAirUnitIndex11.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitIndex12.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitIndex13.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitIndex14.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitIndex21.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitIndex22.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitIndex23.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitIndex24.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitIndex31.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitIndex32.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitIndex33.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitIndex34.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitMas11.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitMas12.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitMas13.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitMas14.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitMas21.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitMas22.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitMas23.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitMas24.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitMas31.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitMas32.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitMas33.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitMas34.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitRf11.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitRf12.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitRf13.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitRf14.Subscribe(_ => ReCalcBasedAirUnit1AAV());
			BasedAirUnitRf21.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitRf22.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitRf23.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitRf24.Subscribe(_ => ReCalcBasedAirUnit2AAV());
			BasedAirUnitRf31.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitRf32.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitRf33.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			BasedAirUnitRf34.Subscribe(_ => ReCalcBasedAirUnit3AAV());
			EnemyUnitIndex1.Subscribe(_ => ReCalcEnemyUnitAAV());
			EnemyUnitIndex2.Subscribe(_ => ReCalcEnemyUnitAAV());
			EnemyUnitIndex3.Subscribe(_ => ReCalcEnemyUnitAAV());
			EnemyUnitIndex4.Subscribe(_ => ReCalcEnemyUnitAAV());
			EnemyUnitIndex5.Subscribe(_ => ReCalcEnemyUnitAAV());
			EnemyUnitIndex6.Subscribe(_ => ReCalcEnemyUnitAAV());
			#endregion
			RunSimulationCommand = new[] { BasedAirUnit1Flg, BasedAirUnit2Flg, BasedAirUnit3Flg }
				.CombineLatest(x => x.Any(y => y)).ToReactiveCommand();
			RunSimulationCommand.Subscribe(RunSimulation);
			UpdateDatabaseCommand.Subscribe(UpdateDatabase);
			// デバッグ用に編成を予めセットしておく
			// 参考：http://5-4.blog.jp/archives/1063413608.html
			BasedAirUnit2Mode.Value = 1;
			BasedAirUnitIndex21.Value = BAUIndex("烈風改");
			BasedAirUnitIndex22.Value = BAUIndex("一式陸攻(野中隊)");
			BasedAirUnitIndex23.Value = BAUIndex("一式陸攻 二二型甲");
			BasedAirUnitIndex24.Value = BAUIndex("一式陸攻 二二型甲");
			EnemyUnitIndex1.Value = EUIndex("空母ヲ級改flagship-4");
			EnemyUnitIndex2.Value = EUIndex("戦艦タ級flagship");
			EnemyUnitIndex3.Value = EUIndex("重巡ネ級elite");
			EnemyUnitIndex4.Value = EUIndex("軽巡ツ級elite");
		}
	}
}
