using AWSK.Models;
using AWSK.Service;
using Codeplex.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSK.Model {
    /// <summary>
    /// 艦隊を表現するクラス
    /// </summary>
    class Fleet {
        /// <summary>
        /// 艦隊に含まれている艦娘一覧
        /// </summary>
        public List<List<Kammusu>> KammusuList { get; } = new List<List<Kammusu>>();

        /// <summary>
        /// 艦隊の艦娘の各搭載数の一覧
        /// </summary>
        public List<List<List<int>>> SlotList {
            get {
                return KammusuList.Select(kList =>
                    kList.Select(k =>
                        k.SlotList.Select(s => 
                            s
                        ).ToList()
                    ).ToList()
                ).ToList();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Fleet() {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="jsonString">JSON文字列</param>
        public Fleet(string jsonString) {
            // JSONをパース
            var obj = DynamicJson.Parse(jsonString);

            // パース結果を翻訳する
            var database = DataBaseService.Instance;
            foreach (var kammusuList in obj) {
                if (!kammusuList.IsDefined("kammusu")) {
                    continue;
                }
                var list = new List<Kammusu>();
                foreach (int id in kammusuList.kammusu) {
                    list.Add(database.FindByKammusuId(id, false));
                }
                KammusuList.Add(list);
            }
        }

        /// <summary>
        /// 文字列化するメソッド
        /// </summary>
        /// <returns>艦隊を表す文字列</returns>
        public override string ToString() {
            var output = new StringBuilder();
            // 艦隊毎に
            for (int fi = 0; fi < KammusuList.Count; ++fi) {
                // 艦番毎に
                for (int si = 0; si < KammusuList[fi].Count; ++si) {
                    var kammusu = KammusuList[fi][si];
                    // 艦番号を出力
                    if (fi == 0) {
                        output.AppendFormat("({0})", si + 1);
                    } else {
                        output.AppendFormat("({0}-{1})", fi + 1, si + 1);
                    }
                    
                    // 艦名と練度を出力
                    output.AppendFormat("{0} Lv{1}　", kammusu.Name, kammusu.Level);

                    // 装備とスロット数情報を出力
                    for (int ii = 0; ii < kammusu.WeaponList.Count; ++ii) {
                        var weapon = kammusu.WeaponList[ii];
                        if (weapon == null) {
                            continue;
                        }
                        if (ii != 0)
                            output.Append(",");

                        //搭載数
                        output.AppendFormat("[{0}]", kammusu.SlotList[ii]);
                        
                        // 装備名
                        output.Append(weapon.Name);
                        
                        // 艦載機熟練度
                        if (weapon.Mas != 0)
                            output.Append(Constant.MasStringList[weapon.Mas]);
                        
                        // 装備改修度
                        if (weapon.Rf != 0)
                            output.AppendFormat("★{0}", weapon.Rf);
                    }
                    output.Append("\n");
                }
            }
            return output.ToString();
        }

        /// <summary>
        /// JSON化するメソッド
        /// </summary>
        /// <returns>艦隊のJSONデータ</returns>
        public string ToJson() {
            // 艦隊が登録されていない場合は
            if (KammusuList.Count == 0)
                return "[]";

            // 順に読み取って記録していく
            var output = new StringBuilder("[");
            for (int i = 0; i < KammusuList.Count; ++i) {
                if (i != 0)
                    output.Append(",");
                output.Append("{");
                output.Append("\"kammusu\":[");
                for (int j = 0; j < KammusuList[i].Count; ++j) {
                    if (j != 0)
                        output.Append(",");
                    output.AppendFormat("%d", KammusuList[i][j].Id);
                }
                output.Append("]}");
            }
            output.Append("]");
            return output.ToString();
        }
    }
}
