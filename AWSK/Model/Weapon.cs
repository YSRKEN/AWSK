using System.Collections.Generic;
using static AWSK.Constant;

namespace AWSK.Models {
    /// <summary>
    /// 装備を表現するクラス
    /// </summary>
    class Weapon {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Weapon() {
            Id = 0;
            Name = "なし";
            Type = WeaponType.Other;
            AntiAir = 0;
            Intercept = 0;
            BasedAirUnitRange = 0;
            ForKammusuFlg = true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">装備ID</param>
        /// <param name="name">装備名</param>
        /// <param name="type">装備種</param>
        /// <param name="antiAir">対空値</param>
        /// <param name="intercept">迎撃値</param>
        /// <param name="basedAirUnitRange">戦闘行動半径</param>
        /// <param name="forKammusuFlg">艦娘用装備か？</param>
        public Weapon(int id, string name, WeaponType type, int antiAir,
            int intercept, int basedAirUnitRange, bool forKammusuFlg) {
            Id = id;
            Name = name;
            Type = type;
            AntiAir = antiAir;
            Intercept = intercept;
            BasedAirUnitRange = basedAirUnitRange;
            ForKammusuFlg = forKammusuFlg;
        }

        /// <summary>
        /// 装備ID(図鑑番号と一致)
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 装備名
        /// </summary>
        private string name;
        public string Name {
            get => name;
            private set {
                this.name = value;

                // 爆戦系かの判定を連動して設定している
                this.IsBombFighter = value.Contains("爆戦");
            }
        }

        /// <summary>
        /// 装備種
        /// </summary>
        public WeaponType Type { get; }

        /// <summary>
        /// 対空値
        /// </summary>
        public int AntiAir { get; }

        /// <summary>
        /// 迎撃値
        /// </summary>
        public int Intercept { get; }

        /// <summary>
        /// 戦闘行動半径
        /// </summary>
        public int BasedAirUnitRange { get; }

        /// <summary>
        /// 艦娘用装備か？
        /// </summary>
        public bool ForKammusuFlg { get; }

        /// <summary>
        /// 艦載機熟練度
        /// </summary>
        public int Mas { get; set; }

        /// <summary>
        /// 装備改修度
        /// </summary>
        public int Rf { get; set; }

        /// <summary>
        /// 改修補正を適用した対空値
        /// </summary>
        public double CorrectedAntiAir {
            get {
                double correctedAA = 1.0 * AntiAir + 1.5 * Intercept;
                if (IsFighter) {
                    // 艦戦・水戦・陸戦(便宜上局戦もこちらに含めた)
                    correctedAA += 0.2 * Rf;
                } else if (IsBombFighter) {
                    // 爆戦
                    correctedAA += 0.25 * Rf;
                }
                return correctedAA;
            }
        }

        /// <summary>
        /// 戦闘機系ならtrue
        /// </summary>
        public bool IsFighter {
            get => Type == WeaponType.PF || Type == WeaponType.WF || Type == WeaponType.LF;
        }

        /// <summary>
        /// 偵察機系ならtrue
        /// </summary>
        public bool IsSearcher {
            get => Type == WeaponType.PS || Type == WeaponType.WS || Type == WeaponType.LFB;
        }

        /// <summary>
        /// 航空戦に参加するならtrue
        /// </summary>
        /// <param name="wsFlg">水上偵察機が関わる場合はtrue</param>
        /// <returns></returns>
        public bool HasAAV(bool wsFlg) {
            if (AAVWeaponTypeSet.Contains(Type)){
                return true;
            }
            if (wsFlg && Type != WeaponType.WS) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 爆戦系ならtrue
        /// </summary>
        public bool IsBombFighter { get; private set; }
    }
}
