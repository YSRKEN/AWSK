using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net.Http;
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

		// 艦娘のデータをダウンロードする
		private static async Task<bool> DownloadKammusuDataAsync() {
			Console.WriteLine("艦娘のデータをダウンロード中……");
			try {
				// 艦娘データをダウンロード・パースする
				var commandList = new List<string>();
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
						int antiair = (int)(kammusu.max_aac);
						int slotsize = (int)(kammusu.slot);
						var slot = kammusu.carry;
						var firstWeapon = kammusu.equip;
						// 艦娘・深海棲艦のデータと呼べないものは無視する
						if (name == "なし")
							continue;
						if (id > 1900)
							continue;
						// 艦娘か？
						bool kammusuFlg = (id <= 1500);
						// コマンドを記録する
						string sql = "INSERT INTO Kammusu VALUES(";
						sql += $"{id},'{name}',{antiair},{slotsize},";
						{
							int count = 0;
							foreach (var size in slot) {
								sql += $"{size},";
								++count;
							}
							for (int wi = count; wi < 5; ++wi) {
								sql += "0,";
							}
						}
						{
							int count = 0;
							foreach (var wId in firstWeapon) {
								sql += $"{wId},";
								++count;
							}
							for (int wi = count; wi < 5; ++wi) {
								sql += "0,";
							}
						}
						sql += $"'{(kammusuFlg ? 1 : 0)}')";
						commandList.Add(sql);
					}
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
				return true;
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				return false;
			}
		}
		// 装備のデータをダウンロードする
		private static async Task<bool> DownloadWeaponDataAsync() {
			Console.WriteLine("装備のデータをダウンロード中……");
			try {
				// 装備データをダウンロード・パースする
				var commandList = new List<string>();
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
						var type = weapon.type;
						// 艦娘用の装備か？
						bool weaponFlg = (id <= 500);
						// コマンドを記録する
						string sql = "INSERT INTO Weapon VALUES(";
						sql += $"{id},'{name}',{antiair},";
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
				return true;
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
				return false;
			}
		}

		// データベースの初期化処理
		public static async Task<DataStoreStatus> Initialize() {
			// テーブルが存在しない場合、テーブルを作成し、ついでにデータをダウンロードする
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
						if (!hasTableFlg) {
							string sql = @"
								CREATE TABLE [Kammusu](
								[id] INTEGER NOT NULL PRIMARY KEY,
								[name] TEXT NOT NULL DEFAULT '',
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
			if (createTableFlg) {
				if(await DownloadDataAsync()) {
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
					cmd.CommandText = $"SELECT name, antiair, slotsize, kammusu_flg, slot1, slot2, slot3, slot4, slot5, weapon1, weapon2, weapon3, weapon4, weapon5 FROM Kammusu WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							kd = new KammusuData {
								Id = id,
								Name = reader.GetString(0),
								Level = 1,
								AntiAir = reader.GetInt32(1),
								SlotSize = reader.GetInt32(2),
								KammusuFlg = (reader.GetInt32(3) == 1 ? true : false),
								Weapon = new List<WeaponData>(),
								Slot = new List<int>(),
							};
							for(int i = 0; i < 5; ++i) {
								int size = reader.GetInt32(4 + i);
								kd.Slot.Add(size);
								int wId = reader.GetInt32(9 + i);
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
				foreach(int wId in weaponList) {
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
				Mas = 0,
				Rf = 0,
				WeaponFlg = true,
			};
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT name, antiair, weapon_flg, type1, type2, type3, type4, type5 FROM Weapon WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							wd = new WeaponData {
								Id = id,
								Name = reader.GetString(0),
								Type = new List<int> {
									reader.GetInt32(3),
									reader.GetInt32(4),
									reader.GetInt32(5),
									reader.GetInt32(6),
									reader.GetInt32(7)
								},
								AntiAir = reader.GetInt32(1),
								Mas = 0,
								Rf = 0,
								WeaponFlg = (reader.GetInt32(2) == 1 ? true : false)
							};
						}
					}
				}
			}
			return wd;
		}
		// 装備のデータをnameから逆引きする
		public static WeaponData WeaponDataByName(string name) {
			var wd = new WeaponData {
				Id = 0,
				Name = name,
				Type = new List<int> { 0, 0, 0, 0, 0 },
				AntiAir = 0,
				Mas = 0,
				Rf = 0,
				WeaponFlg = true,
			};
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT id, antiair, weapon_flg, type1, type2, type3, type4, type5 FROM Weapon WHERE name='{name}'";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							wd = new WeaponData {
								Id = reader.GetInt32(0),
								Name = name,
								Type = new List<int> {
									reader.GetInt32(3),
									reader.GetInt32(4),
									reader.GetInt32(5),
									reader.GetInt32(6),
									reader.GetInt32(7)
								},
								AntiAir = reader.GetInt32(1),
								Mas = 0,
								Rf = 0,
								WeaponFlg = (reader.GetInt32(2) == 1 ? true : false)
							};
						}
					}
				}
			}
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
		public static Dictionary<int, string> EnemyNameList() {
			var list = new Dictionary<int, string>();
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					cmd.CommandText = $"SELECT id, name FROM Kammusu WHERE kammusu_flg=0";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							list[reader.GetInt32(0)] = reader.GetString(1);
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
					cmd.CommandText = $"SELECT name FROM Weapon WHERE weapon_flg=1 AND ((type1=3 AND type2 NOT IN (15, 16)) OR type1 in (17, 21, 22) OR (type1=5 AND type2 in (7, 36, 43))) ORDER BY type1, type2, type3";
					using (var reader = cmd.ExecuteReader()) {
						while (reader.Read()) {
							list.Add(reader.GetString(0));
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
			for(int fi = 0; fi < Kammusu.Count; ++fi) {
				// 艦番毎に
				for (int si = 0; si < Kammusu[fi].Count; ++si) {
					var kammusu = Kammusu[fi][si];
					// 艦番号を出力
					if (fi == 0) {
						output += $"({si+1})";
					} else {
						output += $"({fi+1}-{si+1})";
					}
					// 艦名と練度を出力
					output += $"{kammusu.Name} Lv{kammusu.Level}　";
					// 装備情報を出力
					for (int ii = 0; ii < kammusu.Weapon.Count; ++ii) {
						var weapon = kammusu.Weapon[ii];
						if (ii != 0)
							output += ",";
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
			foreach(var kammusuList in Kammusu) {
				var list1 = new List<List<int>>();
				foreach (var kammusu in kammusuList) {
					var list2 = new List<int>();
					for(int si = 0; si < kammusu.SlotSize; ++si) {
						list2.Add(kammusu.Slot[si]);
					}
					list1.Add(list2);
				}
				slotData.Add(list1);
			}
			return slotData;
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
			for(int ui = 0; ui < Weapon.Count; ++ui) {
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
			foreach(var weaponList in Weapon) {
				var list = new List<int>();
				foreach (var weapon in weaponList) {
					// 搭載数は、偵察機が4機・それ以外は18機
					if((weapon.Type[0] == 5 && weapon.Type[1] == 7)
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
	}
	// 艦娘データ
	struct KammusuData
	{
		public int Id;
		public string Name;
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
		public List<int> Type;	//装備種
		public int AntiAir;		//対空
		public int Mas;			//艦載機熟練度
		public int Rf;			//装備改修度
		public bool WeaponFlg;
	}
	// データベースの状態(既にデータが存在する・ダウンロード成功・ダウンロード失敗)
	enum DataStoreStatus { Exist, Success, Failed }
}
