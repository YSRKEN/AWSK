using AWSK.Service;
using Codeplex.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSK.Model {
    /// <summary>
    /// 基地航空隊全体を表現するクラス
    /// </summary>
    class BasedAirUnitGroup {
        /// <summary>
        /// 各基地航空隊のリスト
        /// </summary>
        public List<BasedAirUnit> BasedAirUnitList { get; } = new List<BasedAirUnit>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitGroup() {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="jsonString">JSON文字列</param>
        public BasedAirUnitGroup(string jsonString) {
            // JSONをパース
            var obj = DynamicJson.Parse(jsonString);

            // パース結果を翻訳する
            var database = DataBaseService.instance;
            foreach (var weaponList in obj) {
                var basedAirUnit = new BasedAirUnit();
                basedAirUnit.SallyCount = (int)weaponList.count;
                foreach (var weapon in weaponList.weapon) {
                    var weaponData = database.FindByWeaponId((int)weapon.id);
                    weaponData.Mas = (int)weapon.mas;
                    weaponData.Rf = (int)weapon.rf;
                    basedAirUnit.WeaponList.Add(weaponData);
                }
                BasedAirUnitList.Add(basedAirUnit);
            }
        }

        /// <summary>
        /// 基地航空隊全体の各搭載数の一覧
        /// </summary>
        public List<List<int>> SlotList {
            get {
                return BasedAirUnitList.Select(bauList =>
                    bauList.WeaponList.Select(w =>
                        // 搭載数は、偵察機が4機・それ以外は18機
                        w.IsSearcher ? 4 : 18
                    ).ToList()
                ).ToList();
            }
        }

        /// <summary>
        /// 文字列化するメソッド
        /// </summary>
        /// <returns>基地航空隊全体を表す文字列</returns>
        public override string ToString() {
            var output = new StringBuilder();
            // 航空隊毎に
            for (int ui = 0; ui < BasedAirUnitList.Count; ++ui) {
                var baseAirUnit = BasedAirUnitList[ui];
                output.AppendFormat("第{0}航空隊(出撃回数：{1})\n", ui + 1, baseAirUnit.SallyCount);
                // 中隊毎に
                for (int wi = 0; wi < baseAirUnit.WeaponList.Count; ++wi) {
                    // 装備名
                    var weapon = baseAirUnit.WeaponList[wi];
                    output.AppendFormat("　{0}", weapon.Name);

                    // 艦載機熟練度
                    if (weapon.Mas != 0)
                        output.Append(Constant.MasStringList[weapon.Mas]);

                    // 装備改修度
                    if (weapon.Rf != 0)
                        output.AppendFormat("★{0}", weapon.Rf);
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
            // データが存在しない場合
            if (BasedAirUnitList.Count == 0)
                return "[]";

            // データが存在する場合
            var output = new StringBuilder("[");
            for (int ui = 0; ui < BasedAirUnitList.Count; ++ui) {
                var basedAirUnit = BasedAirUnitList[ui];
                if (ui != 0)
                    output.Append(",");
                output.Append("{");
                output.AppendFormat("\"count\":{0},\"weapon\":[", basedAirUnit.SallyCount);
                for (int wi = 0; wi < basedAirUnit.WeaponList.Count; ++wi) {
                    var weapon = basedAirUnit.WeaponList[wi];
                    if (wi != 0)
                        output.Append(",");
                    output.Append("{");
                    output.AppendFormat("\"id\":{0},\"mas\":{1},\"rf\":{2}", weapon.Id, weapon.Mas, weapon.Rf);
                    output.Append("}");
                }
                output.Append("]}");
            }
            output.Append("]");
            return output.ToString();
        }
    }
}
