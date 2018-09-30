using AWSK.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
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
        private static DataBaseService singleton = null;

        /// <summary>
        /// 接続用文字列
        /// </summary>
        private const string CONNECTION_STRING = @"Data Source=GameData2.db";

        /// <summary>
        /// privateコンストラクタ
        /// </summary>
        private DataBaseService() { }

        /// <summary>
        /// 返り値が不要なクエリを実行する
        /// </summary>
        /// <param name="query">クエリ</param>
        private void ExecuteNonQuery(string query) {
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
        private List<Dictionary<string, dynamic>> ExecuteReader(string query) {
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
        /// <returns>テーブルを作成したならtrue</returns>
        private bool CreateTable(string tableName, string query, bool forceFlg) {
            // 既存のテーブルが存在するかを調べる
            var result = ExecuteReader($"SELECT count(*) AS count FROM sqlite_master WHERE type='table' AND name='{tableName}'");
            if (result.Count > 0 && result[0]["count"] >= 1) {
                if (forceFlg) {
                    ExecuteNonQuery($"DROP TABLE {tableName}");
                } else {
                    return false;
                }
            }

            // CREATE TABLE処理を実行する
            ExecuteNonQuery(query);
            return true;
        }

        /// <summary>
        /// 装備テーブルを作成する
        /// </summary>
        /// <param name="forceFlg">強制的に作成するならtrue</param>
        /// <returns>テーブルを作成したならtrue</returns>
        private bool CreateWeaponTable(bool forceFlg) {
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
            return CreateTable("weapon", sql, forceFlg);
        }

        /// <summary>
        /// 艦娘テーブルを作成する
        /// </summary>
        /// <param name="forceFlg">強制的に作成するならtrue</param>
        /// <returns>テーブルを作成したならtrue</returns>
        private bool CreateKammusuTable(bool forceFlg) {
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
            return CreateTable("kammusu", sql, forceFlg);
        }

        /// <summary>
        /// 装備情報を挿入・上書きする
        /// </summary>
        /// <param name="weapon">装備情報</param>
        /// <param name="forceFlg">既存データがある場合上書きしないならfalse</param>
        private void Save(Weapon weapon, bool forceFlg) {
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
        private void SaveAll(List<Weapon> weaponList, bool forceFlg) {
            // 上書きしないオプションを反映する
            var weaponList2 = weaponList;
            if (!forceFlg) {
                string idList = string.Join(",", weaponList.Select(w => w.Id.ToString()));
                var temp = ExecuteReader($"SELECT id FROM weapon WHERE id IN ({idList})");
                var temp2 = temp.Select(r => (int)r["id"]).ToHashSet();
                weaponList2 = new List<Weapon>();
                foreach (var weapon in weaponList) {
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
                        Console.WriteLine(e);
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
        private void Save(Kammusu kammusu, List<int> defaultWeaponIdList, bool forceFlg) {
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
        private void SaveAll(List<KeyValuePair<Kammusu, List<int>>> kammusuList, bool forceFlg) {
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
        /// データベースの初期化処理
        /// </summary>
        /// <param name="forceUpdateFlg">強制的に更新する場合はtrue</param>
        /// <returns></returns>
        public async Task<DataStoreStatus> Initialize(bool forceUpdateFlg = false) {
            // テーブルが存在しない場合、テーブルを作成する
            bool createTableFlg = CreateWeaponTable(forceUpdateFlg)
                || CreateKammusuTable(forceUpdateFlg);

            // テーブルを作成したならば、データをダウンロードする
            if (!createTableFlg) {
                return DataStoreStatus.Exist;
            }
            try {
                var downloader = DownloadService.instance;
                var weaponData = await downloader.downloadWeaponDataFromWikiaAsync();
                SaveAll(weaponData, true);
                var kammusuData = await downloader.downloadKammusuDataFromWikiaAsync();
                SaveAll(kammusuData, true);
                var kammusuData2 = downloader.downloadKammusuDataFromLocalFile();
                SaveAll(kammusuData2, true);
                return DataStoreStatus.Success;
            } catch (Exception e) {
                Console.WriteLine(e);
                return DataStoreStatus.Failed;
            }
        }

        /// <summary>
        /// 装備情報を装備IDから検索して返す
        /// </summary>
        /// <param name="id">装備ID</param>
        /// <returns>装備情報。未ヒットの場合はnull</returns>
        public Weapon FindByWeaponId(int id) {
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
        public Kammusu FindByKammusuId(int id, bool setDefaultWeaponFlg) {
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
                    result.WeaponList[i] = FindByWeaponId(weaponList[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// 装備情報を装備名から検索して返す
        /// </summary>
        /// <param name="name">装備名</param>
        /// <returns>装備情報。未ヒットの場合はnull</returns>
        public Weapon FindByWeaponName(string name) {
            // SELECT文を実行する
            var queryResult = ExecuteReader($"SELECT * FROM weapon WHERE name='{name}'");
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
        /// 艦娘情報を艦娘フラグで絞り込んで返す
        /// </summary>
        /// <param name="kammusuFlg">艦娘フラグ</param>
        /// <param name="setDefaultWeaponFlg">初期装備を持たせる場合はtrue</param>
        /// <returns>艦娘リスト。未ヒットの場合は空配列</returns>
        public List<Kammusu> FindByKammusuFlg(bool kammusuFlg, bool setDefaultWeaponFlg) {
            // SELECT文を実行する
            var queryResult = ExecuteReader($"SELECT * FROM kammusu WHERE kammusu_flg={(kammusuFlg ? 1 : 0)}");
            if (queryResult.Count == 0) {
                return new List<Kammusu>();
            }

            // SELECT文から結果を取得する
            var result = new List<Kammusu>();
            foreach (var qr in queryResult) {
                int slotSize = (int)qr["slotsize"];
                var slotList = new List<int>();
                var weaponList = new List<int>();
                for (int i = 0; i < slotSize; ++i) {
                    slotList.Add((int)qr[$"slot{i + 1}"]);
                    weaponList.Add((int)qr[$"weapon{i + 1}"]);
                }

                var temp = new Kammusu(
                    (int)qr["id"],
                    (string)qr["name"],
                    (KammusuType)qr["type"],
                    (int)qr["antiair"],
                    slotList,
                    qr["kammusu_flg"] == 1);

                // 初期装備を持たせる
                if (setDefaultWeaponFlg) {
                    for (int i = 0; i < slotSize; ++i) {
                        temp.WeaponList[i] = FindByWeaponId(weaponList[i]);
                    }
                }

                result.Add(temp);
            }
            return result;
        }

        /// <summary>
        /// 装備情報を装備フラグから絞り込んで返す
        /// </summary>
        /// <param name="forKammusuFlg">艦娘用装備か？</param>
        /// <returns>装備リスト。未ヒットの場合は空配列</returns>
        public List<Weapon> FindByForKammusuFlg(bool forKammusuFlg) {
            // SELECT文を実行する
            var queryResult = ExecuteReader($"SELECT * FROM weapon WHERE for_kammusu_flg={(forKammusuFlg ? 1 : 0)}");
            if (queryResult.Count == 0) {
                return new List<Weapon>();
            }

            // SELECT文から結果を取得して返す
            var result = new List<Weapon>();
            foreach (var qr in queryResult) {
                var temp = new Weapon(
                    (int)qr["id"],
                    (string)qr["name"],
                    (WeaponType)qr["type"],
                    (int)qr["antiair"],
                    (int)qr["intercept"],
                    (int)qr["based_air_unit_range"],
                    qr["for_kammusu_flg"] == 1);
                result.Add(temp);
            }
            return result;
        }

        /// <summary>
        /// 深海棲艦の艦名一覧をid付きで返す
        /// </summary>
        /// <returns>艦船ID, [艦名・艦種]</returns>
        public Dictionary<int, KeyValuePair<string, string>> EnemyNameList() {
            // 深海棲艦の艦名・艦種一覧を取得する
            var rawKammusuList = FindByKammusuFlg(false, false);
            var kammusuList = new Dictionary<int, KeyValuePair<string, string>>();
            foreach (var kammusu in rawKammusuList) {
                kammusuList[kammusu.Id] = new KeyValuePair<string, string>(kammusu.Name, KammusuTypeDic[kammusu.Type]);
            }

            // 俗称一覧を取得する
            var enemyFamiliarName = new Dictionary<string, int>();
            if (System.IO.File.Exists(@"Resource\EnemyFamiliarName.csv")) {
                using (var sr = new System.IO.StreamReader(@"Resource\EnemyFamiliarName.csv")) {
                    while (!sr.EndOfStream) {
                        try {
                            // 1行読み込み、カンマ毎に区切る
                            string line = sr.ReadLine();
                            string[] values = line.Split(',');
                            // 行数がおかしい場合は飛ばす
                            if (values.Count() < 2)
                                continue;
                            // ヘッダー行は飛ばす
                            if (values[0] == "俗称")
                                continue;
                            // データを読み取る
                            string name = values[0];
                            int id = int.Parse(values[1]);
                            enemyFamiliarName[name] = id;
                        } catch (Exception) { }
                    }
                }
            }

            // 俗称部分は、idの数字を10000だけ大きくして登録する
            foreach (var pair in enemyFamiliarName) {
                string name = pair.Key;
                int id = pair.Value;
                var kammusu = FindByKammusuId(id, false);
                kammusuList[id + 10000] = new KeyValuePair<string, string>($"*{name}", KammusuTypeDic[kammusu.Type]);
            }
            return kammusuList;
        }

        /// <summary>
        /// 基地航空隊に使用できる装備名一覧を返す
        /// </summary>
        /// <returns>基地航空隊に使用できる装備名一覧</returns>
        public List<string> BasedAirUnitNameList() {
            // 艦娘サイドの装備一覧を抽出
            var rawWeaponList = FindByForKammusuFlg(true);

            // 基地航空隊に使用できるものだけ抽出
            var temp = rawWeaponList
                .Where(w => BAUWeaponTypeSet.Contains(w.Type))
                .OrderBy(w => w.Id)
                .OrderBy(w => (int)w.Type)
                .ToList();

            // 一覧を生成する
            var list = new List<string>();
            list.Add("なし");
            for (int i = 0; i < temp.Count; ++i) {
                if (i == 0 || temp[i].Type != temp[i - 1].Type) {
                    list.Add($"【{WeaponTypeDic[temp[i].Type]}】");
                }
                list.Add($"{temp[i].Name}：{temp[i].AntiAir}：{temp[i].BasedAirUnitRange}");
            }
            return list;
        }
    }
}
