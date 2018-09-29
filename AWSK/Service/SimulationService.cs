using AWSK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Service {
    /// <summary>
    /// シミュレーションを行うサービスクラス
    /// </summary>
    class SimulationService {
        /// <summary>
        /// 自分自身の唯一のinstance
        /// </summary>
        private static SimulationService singleton = null;

        /// <summary>
        /// privateコンストラクタ
        /// </summary>
        private SimulationService() { }

        /// <summary>
        /// 唯一のinstanceを返す(Singletonパターン)
        /// </summary>
        /// <returns></returns>
        public static SimulationService instance {
            get {
                if (singleton == null) {
                    singleton = new SimulationService();
                }
                return singleton;
            }
        }

        /// <summary>
        /// 装備1つにおける制空値を計算する
        /// </summary>
        /// <param name="weapon">装備</param>
        /// <param name="slotSize">搭載数</param>
        /// <param name="wsFlg">水偵を制空値計算に含める場合はtrue</param>
        /// <returns></returns>
        public int CalcAntiAirValue(Weapon weapon, int slotSize, bool wsFlg) {
            // 航空戦に参加しない装備の場合はスルー
            if (!Constant.AAVWeaponTypeSet.Contains(weapon.Type)) {
                if (!wsFlg || weapon.Type != Constant.WeaponType.WS) {
                    return 0;
                }
            }

            // 熟練度補正を計算
            double antiAirBonus = Math.Sqrt(1.0 * Constant.MasBonus[weapon.Mas] / 10);
            if (weapon.Type == Constant.WeaponType.PF
                || weapon.Type == Constant.WeaponType.WF
                || weapon.Type == Constant.WeaponType.LF) {
                // 艦戦・水戦・陸戦・局戦は+25
                antiAirBonus += Constant.PfWfBonus[weapon.Mas];
            } else if (weapon.Type == Constant.WeaponType.WB) {
                // 水爆は+9
                antiAirBonus += Constant.WbBonus[weapon.Mas];
            }

            // 最終的な制空値を算出
            return (int)(weapon.CorrectedAntiAir * Math.Sqrt(slotSize) + antiAirBonus);
        }
    }
}
