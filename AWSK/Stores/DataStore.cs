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
						// 艦娘・深海棲艦のデータと呼べないものは無視する
						if (name == "なし")
							continue;
						if (id > 1900)
							continue;
						// 艦娘か？
						bool kammusuFlg = (id <= 1500);
						// コマンドを記録する
						commandList.Add($"INSERT INTO Kammusu VALUES({id},'{name}','{(kammusuFlg ? 1 : 0)}')");
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
						// 艦娘用の装備か？
						bool weaponFlg = (id <= 500);
						// コマンドを記録する
						commandList.Add($"INSERT INTO Weapon VALUES({id},'{name}','{(weaponFlg ? 1 : 0)}')");
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
		public static async void Initialize() {
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
				if (await DownloadDataAsync()) {
					MessageBox.Show("ダウンロードに成功しました。", "AWSK");
				} else {
					MessageBox.Show("ダウンロードに失敗しました。", "AWSK");
				}
			}
		}
		// データをダウンロードする
		public static async Task<bool> DownloadDataAsync() {
			bool flg1 = await DownloadKammusuDataAsync();
			bool flg2 = await DownloadWeaponDataAsync();
			return flg1 && flg2;
		}
		// 艦娘のデータをidから正引きする
		public static KammusuData KammusuDataById(int id) {
			var kd = new KammusuData {
				Id = 0,
				Name = "なし",
				Level = 0,
				KammusuFlg = true,
				Weapon = new List<WeaponData>()
			};
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT name, kammusu_flg FROM Kammusu WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							kd = new KammusuData {
								Id = id,
								Name = reader.GetString(0),
								Level = 1,
								KammusuFlg = (reader.GetInt32(1) == 1 ? true : false),
								Weapon = new List<WeaponData>()
							};
						}
					}
				}
			}
			return kd;
		}
		// 装備のデータをidから正引きする
		public static WeaponData WeaponDataById(int id) {
			var wd = new WeaponData {
				Id = id,
				Name = "empty",
				Mas = 0,
				Rf = 0,
				WeaponFlg = true
			};
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT name, weapon_flg FROM Weapon WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							wd = new WeaponData {
								Id = id,
								Name = reader.GetString(0),
								Mas = 0,
								Rf = 0,
								WeaponFlg = (reader.GetInt32(1) == 1 ? true : false)
							};
						}
					}
				}
			}
			return wd;
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
	}
	// 艦娘データ
	struct KammusuData
	{
		public int Id;
		public string Name;
		public int Level;
		public bool KammusuFlg;
		public List<WeaponData> Weapon;
	}
	// 装備データ
	struct WeaponData
	{
		public int Id;
		public string Name;
		public int Mas;	// 艦載機熟練度
		public int Rf;	// 装備改修度
		public bool WeaponFlg;
	}
}
