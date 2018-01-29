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
		public static KammusuData? KammusuDataById(int id) {
			KammusuData? kd = null;
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
								KammusuFlg = (reader.GetInt32(1) == 1 ? true : false)
							};
						}
					}
				}
			}
			return kd;
		}
		// 装備のデータをidから正引きする
		public static WeaponData? WeaponDataById(int id) {
			WeaponData? kd = null;
			using (var con = new SQLiteConnection(connectionString)) {
				con.Open();
				using (var cmd = con.CreateCommand()) {
					// 艦娘データを正引き
					cmd.CommandText = $"SELECT name, weapon_flg FROM Weapon WHERE id={id}";
					using (var reader = cmd.ExecuteReader()) {
						if (reader.Read()) {
							kd = new WeaponData {
								Id = id,
								Name = reader.GetString(0),
								WeaponFlg = (reader.GetInt32(1) == 1 ? true : false)
							};
						}
					}
				}
			}
			return kd;
		}
	}
	struct KammusuData
	{
		public int Id;
		public string Name;
		public bool KammusuFlg;
	}
	struct WeaponData
	{
		public int Id;
		public string Name;
		public bool WeaponFlg;
	}
}
