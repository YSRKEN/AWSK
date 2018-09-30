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
        public static readonly Dictionary<KammusuType, string> KammusuTypeDic =
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
        /// 艦種文字列を艦種enumに変換する(Wikia用)
        /// </summary>
        public static readonly Dictionary<string, KammusuType> KammusuTypeReverseDicWikia
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

        /// <summary>
        /// 深海棲艦における「航空戦艦」の中で陸上型なリスト
        /// </summary>
        public static readonly HashSet<string> AFSet = new HashSet<string> {
            "飛行場姫", "港湾棲姫", "離島棲鬼", "北方棲姫", "中間棲姫",
            "港湾水鬼", "泊地水鬼", "港湾棲姫", "集積地棲姫", "集積地棲姫-壊",
            "離島棲姫", "港湾夏姫", "港湾夏姫-壊", "北端上陸姫", "集積地夏姫",
        };

        /// <summary>
        /// 装備種を表すenum
        /// </summary>
        public enum WeaponType {
            Other, PF, PB, PA, JPB, PS, WF, WB, WS, LFB, LB, LA, LF
        };

        /// <summary>
        /// 装備種enumを装備文字列に変換する
        /// </summary>
        public static readonly Dictionary<WeaponType, string> WeaponTypeDic =
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
        /// 装備種文字列を装備種enumに変換する(Wikia用)
        /// </summary>
        public static readonly Dictionary<string, WeaponType> WeaponTypeReverseDicWikia
            = new Dictionary<string, WeaponType>{
                {"Carrier-based Fighter Aircraft", WeaponType.PF},
                {"Carrier-based Dive Bomber", WeaponType.PB},
                {"Carrier-based Torpedo Bomber", WeaponType.PA},
                {"Jet-powered Fighter-Bomber", WeaponType.JPB},
                {"Carrier-based Reconnaissance Aircraft", WeaponType.PS},
                {"Seaplane Fighter", WeaponType.WF},
                {"Seaplane Bomber", WeaponType.WB},
                {"Reconnaissance Seaplane", WeaponType.WS},
                {"Large Flying Boat", WeaponType.LFB},
                {"Land-based Attack Aircraft", WeaponType.LA},
                {"Land-based Fighter", WeaponType.LF},
        };

        /// <summary>
        /// 基地航空隊に使用できる装備種一覧
        /// </summary>
        public static readonly HashSet<WeaponType> BAUWeaponTypeSet
            = new HashSet<WeaponType> {
                WeaponType.PF, WeaponType.PB, WeaponType.PA, WeaponType.JPB,
                WeaponType.PS, WeaponType.WF, WeaponType.WB, WeaponType.WS,
                WeaponType.LFB, WeaponType.LB, WeaponType.LA, WeaponType.LF
        };

        /// <summary>
        /// 航空戦に参加する装備種一覧
        /// </summary>
        public static readonly HashSet<WeaponType> AAVWeaponTypeSet
            = new HashSet<WeaponType> {
                WeaponType.PF, WeaponType.PB, WeaponType.PA, WeaponType.JPB,
                WeaponType.WF, WeaponType.WB,
                WeaponType.LB, WeaponType.LA, WeaponType.LF
        };

        /// <summary>
        /// 最大のスロット数
        /// </summary>
        public static readonly int MAX_SLOT_COUNT = 5;

        /// <summary>
        /// 艦戦・水戦制空ボーナス
        /// </summary>
        public static readonly int[] PfWfBonus = new int[] { 0, 0, 2, 5, 9, 14, 14, 22, 22 };

        /// <summary>
        /// 水爆制空ボーナス
        /// </summary>
        public static readonly int[] WbBonus = new int[] { 0, 0, 1, 1, 1, 3, 3, 6, 6 };

        /// <summary>
        /// 内部熟練ボーナス
        /// </summary>
        public static readonly int[] MasBonus = new int[] { 0, 10, 25, 40, 55, 70, 85, 100, 120 };

        public static readonly string[] MasStringList = { "", "|", "||", "|||", "/", "//", "///", ">>" };

        /// <summary>
        /// データベースの状態(既にデータが存在する・ダウンロード成功・ダウンロード失敗)
        /// </summary>
        public enum DataStoreStatus { Exist, Success, Failed }

        /// <summary>
        /// 制空状況(制空権確保～制空権喪失、種類数)
        /// </summary>
        public enum AirWarStatus { Best, Good, Even, Bad, Worst, Size }

        /// <summary>
        /// 制空状況enumを制空状況文字列に変換する
        /// </summary>
        public static readonly Dictionary<AirWarStatus, string> AirWarStatusDic
            = new Dictionary<AirWarStatus, string>{
                { AirWarStatus.Best, "確保" },
                { AirWarStatus.Good, "優勢" },
                { AirWarStatus.Even, "均衡" },
                { AirWarStatus.Bad, "劣勢" },
                { AirWarStatus.Worst, "喪失" },
            };

        /// <summary>
        /// ゲーム内で登場する最大のスロットサイズ
        /// </summary>
        public static readonly int MaxSlotSize = 300;
    }
}
;