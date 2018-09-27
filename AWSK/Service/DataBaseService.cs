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
                            for (int i = 0; i < reader.FieldCount; ++i) {
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
        /// 艦娘テーブルを作成する
        /// </summary>
        public void CreateKammusuTable(bool forceFlg) {
            string sql = @"
				CREATE TABLE [kammusu](
				[id] INTEGER NOT NULL PRIMARY KEY,
				[name] TEXT NOT NULL DEFAULT '',
				[type] INTEGER NOT NULL DEFAULT 0,
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
				[kammusu_flg] INTEGER NOT NULL DEFAULT 1)
			";
            CreateTable("kammusu", sql, forceFlg);
        }

        /// <summary>
        /// 装備情報を挿入・上書きする
        /// </summary>
        /// <param name="weapon">装備情報</param>
        /// <param name="forceFlg">既存データがある場合上書きしないならfalse</param>
        public void Save(Weapon weapon, bool forceFlg) {
            if (!forceFlg) {
                var temp = ExecuteReader($"SELECT count(id) AS count FROM weapon WHERE id={weapon.Id}");
                if (temp.Count > 0 && temp[0]["count"] >= 1) {
                    return;
                }
            }

            // クエリを作成する
            string query = string.Format("REPLACE INTO weapon VALUES ({0},'{1}',{2},{3},{4},{5},{6});",
                weapon.Id, weapon.Name, (int)weapon.Type, weapon.AntiAir, weapon.Intercept,
                weapon.BasedAirUnitRange, weapon.ForKammusuFlg ? 1 : 0);

            // クエリを実行する
            ExecuteNonQuery(query);
            return;
        }

        /// <summary>
        /// 装備情報を一括で挿入・上書きする
        /// </summary>
        /// <param name="weaponList">装備リスト</param>
        /// <param name="forceFlg">既存データがある場合上書きしないならfalse</param>
        public void SaveAll(List<Weapon> weaponList, bool forceFlg) {
            // 上書きしないオプションを反映する
            var weaponList2 = weaponList;
            if (!forceFlg) {
                string idList = string.Join(",", weaponList.Select(w => w.Id.ToString()));
                var temp = ExecuteReader($"SELECT id FROM weapon WHERE id IN ({idList})");
                var temp2 = temp.Select(r => (int)r["id"]).ToHashSet();
                weaponList2 = new List<Weapon>();
                foreach(var weapon in weaponList) {
                    if (!temp2.Contains(weapon.Id)) {
                        weaponList2.Add(weapon);
                    }
                }
            }

            // トランザクションを実行する
            using (var con = new SQLiteConnection(CONNECTION_STRING)) {
                con.Open();
                using (var trans = con.BeginTransaction()) {
                    try {
                        foreach (var weapon in weaponList2) {
                            // クエリを作成する
                            string query = string.Format("REPLACE INTO weapon VALUES ({0},'{1}',{2},{3},{4},{5},{6});",
                                weapon.Id, weapon.Name, (int)weapon.Type, weapon.AntiAir, weapon.Intercept,
                                weapon.BasedAirUnitRange, weapon.ForKammusuFlg ? 1 : 0);

                            // クエリを実行する
                            using (var cmd = con.CreateCommand()) {
                                cmd.CommandText = query;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    } catch (Exception e) {
                        trans.Rollback();
                    }
                }
            }
            return;
        }

        /// <summary>
        /// 艦娘情報を挿入・上書きする
        /// </summary>
        /// <param name="kammusu">艦娘情報</param>
        /// <param name="defaultWeaponIdList">初期装備</param>
        /// <param name="forceFlg">既存データがある場合上書きしないならfalse</param>
        public void Save(Kammusu kammusu, List<int> defaultWeaponIdList, bool forceFlg) {
            if (!forceFlg) {
                var temp = ExecuteReader($"SELECT count(id) AS count FROM kammusu WHERE id={kammusu.Id}");
                if (temp.Count > 0 && temp[0]["count"] >= 1) {
                    return;
                }
            }

            // クエリを作成する
            string query = string.Format("REPLACE INTO kammusu VALUES ({0},'{1}',{2},{3},{4}",
                kammusu.Id, kammusu.Name, (int)kammusu.Type, kammusu.AntiAir, kammusu.SlotList.Count);
            for (int i = 0; i < kammusu.SlotList.Count; ++i) {
                query += $",{kammusu.SlotList[i]}";
            }
            for (int i = kammusu.SlotList.Count; i < MAX_SLOT_COUNT; ++i) {
                query += ",0";
            }
            for (int i = 0; i < kammusu.SlotList.Count; ++i) {
                query += $",{defaultWeaponIdList[i]}";
            }
            for (int i = kammusu.SlotList.Count; i < MAX_SLOT_COUNT; ++i) {
                query += ",0";
            }
            query += $",{(kammusu.KammusuFlg ? 1 : 0)});";

            // クエリを実行する
            ExecuteNonQuery(query);
            return;
        }

        /// <summary>
        /// 艦娘情報を一括で挿入・上書きする
        /// </summary>
        /// <param name="kammusuList">艦娘と初期装備のペアのリスト</param>
        /// <param name="forceFlg">既存データがある場合上書きしないならfalse</param>
        public void SaveAll(List<KeyValuePair<Kammusu, List<int>>> kammusuList, bool forceFlg) {
            // 上書きしないオプションを反映する
            var kammusuList2 = kammusuList;
            if (!forceFlg) {
                string idList = string.Join(",", kammusuList.Select(w => w.Key.Id.ToString()));
                var temp = ExecuteReader($"SELECT id FROM kammusu WHERE id IN ({idList})");
                var temp2 = temp.Select(r => (int)r["id"]).ToHashSet();
                kammusuList2 = new List<KeyValuePair<Kammusu, List<int>>>();
                foreach (var pair in kammusuList) {
                    if (!temp2.Contains(pair.Key.Id)) {
                        kammusuList2.Add(pair);
                    }
                }
            }

            // トランザクションを実行する
            using (var con = new SQLiteConnection(CONNECTION_STRING)) {
                con.Open();
                using (var trans = con.BeginTransaction()) {
                    try {
                        foreach (var pair in kammusuList2) {
                            Console.WriteLine(pair.Key.Name);
                            // クエリを作成する
                            string query = string.Format("REPLACE INTO kammusu VALUES ({0},'{1}',{2},{3},{4}",
                                pair.Key.Id, pair.Key.Name, (int)pair.Key.Type, pair.Key.AntiAir, pair.Key.SlotList.Count);
                            for (int i = 0; i < pair.Key.SlotList.Count; ++i) {
                                query += $",{pair.Key.SlotList[i]}";
                            }
                            for (int i = pair.Key.SlotList.Count; i < MAX_SLOT_COUNT; ++i) {
                                query += ",0";
                            }
                            for (int i = 0; i < pair.Value.Count; ++i) {
                                query += $",{pair.Value[i]}";
                            }
                            for (int i = pair.Value.Count; i < MAX_SLOT_COUNT; ++i) {
                                query += ",0";
                            }
                            query += $",{(pair.Key.KammusuFlg ? 1 : 0)});";

                            // クエリを実行する
                            using (var cmd = con.CreateCommand()) {
                                cmd.CommandText = query;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        trans.Rollback();
                    }
                }
            }
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
        /// <param name="setDefaultWeaponFlg">初期装備を持たせる場合はtrue</param>
        /// <returns>艦娘情報。未ヒットの場合はnull</returns>
        public Kammusu findByKammusuId(int id, bool setDefaultWeaponFlg) {
            // SELECT文を実行する
            var queryResult = ExecuteReader($"SELECT * FROM kammusu WHERE id={id}");
            if (queryResult.Count == 0) {
                return null;
            }

            // SELECT文から結果を取得する
            var queryResult2 = queryResult[0];
            int slotSize = (int)queryResult2["slotsize"];
            var slotList = new List<int>();
            var weaponList = new List<int>();
            for (int i = 0; i < slotSize; ++i) {
                slotList.Add((int)queryResult2[$"slot{i + 1}"]);
                weaponList.Add((int)queryResult2[$"weapon{i + 1}"]);
            }

            var result = new Kammusu(
                (int)queryResult2["id"],
                (string)queryResult2["name"],
                (KammusuType)queryResult2["type"],
                (int)queryResult2["antiair"],
                slotList,
                queryResult2["kammusu_flg"] == 1);

            // 初期装備を持たせる
            if (setDefaultWeaponFlg) {
                for (int i = 0; i < slotSize; ++i) {
                    result.WeaponList[i] = findByWeaponId(weaponList[i]);
                }
            }
            return result;
        }
    }
}
