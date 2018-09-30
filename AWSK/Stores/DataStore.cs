using AWSK.Models;
using AWSK.Service;
using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.Stores {
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
                    if (weapon.IsSearcher) {
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
}
