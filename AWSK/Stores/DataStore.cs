using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AWSK.Stores
{
	static class DataStore
	{
		// 接続用文字列
		private const string connectionString = @"Data Source=GameData.db";
		// 装備の種類(type1)の名称
		private static Dictionary<string, int> WeaponType1 = new Dictionary<string, int> {
			{ "主砲", 1},
			{ "魚雷", 2},
			{ "艦上機", 3},
			{ "対空装備", 4},
			{ "索敵装備", 5},
			{ "缶・女神・バルジ", 6},
			{ "対潜装備", 7},
			{ "大発・探照灯", 8},
			{ "ドラム缶", 9},
			{ "艦艇修理施設", 10},
			{ "照明弾", 11},
			{ "艦隊司令部", 12},
			{ "航空要員", 13},
			{ "高射装置", 14},
			{ "WG42", 15},
			{ "熟練見張員", 16},
			{ "大型飛行艇", 17},
			{ "戦闘糧食", 18},
			{ "洋上補給", 19},
			{ "特二式内火艇", 20},
			{ "陸上攻撃機", 21},
			{ "局地戦闘機", 22},
			{ "分解済彩雲", 23},
			{ "潜水艦電探", 24},
		};
		// 装備URLとIDとの対応表(深海棲艦向け)
		private static Dictionary<string, int> weaponUrlDicWikia = new Dictionary<string, int>();

		// 敵艦のカテゴリを取得する
		public static Dictionary<string, string> ParseEnemyListWikia(string rawData) {
			var category = new Dictionary<string, string>();
			// テキストデータをパースする
			var doc = default(IHtmlDocument);
			var parser = new HtmlParser();
			doc = parser.Parse(rawData);
			// パースしたデータからカテゴリ名とURLのペアを引き出す
			var categoryTemp = doc.QuerySelectorAll("table.wikitable > tbody > tr > td")
				.Where(item => item.QuerySelector("b > a") != null && item.QuerySelector("p") != null)
				.Select(item => {
					string url = $"http://kancolle.wikia.com{item.QuerySelector("b > a").GetAttribute("href")}";
					string temp = item.QuerySelector("p > span").TextContent;
					string name = item.QuerySelector("p").TextContent.Replace(temp, "").Replace("\n", "");
					return new KeyValuePair<string, string>(name, url);
				});
			foreach(var pair in categoryTemp) {
				category[pair.Key] = pair.Value;
			}
			return category;
		}
		// 装備URLとIDとの対応表(深海棲艦向け)
		public static async Task GetWeaponDicWikia() {
			using (var client = new System.Net.Http.HttpClient()) {
				string rawData = await client.GetStringAsync("http://kancolle.wikia.com/wiki/List_of_equipment_used_by_the_enemy");
				var doc = default(IHtmlDocument);
				var parser = new HtmlParser();
				doc = parser.Parse(rawData);
				weaponUrlDicWikia = doc.QuerySelectorAll("table.wikitable.typography-xl-optout > tbody > tr")
					.Where(item => item.QuerySelector("td") != null )
					.Select(item => {
					int id = int.Parse(item.QuerySelector("td").TextContent);
					string url = "http://kancolle.wikia.com" + item.QuerySelectorAll("td").Skip(2).First().QuerySelector("a").Attributes["href"].Value;
					return new KeyValuePair<string, int>(url, id);
				}).ToDictionary(item => item.Key, item => item.Value);
			}
		}
		// 敵艦のデータを取得する
		public static List<KammusuData> GetKammusuDataWikia(string rawData) {
			// テキストデータをパースする
			var doc = default(IHtmlDocument);
			var parser = new HtmlParser();
			doc = parser.Parse(rawData);
			// パースしたデータから敵艦のデータを引き出す
			var kammusuData = doc.QuerySelectorAll("table.infobox-ship > tbody")
				.Where(item => {
					string nameText = item.QuerySelector("tr > td > div > b").TextContent;
					int dotIndex = nameText.IndexOf(".");
					int spaceIndex = nameText.IndexOf(" ");
					string id = nameText.Substring(dotIndex + 1, spaceIndex - dotIndex - 1);
					if (!int.TryParse(id, out int _))
						return false;
					//
					var temp = item.QuerySelectorAll("td").Select(item2 => item2.TextContent.Replace("\n", "").Replace(" ", "")).ToList();
					string str = temp[temp.IndexOf("AA") + 1];
					return int.TryParse(str, out int _);
				})
				.Select(item => {
					// 艦名のテキストから、艦名とIDを取得
					string nameText = item.QuerySelector("tr > td > div > b").TextContent;
					int dotIndex = nameText.IndexOf(".");
					int spaceIndex = nameText.IndexOf(" ");
					int no = int.Parse(nameText.Substring(dotIndex + 1, spaceIndex - dotIndex - 1));
					string name = nameText.Substring(spaceIndex + 1);
					// 対空値を読み取る
					var temp = item.QuerySelectorAll("td").Select(item2 => item2.TextContent.Replace("\n", "").Replace(" ", "")).ToList();
					int antiAir = int.Parse(temp[temp.IndexOf("AA") + 1]);
					int slots = int.Parse(temp[temp.IndexOf("Slots") + 2]);
					// 結果を書き込む
					var kammusu = new KammusuData {
						Id = (no < 1000 ? no + 1000 : no),
						Name = name,
						Level = 1,
						KammusuFlg = (no <= 500),
						AntiAir = antiAir,
						SlotSize = slots,
						Slot = new List<int>(),
						Weapon = new List<WeaponData>()
					};
					// 艦種を推定する
					if(kammusu.Name.Contains("駆逐")) {
						kammusu.Type = "駆逐艦";
					}else if (kammusu.Name.Contains("軽巡")) {
						kammusu.Type = "軽巡洋艦";
					} else if (kammusu.Name.Contains("重巡")) {
						if(kammusu.Name == "重巡ネ級flagship") {
							kammusu.Type = "航空巡洋艦";
						} else {
							kammusu.Type = "重巡洋艦";
						}
					} else if (kammusu.Name.Contains("戦艦")) {
						if (kammusu.Name.Contains("レ級") || kammusu.Name.Contains("戦艦仏棲姫")) {
							kammusu.Type = "航空戦艦";
						} else if (kammusu.Name.Contains("タ級")) {
							kammusu.Type = "巡洋戦艦";
						} else {
							kammusu.Type = "戦艦";
						}
					} else if (kammusu.Name.Contains("軽母")) {
						kammusu.Type = "軽空母";
					} else if (kammusu.Name.Contains("空母")) {
						if (kammusu.Name.Contains("装甲空母")) {
							kammusu.Type = "航空戦艦";
						} else {
							kammusu.Type = "正規空母";
						}
					} else if (kammusu.Name.Contains("雷巡")) {
						kammusu.Type = "重雷装巡洋艦";
					} else if (kammusu.Name.Contains("潜水")) {
						kammusu.Type = "潜水艦";
					} else if (kammusu.Name.Contains("水母")) {
						kammusu.Type = "水上機母艦";
					} else if (kammusu.Name.Contains("輸送")) {
						kammusu.Type = "輸送艦";
					}
					// スロットの枠数を読み取る
					int weaponIndex = temp.IndexOf("Equipment") + 3;
					for(int i = 0; i < 4; ++i) {
						// スロット数の文字列を読み取る
						string slotStr = temp[weaponIndex + 1 + i * 3];
						// 文字列が「-」「???」ならループを抜ける
						if (slotStr.Contains("-") || slotStr.Contains("???"))
							break;
						// 文字列を数字だけにしてからint型にパースして読み取る
						var regex = new Regex("[^0-9]");
						slotStr = regex.Replace(slotStr, "");
						kammusu.Slot.Add(int.Parse(slotStr));
					}
					// スロット毎の装備を読み取る
					var trList = item.QuerySelectorAll("tr");
					int equipIndex = trList
						.Select((value, index) => new {Index = index, Value = value})
						.Where(x => x.Value.TextContent.Contains("Equipment"))
						.Select(x => x.Index).First();
					for (int i = 0; i < 4; ++i) {
						// 装備のリンクを読み取る
						var node = trList[equipIndex + i + 1].QuerySelectorAll("td").Skip(1).First().QuerySelector("a");
						if(node == null)
							break;
						string url = "http://kancolle.wikia.com" + node.GetAttribute("href");
						kammusu.Weapon.Add(WeaponDataById(weaponUrlDicWikia[url]));
					}
					return kammusu;
				}
			).ToList();
			return kammusuData;
		}
		// 艦娘のデータをダウンロードする
		private static async Task<bool> DownloadKammusuDataAsync() {
			Console.WriteLine("艦娘のデータをダウンロード中……");
			try {
				// 艦娘データをダウンロード・パースする
				var kammusuList = new List<KammusuData>();	//艦娘データ
				var kammusuDataFlg = new List<bool>();      //艦娘データが不完全ならfalse
				Console.WriteLine("・デッキビルダー");
				using (var client = new HttpClient()) {
					// ダウンロード
					string rawData = await client.GetStringAsync("http://kancolle-calc.net/data/shipdata.js");
					// 余計な文字を削除
					rawData = rawData.Replace("var gShips = ", "");
					// JSONとしてゆるーくパース
					var obj = DynamicJson.Parse(rawData);
					// パース結果をゆるーく取得
					foreach (var kammusu in obj) {
						// IDや艦名などを取得
						int id = int.Parse(kammusu.id);
						string name = kammusu.name;
						string type = kammusu.type;
						int antiair = (int)(kammusu.max_aac);
						int slotsize = (int)(kammusu.slot);
						var slot = kammusu.carry;
						var firstWeapon = kammusu.equip;
						bool kammusuFlg = (id <= 1500);
						// 艦娘・深海棲艦のデータと呼べないものは無視する
						if (name == "なし")
							continue;
						if (id > 1900)
							continue;
						// 不完全なデータかどうかを判断する
						bool kammusuDataFlgTemp = true;
						if (kammusu.next == "" && !kammusuFlg) {
							int count = 0;
							foreach (var wId in firstWeapon) {
								++count;
							}
							if (count == 0) {
								kammusuDataFlgTemp = false;
							}
						}
						// データを登録する
						var kammusuDataTemp = new KammusuData {
							Id = id, AntiAir = antiair, KammusuFlg = kammusuFlg, Name = name, Type = type,
							Slot = new List<int>(), SlotSize = slotsize, Weapon = new List<WeaponData>() };
						foreach (var size in slot) {
							kammusuDataTemp.Slot.Add((int)size);
						}
						foreach (var wId in firstWeapon) {
							kammusuDataTemp.Weapon.Add(new WeaponData { Id = (int)wId });
						}
						kammusuList.Add(kammusuDataTemp);
						kammusuDataFlg.Add(kammusuDataFlgTemp);
					}
				}
				// 不完全データについては、wikiを読み取って補完する
				Console.WriteLine("・英語Wiki");
				using (var client = new HttpClient()) {
					// ダウンロード
					string rawData = await client.GetStringAsync("http://kancolle.wikia.com/wiki/Enemy_Vessel");
					// カテゴリ解析
					var category = ParseEnemyListWikia(rawData);
					category["護衛棲水姫"] = "http://kancolle.wikia.com/wiki/Escort_Water_Princess"; //パッチ
					category["深海鶴棲姫"] = "http://kancolle.wikia.com/wiki/Abyssal_Crane_Princess"; //パッチ
					category["戦艦棲姫改"] = "http://kancolle.wikia.com/wiki/Battleship_Princess_Kai"; //パッチ
					category["戦艦水鬼改"] = "http://kancolle.wikia.com/wiki/Battleship_Water_Demon_Kai"; //パッチ
					// カテゴリ毎にページをクロールしていく
					var kammusuDicWikia = new Dictionary<int, KammusuData>();
					foreach (var pair in category) {
						Console.WriteLine($"　・{pair.Key}");
						string rawData2 = await client.GetStringAsync(pair.Value);
						var list = GetKammusuDataWikia(rawData2);
						foreach (var kammusu in list) {
							kammusuDicWikia[kammusu.Id] = kammusu;
						}
					}
					// クロール結果と比較し、不完全データを補う
					for (int ki = 0; ki < kammusuList.Count; ++ki) {
						if (kammusuDicWikia.ContainsKey(kammusuList[ki].Id)) {
							var kammusuTemp = kammusuDicWikia[kammusuList[ki].Id];
							kammusuTemp.Type = kammusuList[ki].Type;
							kammusuList[ki] = kammusuTemp;
							kammusuDataFlg[ki] = true;
						}
					}
					foreach(var pair in kammusuDicWikia) {
						int id = pair.Key;
						var kammusu = pair.Value;
						if(kammusuList.Count(p => p.Id == id) == 0) {
							kammusuList.Add(kammusu);
							kammusuDataFlg.Add(true);
						}
					}
				}
				// 不完全データに対する追加パッチ
				string patchMemo = "";
				{
					// CSVファイルを読み込み、その中身を記録する
					// 入力チェックはあまりしてないので注意
					if (System.IO.File.Exists(@"KammusuPatch.csv")) {
						using (var sr = new System.IO.StreamReader(@"KammusuPatch.csv")) {
							while (!sr.EndOfStream) {
								try {
									// 1行読み込み、カンマ毎に区切る
									string line = sr.ReadLine();
									var values = line.Split(',');
									// 行数がおかしい場合は飛ばす
									if (values.Count() < 16)
										continue;
									// ヘッダー行は飛ばす
									if (values[0] == "id")
										continue;
									// データを読み取る
									var kammusu = new KammusuData();
									kammusu.Id = int.Parse(values[0]);
									kammusu.Name = values[1];
									kammusu.Type = values[2];
									kammusu.AntiAir = int.Parse(values[3]);
									kammusu.SlotSize = int.Parse(values[4]);
									kammusu.Slot = new List<int>();
									kammusu.Slot.Add(int.Parse(values[5]));
									kammusu.Slot.Add(int.Parse(values[6]));
									kammusu.Slot.Add(int.Parse(values[7]));
									kammusu.Slot.Add(int.Parse(values[8]));
									kammusu.Slot.Add(int.Parse(values[9]));
									kammusu.Weapon = new List<WeaponData>();
									kammusu.Weapon.Add(WeaponDataById(int.Parse(values[10])));
									kammusu.Weapon.Add(WeaponDataById(int.Parse(values[11])));
									kammusu.Weapon.Add(WeaponDataById(int.Parse(values[12])));
									kammusu.Weapon.Add(WeaponDataById(int.Parse(values[13])));
									kammusu.Weapon.Add(WeaponDataById(int.Parse(values[14])));
									kammusu.KammusuFlg = (int.Parse(values[15]) != 0);
									// データを追記する
									int index = kammusuList.FindIndex(k => k.Id == kammusu.Id);
									if (index >= 0) {
										patchMemo += $"・ID：{kammusuList[index].Id} 艦名：{kammusuList[index].Name}\n";
										kammusuList[index] = kammusu;
										kammusuDataFlg[index] = true;
									} else {
										kammusuList.Add(kammusu);
										kammusuDataFlg.Add(true);
									}
								} catch (Exception e) { }
							}
						}
					}
				}
				// SQLコマンドを生成する
				var commandList = new List<string>();
				for (int ki = 0; ki < kammusuList.Count; ++ki) {
					if (!kammusuDataFlg[ki]) {
						continue;
					}
					var kammusu = kammusuList[ki];
					// コマンドを記録する
					string sql = "INSERT INTO Kammusu VALUES(";
					sql += $"{kammusu.Id},'{kammusu.Name}','{kammusu.Type}',{kammusu.AntiAir},{kammusu.SlotSize},";
					{
						int count = 0;
						foreach (int size in kammusu.Slot) {
							sql += $"{size},";
							++count;
						}
						for (int wi = count; wi < 5; ++wi) {
							sql += "0,";
						}
					}
					{
						int count = 0;
						foreach (int wId in kammusu.Weapon.Select(p => p.Id)) {
							sql += $"{wId},";
							++count;
						}
						for (int wi = count; wi < 5; ++wi) {
							sql += "0,";
						}
					}
					sql += $"'{(kammusu.KammusuFlg ? 1 : 0)}')";
					commandList.Add(sql);
				}
				// データベースに書き込む
				using (var con = new SQLiteConnection(connectionString)) {
					con.Open();
					using (var cmd = con.CreateCommand()) {
						// 艦娘データを全削除
						cmd.CommandText = "DELETE FROM Kammusu";
						cmd.ExecuteNonQuery();
						// 書き込み処理
						foreach (string command in commandList) {
							cmd.CommandText = command;
							cmd.ExecuteNonQuery();
						}
					}
				}
				Console.WriteLine("ダウンロード完了");
				if (patchMemo != "") {
					MessageBox.Show($"※パッチで上書きされた艦船：\n{patchMemo}");
					Console.WriteLine($"※パッチで上書きされた艦船：\n{patchMemo}");
				}
				return true;
			} catch (Exception e) {
				Console.WriteLine("ダウンロード失敗");
				Console.WriteLine(e.ToString());
				return false;
			}
		}
		// 装備のデータをダウンロードする
		private static async Task<bool> DownloadWeaponDataAsync() {
			Console.WriteLine("装備のデータをダウンロード中……");
			try {
				// 戦闘行動半径データをダウンロードして読み込む
				var BasedAirUnitRange = new Dictionary<string, int>();
				using (var client = new HttpClient()) {
					using(var stream = await client.GetStreamAsync("https://raw.githubusercontent.com/YSRKEN/AWSK/master/AWSK/WeaponData.csv")) {
						using (var sr = new System.IO.StreamReader(stream)) {
							while (!sr.EndOfStream) {
								string line = sr.ReadLine();
								var values = line.Split(',');
								if (values[0] == "名称")
									continue;
								BasedAirUnitRange[values[0]] = int.Parse(values[1]);
							}
						}
					}
				}
				// 不完全データに対する追加パッチ
				Console.WriteLine("・自前DB");
				string patchMemo = "";
				{
					// CSVファイルを読み込み、その中身を記録する
					// 入力チェックはあまりしてないので注意
					if (System.IO.File.Exists(@"WeaponDataPatch.csv")) {
						using (var sr = new System.IO.StreamReader(@"WeaponDataPatch.csv")) {
							while (!sr.EndOfStream) {
								try {
									// 1行読み込み、カンマ毎に区切る
									string line = sr.ReadLine();
									var values = line.Split(',');
									// 行数がおかしい場合は飛ばす
									if (values.Count() < 2)
										continue;
									// ヘッダー行は飛ばす
									if (values[0] == "名称")
										continue;
									// データを読み取る
									string name = values[0];
									int r = int.Parse(values[1]);
									if (BasedAirUnitRange.ContainsKey(name)) {
										patchMemo += $"・装備名：{name} 戦闘行動半径：{r}\n";
									}
									BasedAirUnitRange[name] = r;
								} catch (Exception e) { }
							}
						}
					}
				}
				// 装備データをダウンロード・パースする
				var commandList = new List<string>();
				Console.WriteLine("・デッキビルダー");
				using (var client = new HttpClient()) {
					// ダウンロード
					string rawData = await client.GetStringAsync("http://kancolle-calc.net/data/itemdata.js");
					// 余計な文字を削除
					rawData = rawData.Replace("var gItems = ", "");
					// JSONとしてゆるーくパース
					var obj = DynamicJson.Parse(rawData);
					// パース結果をゆるーく取得
					foreach (var weapon in obj) {
						// IDや装備名などを取得
						int id = (int)weapon.id;
						string name = weapon.name;
						int antiair = (int)weapon.aac;
						int intercept = 0;
						var type = weapon.type;
						// 局戦・陸戦は迎撃値を読み取るようにした
						if (type[0] == 22 && type[2] == 48) {
							intercept = (int)weapon.evasion;
						}
						// 戦闘行動半径の処理
						int baurange = 0;
						if (BasedAirUnitRange.ContainsKey(name)) {
							baurange = BasedAirUnitRange[name];
						}
						// 艦娘用の装備か？
						bool weaponFlg = (id <= 500);
						// コマンドを記録する
						string sql = "INSERT INTO Weapon VALUES(";
						sql += $"{id},'{name}',{antiair},{intercept},{baurange},";
						sql += $"{type[0]},{type[1]},{type[2]},{type[3]},{type[4]},";
						sql += $"{(weaponFlg ? 1 : 0)})";
						commandList.Add(sql);
					}
				}
				// データベースに書き込む
				using (var con = new SQLiteConnection(connectionString)) {
					con.Open();
					using (var cmd = con.CreateCommand()) {
						// 装備データを全削除
						cmd.CommandText = "DELETE FROM Weapon";
						cmd.ExecuteNonQuery();
						// 書き込み処理
						foreach (string command in commandList) {
							cmd.CommandText = command;
							cmd.ExecuteNonQuery();
						}
					}
				}
				Console.WriteLine("ダウンロード完了");
				if (patchMemo != "") {
					MessageBox.Show($"※パッチで上書きされた装備：\n{patchMemo}");
					Console.WriteLine($"※パッチで上書きされた装備：\n{patchMemo}");
				}
				return true;
			} catch (Exception e) {
				Console.WriteLine("ダウンロード失敗");
				Console.WriteLine(e.ToString());
				return false;
			}
		}

		// データベースの初期化処理
		public static async Task<DataStoreStatus> Initialize(bool forceUpdateFlg = false) {
			// テーブルが存在しない場合、テーブルを作成する
			bool createTableFlg = false;
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データ
					{
						bool hasTableFlg = false;
						cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Kammusu'";
						using (var reader = cmd.ExecuteReader()) {
							if (reader.Read() && reader.GetInt32(0) == 1) {
								hasTableFlg = true;
							}
						}
						if (forceUpdateFlg) {
							string sql = "DROP TABLE [Kammusu]";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
						if (!hasTableFlg || forceUpdateFlg) {
							string sql = @"
								CREATE TABLE [Kammusu](
								[id] INTEGER NOT NULL PRIMARY KEY,
								[name] TEXT NOT NULL DEFAULT '',
								[type] TEXT NOT NULL DEFAULT '',
								[antiair] INTEGER NOT NULL DEFAULT 0,
								[slotsize] INTEGER NOT NULL DEFAULT 0,
								[slot1] INTEGER NOT NULL DEFAULT 0,
								[slot2] INTEGER NOT NULL DEFAULT 0,
								[slot3] INTEGER NOT NULL DEFAULT 0,
								[slot4] INTEGER NOT NULL DEFAULT 0,
								[slot5] INTEGER NOT NULL DEFAULT 0,
								[weapon1] INTEGER NOT NULL DEFAULT 0,
								[weapon2] INTEGER NOT NULL DEFAULT 0,
								[weapon3] INTEGER NOT NULL DEFAULT 0,
								[weapon4] INTEGER NOT NULL DEFAULT 0,
								[weapon5] INTEGER NOT NULL DEFAULT 0,
								[kammusu_flg] INTEGER NOT NULL)
							";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
							createTableFlg = true;
						}
					}
					// 装備データ
					{
						bool hasTableFlg = false;
						cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Weapon'";
						using (var reader = cmd.ExecuteReader()) {
							if (reader.Read() && reader.GetInt32(0) == 1) {
								hasTableFlg = true;
							}
						}
						if (!hasTableFlg) {
							string sql = @"
							CREATE TABLE [Weapon](
							[id] INTEGER NOT NULL PRIMARY KEY,
							[name] TEXT NOT NULL DEFAULT '',
							[antiair] INTEGER NOT NULL DEFAULT 0,
							[intercept] INTEGER NOT NULL DEFAULT 0,
							[baurange] INTEGER NOT NULL DEFAULT 0,
							[type1] INTEGER NOT NULL DEFAULT 0,
							[type2] INTEGER NOT NULL DEFAULT 0,
							[type3] INTEGER NOT NULL DEFAULT 0,
							[type4] INTEGER NOT NULL DEFAULT 0,
							[type5] INTEGER NOT NULL DEFAULT 0,
							[weapon_flg] INTEGER NOT NULL)
						";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
							createTableFlg = true;
						}
					}
				}
			}
			// テーブルを作成したならば、データをダウンロードする
			if (createTableFlg || forceUpdateFlg) {
				if (await DownloadDataAsync()) {
					return DataStoreStatus.Success;
				} else {
					return DataStoreStatus.Failed;
				}
			} else {
				return DataStoreStatus.Exist;
			}
		}
		// データをダウンロードする
		public static async Task<bool> DownloadDataAsync() {
			await GetWeaponDicWikia();
			bool flg1 = await DownloadKammusuDataAsync();
			bool flg2 = await DownloadWeaponDataAsync();
			return flg1 && flg2;
		}
		// 艦娘のデータをidから正引きする
		// setWeaponFlgが有効だと、初期装備を握って登場する
		public static KammusuData KammusuDataById(int id, bool setWeaponFlg = false) {
			var kd = new KammusuData {
				Id = 0,
				Name = "なし",
				Level = 0,
				AntiAir = 0,
				SlotSize = 0,
				KammusuFlg = true,
				Weapon = new List<WeaponData>(),
				Slot = new List<int>(),
			};
			var weaponList = new List<int>();
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT name, type, antiair, slotsize, kammusu_flg, slot1, slot2, slot3, slot4, slot5, weapon1, weapon2, weapon3, weapon4, weapon5 FROM Kammusu WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							kd = new KammusuData {
								Id = id,
								Name = reader.GetString(0),
								Type = reader.GetString(1),
								Level = 1,
								AntiAir = reader.GetInt32(2),
								SlotSize = reader.GetInt32(3),
								KammusuFlg = (reader.GetInt32(4) == 1 ? true : false),
								Weapon = new List<WeaponData>(),
								Slot = new List<int>(),
							};
							for (int i = 0; i < 5; ++i) {
								int size = reader.GetInt32(5 + i);
								kd.Slot.Add(size);
								int wId = reader.GetInt32(10 + i);
								if (wId > 0) {
									weaponList.Add(wId);
								}
							}
						}
					}
				}
			}
			// setWeaponFlgが立っていた場合、装備データを復元する
			if (setWeaponFlg) {
				// N+1クエリの懸念もあるが、順序は重要なのでやむを得ずこうした
				foreach (int wId in weaponList) {
					var weapon = WeaponDataById(wId);
					kd.Weapon.Add(weapon);
				}
			}
			return kd;
		}
		// 装備のデータをidから正引きする
		public static WeaponData WeaponDataById(int id) {
			var wd = new WeaponData {
				Id = id,
				Name = "empty",
				Type = new List<int> { 0, 0, 0, 0, 0 },
				AntiAir = 0,
				Intercept = 0,
				Mas = 0,
				Rf = 0,
				BAURange = 0,
				WeaponFlg = true,
			};
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT name, antiair, intercept, baurange, weapon_flg, type1, type2, type3, type4, type5 FROM Weapon WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							wd = new WeaponData {
								Id = id,
								Name = reader.GetString(0),
								Type = new List<int> {
									reader.GetInt32(5),
									reader.GetInt32(6),
									reader.GetInt32(7),
									reader.GetInt32(8),
									reader.GetInt32(9)
								},
								AntiAir = reader.GetInt32(1),
								Intercept = reader.GetInt32(2),
								Mas = 0,
								Rf = 0,
								BAURange = reader.GetInt32(3),
								WeaponFlg = (reader.GetInt32(4) == 1 ? true : false)
							};
						}
					}
				}
			}
			wd.Refresh();
			return wd;
		}
		// 装備のデータをnameから逆引きする
		public static WeaponData WeaponDataByName(string name) {
			var wd = new WeaponData {
				Id = 0,
				Name = name,
				Type = new List<int> { 0, 0, 0, 0, 0 },
				AntiAir = 0,
				Intercept = 0,
				Mas = 0,
				Rf = 0,
				BAURange = 0,
				WeaponFlg = true,
			};
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT id, antiair, intercept, weapon_flg, baurange, type1, type2, type3, type4, type5 FROM Weapon WHERE name='{name}'";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							wd = new WeaponData {
								Id = reader.GetInt32(0),
								Name = name,
								Type = new List<int> {
									reader.GetInt32(5),
									reader.GetInt32(6),
									reader.GetInt32(7),
									reader.GetInt32(8),
									reader.GetInt32(9)
								},
								AntiAir = reader.GetInt32(1),
								Intercept = reader.GetInt32(2),
								Mas = 0,
								Rf = 0,
								BAURange = reader.GetInt32(4),
								WeaponFlg = (reader.GetInt32(3) == 1 ? true : false)
							};
						}
					}
				}
			}
			wd.Refresh();
			return wd;
		}
		// 艦名一覧を返す
		public static List<string> KammusuNameList() {
			var list = new List<string>();
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					cmd.CommandText = $"SELECT name FROM Kammusu WHERE kammusu_flg=1";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							list.Add(reader.GetString(0));
						}
					}
				}
			}
			return list;
		}
		// 深海棲艦の艦名一覧をid付きで返す
		public static Dictionary<int, KeyValuePair<string, string>> EnemyNameList() {
			var list = new Dictionary<int, KeyValuePair<string, string>>();
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					cmd.CommandText = $"SELECT id, name, type FROM Kammusu WHERE kammusu_flg=0 ORDER BY id";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							list[reader.GetInt32(0)] = new KeyValuePair<string, string>(reader.GetString(1), reader.GetString(2));
						}
					}
				}
			}
			return list;
		}
		// 装備名一覧を返す
		public static List<string> WeaponNameList() {
			var list = new List<string>();
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					cmd.CommandText = $"SELECT name FROM Weapon WHERE weapon_flg=1";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							list.Add(reader.GetString(0));
						}
					}
				}
			}
			return list;
		}
		// 基地航空隊に使用できる装備名一覧を返す
		// (表示用に最適化済)
		public static List<string> BasedAirUnitNameList() {
			var list = new List<string>();
			list.Add("なし");
			// 基地航空隊を飛ばせる条件：
			// ・type1=3 AND type2 not in (15, 16)
			// ・type1 in (17, 21, 22)
			// ・type1=5 AND type2 in (7, 36, 43)
			// ※type1→type2→type3で詳細度が上がるので、その順にソートした
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					cmd.CommandText = $"SELECT name,antiair,baurange FROM Weapon WHERE weapon_flg=1 AND ((type1=3 AND type2 NOT IN (15, 16)) OR type1 in (17, 21, 22) OR (type1=5 AND type2 in (7, 36, 43))) ORDER BY type1, type2, type3";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							list.Add($"{reader.GetString(0)}：{reader.GetInt32(1)}：{reader.GetInt32(2)}");
						}
					}
				}
			}
			return list;
		}
		// JSONデータを分析する
		public static FleetData ParseFleetData(string jsonString) {
			var obj = DynamicJson.Parse(jsonString);
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
							if (rawRf.GetType() == typeof(string)) {
								rf = int.Parse(rawRf);
							} else {
								rf = (int)(rawRf);
							}
							weaponData.Rf = rf;
						}
						kammusuData.Weapon.Add(weaponData);
					}
					kammusuList.Add(kammusuData);
				}
				fleetData.Kammusu.Add(kammusuList);
			}
			return fleetData;
		}
	}
	// 艦隊データ
	class FleetData
	{
		public List<List<KammusuData>> Kammusu { get; set; } = new List<List<KammusuData>>();
		// 文字列化するメソッド
		public override string ToString() {
			string[] masList = { "", "|", "||", "|||", "/", "//", "///", ">>" };
			string output = "";
			// 艦隊毎に
			for (int fi = 0; fi < Kammusu.Count; ++fi) {
				// 艦番毎に
				for (int si = 0; si < Kammusu[fi].Count; ++si) {
					var kammusu = Kammusu[fi][si];
					// 艦番号を出力
					if (fi == 0) {
						output += $"({si + 1})";
					} else {
						output += $"({fi + 1}-{si + 1})";
					}
					// 艦名と練度を出力
					output += $"{kammusu.Name} Lv{kammusu.Level}　";
					// 装備とスロット数情報を出力
					for (int ii = 0; ii < kammusu.Weapon.Count; ++ii) {
						var weapon = kammusu.Weapon[ii];
						if (ii != 0)
							output += ",";
						//搭載数
						output += $"[{kammusu.Slot[ii]}]";
						// 装備名
						output += $"{weapon.Name}";
						// 艦載機熟練度
						if (weapon.Mas != 0)
							output += $"{masList[weapon.Mas]}";
						// 装備改修度
						if (weapon.Rf != 0)
							output += $"★{weapon.Rf}";
					}
					output += "\n";
				}
			}
			return output;
		}
		// 搭載数を抽出するメソッド
		public List<List<List<int>>> GetSlotData() {
			var slotData = new List<List<List<int>>>();
			foreach (var kammusuList in Kammusu) {
				var list1 = new List<List<int>>();
				foreach (var kammusu in kammusuList) {
					var list2 = new List<int>();
					for (int si = 0; si < kammusu.SlotSize; ++si) {
						list2.Add(kammusu.Slot[si]);
					}
					list1.Add(list2);
				}
				slotData.Add(list1);
			}
			return slotData;
		}
		// 敵艦隊用に情報をJSONで書き出す
		public string GetJsonData() {
			if (Kammusu.Count == 0)
				return "[]";
			string output = "[";
			for (int i = 0; i < Kammusu.Count; ++i) {
				if (i != 0)
					output += ",";
				output += "{";
				output += $"\"kammusu\":[";
				for (int j = 0; j < Kammusu[i].Count; ++j) {
					if (j != 0)
						output += ",";
					output += $"{Kammusu[i][j].Id}";
				}
				output += "]}";
			}
			output += "]";
			return output;
		}
		// コンストラクタ
		public FleetData() {}
		public FleetData(string jsonString, bool setWeaponFlg = false) {
			// JSONをパース
			var obj = DynamicJson.Parse(jsonString);
			// パース結果を翻訳する
			foreach(var kammusuList in obj) {
				if (kammusuList.IsDefined("kammusu")) {
					var list = new List<KammusuData>();
					foreach (int kId in kammusuList.kammusu) {
						list.Add(DataStore.KammusuDataById(kId, setWeaponFlg));
					}
					Kammusu.Add(list);
				}
			}
		}
	}
	// 基地航空隊データ
	class BasedAirUnitData
	{
		public List<List<WeaponData>> Weapon { get; set; } = new List<List<WeaponData>>();
		public List<int> SallyCount { get; set; } = new List<int>();
		// 文字列化するメソッド
		public override string ToString() {
			string[] masList = { "", "|", "||", "|||", "/", "//", "///", ">>" };
			string output = "";
			// 航空隊毎に
			for (int ui = 0; ui < Weapon.Count; ++ui) {
				output += $"第{(ui + 1)}航空隊(出撃回数：{SallyCount[ui]})\n";
				// 中隊毎に
				for (int wi = 0; wi < Weapon[ui].Count; ++wi) {
					var weapon = Weapon[ui][wi];
					output += $"　{weapon.Name}";
					// 艦載機熟練度
					if (weapon.Mas != 0)
						output += $"{masList[weapon.Mas]}";
					// 装備改修度
					if (weapon.Rf != 0)
						output += $"★{weapon.Rf}";
					output += "\n";
				}
			}
			return output;
		}
		// 搭載数を抽出するメソッド
		public List<List<int>> GetSlotData() {
			var slotData = new List<List<int>>();
			foreach (var weaponList in Weapon) {
				var list = new List<int>();
				foreach (var weapon in weaponList) {
					// 搭載数は、偵察機が4機・それ以外は18機
					if ((weapon.Type[0] == 5 && weapon.Type[1] == 7)
						|| weapon.Type[0] == 17) {
						list.Add(4);
					} else {
						list.Add(18);
					}
				}
				slotData.Add(list);
			}
			return slotData;
		}
		// 情報をJsonで書き出す
		public string GetJsonData() {
			if (Weapon.Count == 0)
				return "[]";
			string output = "[";
			for(int ui = 0; ui < Weapon.Count; ++ui) {
				if (ui != 0)
					output += ",";
				output += "{";
				output += $"\"count\":{SallyCount[ui]},\"weapon\":[";
				for(int wi = 0; wi < Weapon[ui].Count; ++wi) {
					if (wi != 0)
						output += ",";
					output += "{";
					var w = Weapon[ui][wi];
					output += $"\"id\":{w.Id},\"mas\":{w.Mas},\"rf\":{w.Rf}";
					output += "}";
				}
				output += "]}";
			}
			output += "]";
			return output;
		}
		// コンストラクタ
		public BasedAirUnitData() { }
		public BasedAirUnitData(string jsonString) {
			// JSONをパース
			var obj = DynamicJson.Parse(jsonString);
			// パース結果を翻訳する
			foreach(var weaponList in obj) {
				SallyCount.Add((int)weaponList.count);
				var list = new List<WeaponData>();
				foreach(var weapon in weaponList.weapon) {
					WeaponData weaponData = DataStore.WeaponDataById((int)weapon.id);
					weaponData.Mas = (int)weapon.mas;
					weaponData.Rf = (int)weapon.rf;
					list.Add(weaponData);
				}
				Weapon.Add(list);
			}
		}
	}
	// 艦娘データ
	struct KammusuData
	{
		public int Id;
		public string Name;
		public string Type;
		public int Level;
		public int AntiAir;
		public int SlotSize;
		public bool KammusuFlg;
		public List<WeaponData> Weapon;
		public List<int> Slot;
	}
	// 装備データ
	struct WeaponData
	{
		public int Id;
		public string Name;
		public List<int> Type;  //装備種
		public int AntiAir;     //対空
		public int Intercept;   //迎撃
		public int Mas;         //艦載機熟練度
		public int Rf;          //装備改修度
		public int BAURange;    //戦闘行動半径
		public bool WeaponFlg;
		// 事前計算
		public void Refresh() {
			// 艦載機熟練度による制空ボーナス
			CalcAntiAirBonus();
			// 航空戦に参加するか？
			CalcHasAAV();
			// 最終対空値
			CalcCorrectedAA();
			// スロットサイズごとの制空値
			CalcAntiAirValue();
		}
		// 艦載機熟練度による制空ボーナス
		private void CalcAntiAirBonus() {
			// 艦戦・水戦制空ボーナス
			var pfwfBonus = new int[] { 0, 0, 2, 5, 9, 14, 14, 22, 22 };
			// 水爆制空ボーナス
			var wbBonus = new int[] { 0, 0, 1, 1, 1, 3, 3, 6, 6 };
			// 内部熟練ボーナス
			var masBonus = new int[] { 0, 10, 25, 40, 55, 70, 85, 100, 120 };
			// 装備種を判断し、そこから制空ボーナスを算出
			// 艦戦・水戦・陸戦・局戦は+25
			if ((Type[0] == 3 && Type[2] == 6) || Type[1] == 36 || (Type[0] == 22 && Type[2] == 48)) {
				AntiAirBonus = pfwfBonus[Mas] + Math.Sqrt(1.0 * masBonus[Mas] / 10);
				// 水爆は+9
			} else if (Type[1] == 43) {
				AntiAirBonus = wbBonus[Mas] + Math.Sqrt(1.0 * masBonus[Mas] / 10);
			} else {
				AntiAirBonus = Math.Sqrt(1.0 * masBonus[Mas] / 10);
			}
		}
		public double AntiAirBonus { get; private set; }
		// 航空戦に参加するか？(calcFlgがtrueなら、水上偵察機も参加するものとする)
		private bool[] hasAAV;
		private void CalcHasAAV() {
			hasAAV = new[]{ false, false };
			// 艦戦・艦攻・艦爆・爆戦・噴式(カ号・三式指揮連絡機を除く)
			if (Type[0] == 3 && Type[1] != 15 && Type[1] != 16) {
				hasAAV[0] = true;
				hasAAV[1] = true;
				return;
			}
			// 水戦・水爆
			if (Type[0] == 5 && (Type[1] == 36 || Type[1] == 43)) {
				hasAAV[0] = true;
				hasAAV[1] = true;
				return;
			}
			// 陸攻・局戦・陸戦
			if (Type[0] == 21 || Type[0] == 22) {
				hasAAV[0] = true;
				hasAAV[1] = true;
				return;
			}
			// 艦偵・水偵・飛行艇
			if ((Type[0] == 5 && Type[1] == 7) || Type[0] == 17) {
				hasAAV[0] = true;
				hasAAV[1] = true;
				return;
			}
		}
		public bool HasAAV(bool calcFlg) {
			return (calcFlg ? hasAAV[0] : hasAAV[1]);
		}
		// 各種影響を加味した最終対空値
		private void CalcCorrectedAA() {
			CorrectedAA = 1.0 * AntiAir + 1.5 * Intercept;
			//改修効果補正(艦戦・水戦・陸戦は★×0.2、爆戦は★×0.25だけ追加。局戦は★×0.2とした)
			if ((Type[0] == 3 && Type[2] == 6)
				|| (Type[0] == 5 && Type[1] == 36)
				|| (Type[0] == 22 && Type[2] == 48)) {
				CorrectedAA += 0.2 * Rf;
			} else if (Type[0] == 3 && Type[2] == 7 && Type[4] == 12) {
				CorrectedAA += 0.25 * Rf;
			}
		}
		public double CorrectedAA { get; private set; }
		// スロットサイズごとの制空値
		private List<int> antiAirValue1, antiAirValue2;
		private void CalcAntiAirValue() {
			antiAirValue1 = new List<int>();
			antiAirValue2 = new List<int>();
			for (int slot = 0; slot <= 180; ++slot) {
				int aav = (int)(Math.Floor(CorrectedAA * Math.Sqrt(slot) + AntiAirBonus));
				if (HasAAV(true)) {
					antiAirValue1.Add(aav);
				} else {
					antiAirValue1.Add(0);
				}
				if (HasAAV(false)) {
					antiAirValue2.Add(aav);
				} else {
					antiAirValue2.Add(0);
				}
			}
		}
		public int AntiAirValue(int slot, bool calcFlg) {
			return (calcFlg ? antiAirValue1[slot] : antiAirValue2[slot]);
		}
	}
	// データベースの状態(既にデータが存在する・ダウンロード成功・ダウンロード失敗)
	enum DataStoreStatus { Exist, Success, Failed }
}
