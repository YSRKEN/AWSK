using System.Collections.Generic;

namespace AWSK {
    static class Constant {
        /// <summary>
        /// staticコンストラクタ
        /// </summary>
        static Constant() {
            KammusuTypeReverseDic = new Dictionary<string, KammusuType>();
            foreach (var pair in KammusuTypeDic) {
                KammusuTypeReverseDic[pair.Value] = pair.Key;
            }
        }

        /// <summary>
        /// 艦種を表すenum
        /// </summary>
        public enum KammusuType {
            Other, DD, CD, PT, CL, CLT, CP, CA, CAV, CV, ACV, CVL, AV, BB,
            CC, BBV, SS, SSV, LST, AO, LHA, AR, AS, AF
        };

        /// <summary>
        /// 艦種enumを艦種文字列に変換する
        /// </summary>
        public static Dictionary<KammusuType, string> KammusuTypeDic =
            new Dictionary<KammusuType, string>{
                { KammusuType.Other, "その他" },
                { KammusuType.DD, "駆逐艦" },
                { KammusuType.CD, "海防艦" },
                { KammusuType.PT, "魚雷艇" },
                { KammusuType.CL, "軽巡洋艦" },
                { KammusuType.CLT, "重雷装巡洋艦" },
                { KammusuType.CP, "練習巡洋艦" },
                { KammusuType.CA, "重巡洋艦" },
                { KammusuType.CAV, "航空巡洋艦" },
                { KammusuType.CV, "正規空母" },
                { KammusuType.ACV, "装甲空母" },
                { KammusuType.CVL, "軽空母" },
                { KammusuType.AV, "水上機母艦" },
                { KammusuType.BB, "戦艦" },
                { KammusuType.CC, "巡洋戦艦" },
                { KammusuType.BBV, "航空戦艦" },
                { KammusuType.SS, "潜水艦" },
                { KammusuType.SSV, "潜水空母" },
                { KammusuType.LST, "輸送艦" },
                { KammusuType.AO, "補給艦" },
                { KammusuType.LHA, "揚陸艦" },
                { KammusuType.AR, "工作艦" },
                { KammusuType.AS, "潜水母艦" },
                { KammusuType.AF, "陸上型" },
        };

        /// <summary>
        /// 艦種文字列を艦種enumに変換する
        /// </summary>
        public static Dictionary<string, KammusuType> KammusuTypeReverseDic = null;

        /// <summary>
        /// 装備種を表すenum
        /// </summary>
        public enum WeaponType {
            Other, PF, PB, PA, JPB, PS, WF, WB, WS, LFB, LB, LA, LF
        };

        /// <summary>
        /// 装備種enumを装備文字列に変換する
        /// </summary>
        public static Dictionary<WeaponType, string> WeaponTypeDic =
            new Dictionary<WeaponType, string>{
                { WeaponType.Other, "その他" },
                { WeaponType.PF, "艦上戦闘機" },
                { WeaponType.PB, "艦上爆撃機" },
                { WeaponType.PA, "艦上攻撃機" },
                { WeaponType.JPB, "墳式爆撃機" },
                { WeaponType.PS, "艦上偵察機" },
                { WeaponType.WF, "水上戦闘機" },
                { WeaponType.WB, "水上爆撃機" },
                { WeaponType.WS, "水上偵察機" },
                { WeaponType.LFB, "大型飛行艇" },
                { WeaponType.LB, "陸上爆撃機" },
                { WeaponType.LA, "陸上攻撃機" },
                { WeaponType.LF, "陸上戦闘機" },
        };

        /// <summary>
        /// 最大のスロット数
        /// </summary>
        public static int MAX_SLOT_COUNT = 5;

        /// <summary>
        /// 深海棲艦における「航空戦艦」の中で陸上型なリスト
        /// </summary>
        public static HashSet<string> AFSet = new HashSet<string> {
            "飛行場姫", "港湾棲姫", "離島棲鬼", "北方棲姫", "中間棲姫",
            "港湾水鬼", "泊地水鬼", "港湾棲姫", "集積地棲姫", "集積地棲姫-壊",
            "離島棲姫", "港湾夏姫", "港湾夏姫-壊", "北端上陸姫", "集積地夏姫",
        };

        public static Dictionary<string, KammusuType> KammusuTypeReverseDicWikia
            = new Dictionary<string, KammusuType>{
                {"AP", KammusuType.AO},
                {"AV", KammusuType.AV},
                {"BB", KammusuType.BB},
                {"BBV", KammusuType.BBV},
                {"CA", KammusuType.CA},
                {"CAV", KammusuType.CAV},
                {"CL", KammusuType.CL},
                {"CLT", KammusuType.CLT},
                {"CV", KammusuType.CV},
                {"CVL", KammusuType.CVL},
                {"DD", KammusuType.DD},
                {"FBB", KammusuType.CC},
                {"SS", KammusuType.SS},
        };
    }
}
