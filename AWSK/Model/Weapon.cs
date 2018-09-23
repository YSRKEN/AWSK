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
        public string Name { get; }

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
    }
}
