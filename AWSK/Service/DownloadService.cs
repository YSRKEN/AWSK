using AWSK.Models;
using Codeplex.Data;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.Service {
    /// <summary>
    /// データをダウンロードするクラス
    /// </summary>
    class DownloadService {
        /// <summary>
        /// 自分自身の唯一のinstance
        /// </summary>
        static DownloadService singleton = null;

        /// <summary>
        /// privateコンストラクタ
        /// </summary>
        DownloadService() { }

        /// <summary>
        /// 唯一のinstanceを返す(Singletonパターン)
        /// </summary>
        /// <returns></returns>
        public static DownloadService instance {
            get {
                if (singleton == null) {
                    singleton = new DownloadService();
                }
                return singleton;
            }
        }

        /// <summary>
        /// 装備リストをデッキビルダーからダウンロードする
        /// </summary>
        /// <returns>装備リスト</returns>
        public async Task<List<Weapon>> downloadWeaponDataFromDeckBuilderAsync() {
            var result = new List<Weapon>();
            using (var client = new HttpClient()) {
                // テキストデータダウンロード
                string rawData = await client.GetStringAsync("http://kancolle-calc.net/data/itemdata.js");

                // 余計な文字を削除
                rawData = rawData.Replace("var gItems = ", "");

                // JSONとしてパース
                var obj = DynamicJson.Parse(rawData);

                // パース結果を取得
                foreach (var weapon in obj) {
                    // ID・装備名・対空値を取得
                    int id = (int)weapon.id;
                    string name = (string)weapon.name;
                    int antiAir = (int)weapon.aac;

                    // 装備種は力技で処理する
                    var rawType = weapon.type;
                    WeaponType type;
                    if (rawType[0] == 3 && rawType[1] == 5 && rawType[2] == 6) {
                        type = WeaponType.PF;
                    } else if (rawType[0] == 3 && rawType[1] == 5 && rawType[2] == 7) {
                        type = WeaponType.PB;
                    } else if (rawType[0] == 3 && rawType[1] == 5 && rawType[2] == 8) {
                        type = WeaponType.PA;
                    } else if (rawType[0] == 3 && rawType[1] == 40 && rawType[2] == 57) {
                        type = WeaponType.JPB;
                    } else if (rawType[0] == 5 && rawType[1] == 7 && rawType[2] == 9) {
                        type = WeaponType.PS;
                    } else if (rawType[0] == 5 && rawType[1] == 7 && rawType[2] == 10) {
                        type = WeaponType.WS;
                    } else if (rawType[0] == 5 && rawType[1] == 36 && rawType[2] == 45) {
                        type = WeaponType.WF;
                    } else if (rawType[0] == 5 && rawType[1] == 43 && rawType[2] == 11) {
                        type = WeaponType.WB;
                    } else if (rawType[0] == 17 && rawType[1] == 33 && rawType[2] == 41) {
                        type = WeaponType.LFB;
                    } else if (rawType[0] == 21 && rawType[1] == 28 && rawType[2] == 47) {
                        type = WeaponType.LA;
                    } else if (rawType[0] == 22 && rawType[1] == 39 && rawType[2] == 47) {
                        type = WeaponType.LB;
                    } else if (rawType[0] == 22 && rawType[1] == 39 && rawType[2] == 48) {
                        type = WeaponType.LF;
                    } else {
                        type = WeaponType.Other;
                    }

                    // 局戦・陸戦は迎撃値を読み取る
                    int intercept = 0;
                    if (type == WeaponType.LF) {
                        intercept = (int)weapon.evasion;
                    }

                    /// 戦闘行動半径の処理
                    /// (情報内に含まれていないので取得しようがない。なので0固定)
                    int basedAirUnitRange = 0;

                    // 艦娘用の装備か？
                    bool forKammusuFlg = id <= 500;

                    // 追記する
                    result.Add(new Weapon(id, name, WeaponType.Other, antiAir, intercept, basedAirUnitRange, forKammusuFlg));
                }
            }
            return result;
        }

        /// <summary>
        /// 艦娘リストをデッキビルダーからダウンロードする
        /// </summary>
        /// <returns>艦娘</returns>
        public async Task<List<KeyValuePair<Kammusu, List<int>>>> downloadKammusuDataFromDeckBuilderAsync() {
            var result = new List<KeyValuePair<Kammusu, List<int>>>();
            using (var client = new HttpClient()) {
                // テキストデータダウンロード
                string rawData = await client.GetStringAsync("http://kancolle-calc.net/data/shipdata.js");

                // 余計な文字を削除
                rawData = rawData.Replace("var gShips = ", "");

                // JSONとしてパース
                var obj = DynamicJson.Parse(rawData);

                // パース結果を取得
                foreach (var kammusu in obj) {
                    // 艦船IDや艦名などを取得
                    int id = int.Parse(kammusu.id);
                    if (id > 1900)
                        continue;
                    string name = (string)kammusu.name;
                    if (name == "なし")
                        continue;
                    int antiAir = (int)kammusu.max_aac;
                    int slotCount = (int)kammusu.slot;
                    var slot = kammusu.carry;
                    var defaultWeapon = kammusu.equip;

                    // 艦種データを変換する
                    string rawType = (string)kammusu.type;
                    var type = KammusuTypeReverseDic.ContainsKey(rawType) ? KammusuTypeReverseDic[rawType] : KammusuType.Other;

                    // 艦娘か？
                    bool kammusuFlg = id <= 1500;

                    // 追記する
                    var kammusuData = new Kammusu(id, name, type, antiAir, new List<int>(), kammusuFlg);
                    var defaultWeaponData = new List<int>();
                    foreach (var s in slot) {
                        kammusuData.SlotList.Add((int)s);
                    }
                    foreach (var w in defaultWeapon) {
                        defaultWeaponData.Add((int)w);
                    }
                    int temp = defaultWeaponData.Count;
                    for (int i = temp; i < slotCount; ++i) {
                        defaultWeaponData.Add(0);
                    }
                    result.Add(new KeyValuePair<Kammusu, List<int>>(kammusuData, defaultWeaponData));
                }
            }
            return result;
        }
    }
}
