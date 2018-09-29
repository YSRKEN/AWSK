using AWSK.Models;
using AWSK.Service;
using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.Stores {
    static class DataStore {
        /// <summary>
        /// Kammusu型をKammusuData型に変換する
        /// </summary>
        /// <param name="kammusu">Kammusu型</param>
        /// <returns>KammusuData型</returns>
        private static KammusuData Convert(Kammusu kammusu) {
            // nullならば、デフォルト値を返す
            if (kammusu == null) {
                var kd = new KammusuData {
                    Id = 0,
                    Name = "なし",
                    Type = "その他",
                    Level = 0,
                    AntiAir = 0,
                    SlotSize = 0,
                    KammusuFlg = true,
                    Weapon = new List<Weapon>(),
                    Slot = new List<int>(),
                };
            }

            // Type部分を変換
            string type = KammusuTypeDic.ContainsKey(kammusu.Type) ? KammusuTypeDic[kammusu.Type] : "";

            // 変換後の結果を算出して返す
            var kammusuData = new KammusuData {
                Id = kammusu.Id,
                Name = kammusu.Name,
                Type = type,
                Level = 1,
                AntiAir = kammusu.AntiAir,
                SlotSize = kammusu.SlotList.Count,
                KammusuFlg = kammusu.KammusuFlg,
                Weapon = new List<Weapon>(),
                Slot = kammusu.SlotList,
            };
            foreach(var weapon in kammusu.WeaponList){
                kammusuData.Weapon.Add(weapon);
            }
            return kammusuData;
        }

        /// <summary>
        /// データベースの初期化処理
        /// </summary>
        /// <param name="forceUpdateFlg">強制的に更新する場合はtrue</param>
        /// <returns></returns>
        public static async Task<DataStoreStatus> Initialize(bool forceUpdateFlg = false) {
            // テーブルが存在しない場合、テーブルを作成する
            var database = DataBaseService.instance;
            bool createTableFlg = database.CreateWeaponTable(forceUpdateFlg)
                || database.CreateKammusuTable(forceUpdateFlg);

            // テーブルを作成したならば、データをダウンロードする
            if (!createTableFlg) {
                return DataStoreStatus.Exist;
            }
            try {
                var downloader = DownloadService.instance;
                var weaponData = await downloader.downloadWeaponDataFromWikiaAsync();
                database.SaveAll(weaponData, true);
                var kammusuData = await downloader.downloadKammusuDataFromWikiaAsync();
                database.SaveAll(kammusuData, true);
                var kammusuData2 = downloader.downloadKammusuDataFromLocalFile();
                database.SaveAll(kammusuData2, true);
                return DataStoreStatus.Success;
            } catch (Exception e) {
                Console.WriteLine(e);
                return DataStoreStatus.Failed;
            }
        }

        /// <summary>
        /// 艦娘のデータをidから正引きする
        /// </summary>
        /// <param name="id">艦船ID</param>
        /// <param name="setWeaponFlg">trueの場合、初期装備を握って登場する</param>
        /// <returns>艦船情報</returns>
        public static KammusuData KammusuDataById(int id, bool setWeaponFlg = false) {
            var database = DataBaseService.instance;
            return Convert(database.FindByKammusuId(id, setWeaponFlg));
        }

        /// <summary>
        /// 深海棲艦の艦名一覧をid付きで返す
        /// </summary>
        /// <returns>艦船ID, [艦名・艦種]</returns>
        public static Dictionary<int, KeyValuePair<string, string>> EnemyNameList() {
            // 深海棲艦の艦名・艦種一覧を取得する
            var database = DataBaseService.instance;
            var rawKammusuList = database.FindByKammusuFlg(false, false);
            var kammusuList = new Dictionary<int, KeyValuePair<string, string>>();
            foreach(var kammusu in rawKammusuList) {
                var kammusuData = Convert(kammusu);
                kammusuList[kammusuData.Id] = new KeyValuePair<string, string>(kammusuData.Name, kammusuData.Type);
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
                var kammusu = Convert(database.FindByKammusuId(id, false));
                kammusuList[id + 10000] = new KeyValuePair<string, string>($"*{name}", kammusu.Type);
            }
            return kammusuList;
        }

        /// <summary>
        /// 基地航空隊に使用できる装備名一覧を返す
        /// </summary>
        /// <returns>基地航空隊に使用できる装備名一覧</returns>
        public static List<string> BasedAirUnitNameList() {
            // 艦娘サイドの装備一覧を抽出
            var database = DataBaseService.instance;
            var rawWeaponList = database.FindByForKammusuFlg(true);

            // 基地航空隊に使用できるものだけ抽出
            var temp = rawWeaponList
                .Where(w => BAUWeaponTypeSet.Contains(w.Type))
                .OrderBy(w => w.Id)
                .OrderBy(w => (int)w.Type)
                .ToList();

            // 一覧を生成する
            var list = new List<string>();
            list.Add("なし");
            for(int i = 0; i < temp.Count; ++i) {
                if (i == 0 || temp[i].Type != temp[i - 1].Type) {
                    list.Add($"【{WeaponTypeDic[temp[i].Type]}】");
                }
                list.Add($"{temp[i].Name}：{temp[i].AntiAir}：{temp[i].BasedAirUnitRange}");
            }
            return list;
        }

        /// <summary>
        /// JSONデータを分析する
        /// </summary>
        /// <param name="jsonString">JSONによる艦隊データ</param>
        /// <returns>艦隊データを表すクラス</returns>
        public static FleetData ParseFleetData(string jsonString) {
            var obj = DynamicJson.Parse(jsonString);
            var fleetData = new FleetData();
            var database = DataBaseService.instance;
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
                    var kammusuData = KammusuDataById(k_id);
                    kammusuData.Level = lv;
                    for (int ii = 1; ii <= 5; ++ii) {
                        string key = (ii == 5 ? "ix" : $"i{ii}");
                        // 定義されていない装備は飛ばす
                        if (!obj[$"f{fi}"][$"s{si}"].items.IsDefined(key))
                            continue;
                        // idを読み取り、そこから装備名を出力
                        int w_id = (int)(obj[$"f{fi}"][$"s{si}"].items[key].id);
                        var weaponData = database.FindByWeaponId(w_id);
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
    class FleetData {
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
        public FleetData() { }
        public FleetData(string jsonString, bool setWeaponFlg = false) {
            // JSONをパース
            var obj = DynamicJson.Parse(jsonString);
            // パース結果を翻訳する
            foreach (var kammusuList in obj) {
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
    class BasedAirUnitData {
        public List<List<Weapon>> Weapon { get; set; } = new List<List<Weapon>>();
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
                    if (weapon.Type == WeaponType.PS
                        || weapon.Type == WeaponType.WS
                        || weapon.Type == WeaponType.LFB) {
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
            for (int ui = 0; ui < Weapon.Count; ++ui) {
                if (ui != 0)
                    output += ",";
                output += "{";
                output += $"\"count\":{SallyCount[ui]},\"weapon\":[";
                for (int wi = 0; wi < Weapon[ui].Count; ++wi) {
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
            var database = DataBaseService.instance;
            foreach (var weaponList in obj) {
                SallyCount.Add((int)weaponList.count);
                var list = new List<Weapon>();
                foreach (var weapon in weaponList.weapon) {
                    var weaponData = database.FindByWeaponId((int)weapon.id);
                    weaponData.Mas = (int)weapon.mas;
                    weaponData.Rf = (int)weapon.rf;
                    list.Add(weaponData);
                }
                Weapon.Add(list);
            }
        }
    }

    // 艦娘データ
    struct KammusuData {
        public int Id;
        public string Name;
        public string Type;
        public int Level;
        public int AntiAir;
        public int SlotSize;
        public bool KammusuFlg;
        public List<Weapon> Weapon;
        public List<int> Slot;
    }
}
