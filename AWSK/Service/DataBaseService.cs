using AWSK.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.Service {
    /// <summary>
    /// データベースを操作するクラス
    /// </summary>
    class DataBaseService {
        /// <summary>
        /// 自分自身の唯一のinstance
        /// </summary>
        static DataBaseService singleton = null;

        /// <summary>
        /// 接続用文字列
        /// </summary>
        const string CONNECTION_STRING = @"Data Source=GameData2.db";

        /// <summary>
        /// privateコンストラクタ
        /// </summary>
        DataBaseService() { }

        /// <summary>
        /// 返り値が不要なクエリを実行する
        /// </summary>
        /// <param name="query">クエリ</param>
        void ExecuteNonQuery(string query) {
            using (var con = new SQLiteConnection(CONNECTION_STRING)) {
                con.Open();
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 返り値が存在するクエリを実行する
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<Dictionary<string, dynamic>> ExecuteReader(string query) {
            var result = new List<Dictionary<string, dynamic>>();
            using (var con = new SQLiteConnection(CONNECTION_STRING)) {
                con.Open();
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = query;
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            var record = new Dictionary<string, dynamic>();
                            for(int i = 0; i < reader.FieldCount; ++i) {
                                switch (reader.GetFieldType(i).FullName) {
                                case "System.Int64":
                                    record[reader.GetName(i)] = reader.GetFieldValue<long>(i);
                                    break;
                                case "System.String":
                                    record[reader.GetName(i)] = reader.GetFieldValue<string>(i);
                                    break;
                                default:
                                    record[reader.GetName(i)] = reader.GetValue(i);
                                    break;
                                }
                            }
                            result.Add(record);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// テーブルを作成する
        /// </summary>
        /// <param name="tableName">テーブル名</param>
        /// <param name="query">テーブルを作成するクエリ</param>
        /// <param name="forceFlg">既存のテーブルが存在していた場合でも作成する場合はtrue</param>
        void CreateTable(string tableName, string query, bool forceFlg) {
            // 既存のテーブルが存在するかを調べる
            var result = ExecuteReader($"SELECT count(*) AS count FROM sqlite_master WHERE type='table' AND name='{tableName}'");
            if (result.Count > 0 && result[0]["count"] >= 1) {
                if (forceFlg) {
                    ExecuteNonQuery($"DROP TABLE {tableName}");
                } else {
                    return;
                }
            }

            // CREATE TABLE処理を実行する
            ExecuteNonQuery(query);
        }

        /// <summary>
        /// 唯一のinstanceを返す(Singletonパターン)
        /// </summary>
        /// <returns></returns>
        public static DataBaseService instance {
            get {
                if (singleton == null) {
                    singleton = new DataBaseService();
                }
                return singleton;
            }
        }

        /// <summary>
        /// 装備テーブルを作成する
        /// </summary>
        public void CreateWeaponTable(bool forceFlg) {
            string sql = @"
				CREATE TABLE [weapon](
				[id] INTEGER NOT NULL PRIMARY KEY,
				[name] TEXT NOT NULL DEFAULT '',
				[type] INTEGER NOT NULL DEFAULT 0,
                [antiair] INTEGER NOT NULL DEFAULT 0,
				[intercept] INTEGER NOT NULL DEFAULT 0,
				[based_air_unit_range] INTEGER NOT NULL DEFAULT 0,
				[for_kammusu_flg] INTEGER NOT NULL DEFAULT 1)
			";
            CreateTable("weapon", sql, forceFlg);
        }

        /// <summary>
        /// 装備情報を挿入・上書きする
        /// </summary>
        /// <param name="weapon">装備情報</param>
        public void Save(Weapon weapon) {
            // クエリを作成する
            string query = string.Format("REPLACE INTO Weapon VALUES ({0},'{1}',{2},{3},{4},{5},{6});",
                weapon.Id, weapon.Name, (int)weapon.Type, weapon.AntiAir, weapon.Intercept,
                weapon.BasedAirUnitRange, weapon.ForKammusuFlg ? 1 : 0);

            // クエリを実行する
            ExecuteNonQuery(query);
            return;
        }

        /// <summary>
        /// 艦娘情報を挿入・上書きする
        /// </summary>
        /// <param name="kammusu">艦娘情報</param>
        public void Save(Kammusu kammusu) {
            // スタブ
            return;
        }

        /// <summary>
        /// 装備情報を装備IDから検索して返す
        /// </summary>
        /// <param name="id">装備ID</param>
        /// <returns>装備情報。未ヒットの場合はnull</returns>
        public Weapon findByWeaponId(int id) {
            // SELECT文を実行する
            var queryResult = ExecuteReader($"SELECT * FROM weapon WHERE id={id}");
            if (queryResult.Count == 0) {
                return null;
            }

            // SELECT文から結果を取得して返す
            var queryResult2 = queryResult[0];
            var result = new Weapon(
                (int)queryResult2["id"],
                (string)queryResult2["name"],
                (WeaponType)queryResult2["type"],
                (int)queryResult2["antiair"],
                (int)queryResult2["intercept"],
                (int)queryResult2["based_air_unit_range"],
                queryResult2["for_kammusu_flg"] == 1);
            return result;
        }

        /// <summary>
        /// 艦娘情報を艦船IDから検索して返す
        /// </summary>
        /// <param name="id">艦船ID</param>
        /// <returns>艦娘情報。未ヒットの場合はnull</returns>
        public Kammusu findByKammusuId(int id) {
            // スタブ
            return null;
        }
    }
}
