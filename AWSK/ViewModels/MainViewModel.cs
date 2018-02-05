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
		public List<ReactiveProperty<int>> BasedAirUnitMode { get; } = new List<ReactiveProperty<int>>();
		// 基地航空隊を飛ばしたか？
		public List<ReadOnlyReactiveProperty<bool>> BasedAirUnitFlg { get; } = new List<ReadOnlyReactiveProperty<bool>>();
		// 基地航空隊の装備の選択番号
		public List<List<ReactiveProperty<int>>> BasedAirUnitIndex { get; } = new List<List<ReactiveProperty<int>>>();
		// 基地航空隊の艦載機熟練度
		public List<List<ReactiveProperty<int>>> BasedAirUnitMas { get; } = new List<List<ReactiveProperty<int>>>();
		// 基地航空隊の装備改修度
		public List<List<ReactiveProperty<int>>> BasedAirUnitRf { get; } = new List<List<ReactiveProperty<int>>>();
		// 敵艦隊の艦の選択番号
		public List<ReactiveProperty<int>> EnemyUnitIndex { get; } = new List<ReactiveProperty<int>>();
		// 基地航空隊・敵艦隊の制空値
		public List<ReactiveProperty<string>> BasedAirUnitAAV { get; } = new List<ReactiveProperty<string>>();
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
		// データベースを更新
		public ReactiveCommand UpdateDatabaseCommand { get; } = new ReactiveCommand();
		// 右クリックから敵編成を表示
		public ReactiveCommand ShowEnemyUnitCommand { get; } = new ReactiveCommand();
		// 右クリックから敵編成を読み込み
		public ReactiveCommand LoadEnemyUnitCommand { get; } = new ReactiveCommand();
		// 右クリックから敵編成を保存
		public ReactiveCommand SaveEnemyUnitCommand { get; } = new ReactiveCommand();
		// 右クリックから基地航空隊を読み込み
		public ReactiveCommand LoadBasedAirUnitCommand { get; } = new ReactiveCommand();
		// 右クリックから基地航空隊を保存
		public ReactiveCommand SaveBasedAirUnitCommand { get; } = new ReactiveCommand();
		#endregion
		#region メソッド(基地航空隊用)
		// 基地航空隊のデータを取得
		private BasedAirUnitData GetBasedAirUnitData() {
			// 作成
			var basedAirUnitData = new BasedAirUnitData();
			for (int ui = 0; ui < 3; ++ui) {
				// チェックを入れてない編成は無視する
				if (!BasedAirUnitFlg[ui].Value)
					continue;
				//
				var temp = new List<WeaponData>();
				for (int wi = 0; wi < 4; ++wi) {
					// 「なし」が選択されている装備は無視する
					if (BasedAirUnitIndex[ui][wi].Value == 0)
						continue;
					// 装備名を取り出す
					string name = BasedAirUnitList[BasedAirUnitIndex[ui][wi].Value];
					// 装備名から装備情報を得る
					var weapon = DataStore.WeaponDataByName(name);
					weapon.Mas = BasedAirUnitMas[ui][wi].Value;
					weapon.Rf = BasedAirUnitRf[ui][wi].Value;
					temp.Add(weapon);
				}
				if (temp.Count > 0) {
					basedAirUnitData.Weapon.Add(temp);
					basedAirUnitData.SallyCount.Add(BasedAirUnitMode[ui].Value);
				}
			}
			return basedAirUnitData;
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
		// 基地航空隊のうち、どのデータが選択されているかを検出する
		// 例：戻り値が{0, -1, 1}だった場合、インデックス0が第1航空隊、
		// インデックス1が第3航空隊で、第2航空隊は有効になっていない
		private List<int> GetBasedAirUnitIndex() {
			var output = new List<int> { -1, -1, -1 };
			int sum = 0;
			for (int ui = 0; ui < 3; ++ui) {
				if (!BasedAirUnitFlg[ui].Value)
					continue;
				bool enableFlg = false;
				for (int wi = 0; wi < 4; ++wi) {
					int index = ui * 4 + wi;
					// 「なし」が選択されている装備は無視する
					if (BasedAirUnitIndex[ui][wi].Value == 0)
						continue;
					enableFlg = true;
					break;
				}
				if (enableFlg) {
					output[ui] = sum;
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
		// 基地航空隊の制空値変更処理
		private void ReCalcBasedAirUnitAAV(int ui) {
			int aav1 = GetBasedAirUnitAAV(ui);
			int aav2 = GetEnemyUnitAAV();
			int range = GetBasedAirUnitRange(ui);
			BasedAirUnitAAV[ui].Value = $"制空値：{aav1}({Simulator.JudgeAirWarStatusStr(aav1, aav2)})　戦闘行動半径：{range}";
		}
		// 基地航空隊の読み込み処理
		private void LoadBasedAirUnit() {
			var ofd = new Microsoft.Win32.OpenFileDialog() {
				FileName = "bau_data.bau",
				Filter = "基地航空隊情報(*.bau)|*.bau|すべてのファイル(*.*)|*.*",
				Title = "読み込むのファイルを選択"
			};
			if ((bool)ofd.ShowDialog()) {
				try {
					// ファイルを読み込み
					using (var sr = new System.IO.StreamReader(ofd.FileName)) {
						// テキストとして読み込んでパース
						string output = sr.ReadToEnd();
						var bauData = new BasedAirUnitData(output);
						#region 基地航空隊の情報を初期化
						for(int ui = 0; ui < 3; ++ui) {
							BasedAirUnitMode[ui].Value = 0;
							for(int wi = 0; wi < 4; ++wi) {
								BasedAirUnitIndex[ui][wi].Value = 0;
								BasedAirUnitMas[ui][wi].Value = 0;
								BasedAirUnitRf[ui][wi].Value = 0;
							}
						}
						#endregion
						// 基地航空隊の情報を書き込む
						for (int ui = 0; ui < bauData.Weapon.Count; ++ui) {
							// 出撃回数
							int count = bauData.SallyCount[ui];
							BasedAirUnitMode[ui].Value = count;
							// 装備情報
							var weaponList = bauData.Weapon[ui];
							for (int wi = 0; wi < weaponList.Count; ++wi) {
								var weapon = weaponList[wi];
								BasedAirUnitIndex[ui][wi].Value = BasedAirUnitList.IndexOf(weapon.Name);
								BasedAirUnitMas[ui][wi].Value = weapon.Mas;
								BasedAirUnitRf[ui][wi].Value = weapon.Rf;
							}
						}
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					MessageBox.Show("ファイルを開けませんでした", "AWSK");
				}
			}
		}
		// 基地航空隊の保存処理
		private void SaveBasedAirUnit() {
			var sfd = new Microsoft.Win32.SaveFileDialog() {
				FileName = "bau_data.bau",
				Filter = "基地航空隊情報(*.bau)|*.bau|すべてのファイル(*.*)|*.*",
				Title = "保存先のファイルを選択"
			};
			if ((bool)sfd.ShowDialog()) {
				try {
					string output = GetBasedAirUnitData().GetJsonData();
					using (var sw = new System.IO.StreamWriter(sfd.FileName)) {
						sw.Write(output);
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					MessageBox.Show("ファイルに保存できませんでした", "AWSK");
				}
			}
		}
		#endregion
		#region メソッド(敵艦隊用)
		// 敵艦隊のデータを取得
		private FleetData GetEnemyData() {
			var fleetData = new FleetData();
			// 艦隊データを格納するインスタンスを作成
			var kammusuList = new List<KammusuData>();
			// 作成
			for(int ki = 0; ki < 6; ++ki) {
				int enemyIndex = EnemyUnitIndex[ki].Value;
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
		// 艦名から、敵艦を選択する(デバッグ用)
		private int EUIndex(string name) {
			return EnemyList
				.Select((c, i) => new { Content = c, Index = i })
				.Where(pair => pair.Content == name)
				.Select(pair => pair.Index)
				.First();
		}
		// 敵艦隊の制空値を返す
		private int GetEnemyUnitAAV() {
			var enemyData = GetEnemyData();
			return Simulator.CalcAntiAirValue(enemyData, enemyData.GetSlotData(), true);
		}
		// 敵艦隊の制空値変更処理
		private void ReCalcEnemyUnitAAV() {
			EnemyUnitAAV.Value = $"制空値：{GetEnemyUnitAAV()}";
			for(int ui = 0; ui < 3; ++ui) {
				ReCalcBasedAirUnitAAV(ui);
			}
		}
		// 敵艦隊の読み込み処理
		private void LoadEnemyUnit() {
			var ofd = new Microsoft.Win32.OpenFileDialog() {
				FileName = "enemy_data.enemy",
				Filter = "敵艦隊情報(*.enemy)|*.enemy|すべてのファイル(*.*)|*.*",
				Title = "読み込むのファイルを選択"
			};
			if ((bool)ofd.ShowDialog()) {
				try {
					// ファイルを読み込み
					using (var sr = new System.IO.StreamReader(ofd.FileName)) {
						// テキストとして読み込んでパース
						string output = sr.ReadToEnd();
						var enemyData = new FleetData(output);
						// 敵艦隊の情報を初期化
						for (int ki = 0; ki < 6; ++ki){
							EnemyUnitIndex[ki].Value = 0;
						}
						// IDとリストの表示位置との対応表を入手
						var enemyNameList = DataStore.EnemyNameList().ToList().Select(pair => pair.Key).ToList();
						// 敵艦隊の情報を書き込む
						if (enemyData.Kammusu.Count > 0) {
							for(int ki = 0; ki < enemyData.Kammusu[0].Count; ++ki) {
								var kammusu = enemyData.Kammusu[0][ki];
								int index = enemyNameList.IndexOf(kammusu.Id) + 1;
								EnemyUnitIndex[ki].Value = index;
							}
						}
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					MessageBox.Show("ファイルを開けませんでした", "AWSK");
				}
			}
		}
		// 敵艦隊の保存処理
		private void SaveEnemyUnit() {
			var sfd = new Microsoft.Win32.SaveFileDialog() {
				FileName = "enemy_data.enemy",
				Filter = "敵艦隊情報(*.enemy)|*.enemy|すべてのファイル(*.*)|*.*",
				Title = "保存先のファイルを選択"
			};
			if ((bool)sfd.ShowDialog()) {
				try {
					string output = GetEnemyData().GetJsonData();
					using (var sw = new System.IO.StreamWriter(sfd.FileName)) {
						sw.Write(output);
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					MessageBox.Show("ファイルに保存できませんでした", "AWSK");
				}
			}
		}
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
			// ReactivePropertyを設定
			//BasedAirUnitModeとBasedAirUnitFlgとBasedAirUnitAAV
			//(BasedAirUnitFlgはBasedAirUnitModeから作られる)
			for (int ui = 0; ui < 3; ++ui) {
				var rp = new ReactiveProperty<int>(0);
				var rp2 = rp.Select(x => x != 0).ToReadOnlyReactiveProperty();
				BasedAirUnitMode.Add(rp);
				BasedAirUnitFlg.Add(rp2);
				//
				BasedAirUnitAAV.Add(new ReactiveProperty<string>(""));
			}
			for (int ui = 0; ui < 3; ++ui) {
				// BasedAirUnitIndexとBasedAirUnitMasとBasedAirUnitRf
				var rpList1 = new List<ReactiveProperty<int>>();
				var rpList2 = new List<ReactiveProperty<int>>();
				var rpList3 = new List<ReactiveProperty<int>>();
				for (int wi = 0; wi < 4; ++wi) {
					rpList1.Add(new ReactiveProperty<int>(0));
					rpList2.Add(new ReactiveProperty<int>(7));
					rpList3.Add(new ReactiveProperty<int>(0));
				}
				BasedAirUnitIndex.Add(rpList1);
				BasedAirUnitMas.Add(rpList2);
				BasedAirUnitRf.Add(rpList3);
			}
			for (int ki = 0; ki < 6; ++ki) {
				EnemyUnitIndex.Add(new ReactiveProperty<int>(0));
			}
			for (int ui = 0; ui < 3; ++ui) {
				for (int wi = 0; wi < 4; ++wi) {
					// 変更時に制空値を自動計算する処理
					/* このコードだと、実行時、コンボボックスを動かした際「uiになぜか3が代入されて」
					 * エラーが出てしまう。ファッキュー！ファッキュー！
					 * rp1.Subscribe(_ => ReCalcBasedAirUnitAAV(ui));
					 * rp2.Subscribe(_ => ReCalcBasedAirUnitAAV(ui));
					 * rp3.Subscribe(_ => ReCalcBasedAirUnitAAV(ui));
					 */
					switch (ui) {
					case 0:
						BasedAirUnitIndex[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(0));
						BasedAirUnitMas[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(0));
						BasedAirUnitRf[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(0));
						break;
					case 1:
						BasedAirUnitIndex[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(1));
						BasedAirUnitMas[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(1));
						BasedAirUnitRf[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(1));
						break;
					case 2:
						BasedAirUnitIndex[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(2));
						BasedAirUnitMas[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(2));
						BasedAirUnitRf[ui][wi].Subscribe(_ => ReCalcBasedAirUnitAAV(2));
						break;
					}
				}
			}
			for (int ki = 0; ki < 6; ++ki) {
				EnemyUnitIndex[ki].Subscribe(_ => ReCalcEnemyUnitAAV());
			}
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
			// コマンドを設定
			RunSimulationCommand = BasedAirUnitFlg
				.CombineLatest(x => x.Any(y => y)).ToReactiveCommand();
			RunSimulationCommand.Subscribe(RunSimulation);
			UpdateDatabaseCommand.Subscribe(UpdateDatabase);
			ShowEnemyUnitCommand.Subscribe(_ => { MessageBox.Show("敵編成：\n" + GetEnemyData().ToString(), "AWSK"); });
			LoadEnemyUnitCommand.Subscribe(_ => LoadEnemyUnit());
			SaveEnemyUnitCommand.Subscribe(_ => SaveEnemyUnit());
			LoadBasedAirUnitCommand.Subscribe(_ => LoadBasedAirUnit());
			SaveBasedAirUnitCommand.Subscribe(_ => SaveBasedAirUnit());
		}
	}
}
