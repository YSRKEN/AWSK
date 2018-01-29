using System.Data.SQLite;

namespace AWSK.Stores
{
	static class DataStore
	{
		private const string connectionString = @"Data Source=GameData.db";
		public static void Initialize() {
			// テーブルが存在しない場合、テーブルを作成する
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
							[id] INTEGER NOT NULL UNIQUE,
							[name] TEXT NOT NULL DEFAULT '',
							PRIMARY KEY(id))
						";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
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
							[id] INTEGER NOT NULL UNIQUE,
							[name] TEXT NOT NULL DEFAULT '',
							PRIMARY KEY(id))
						";
							cmd.CommandText = sql;
							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}
		public static bool DownloadData() {
			return true;
		}
	}
}
