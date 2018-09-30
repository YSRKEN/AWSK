using AWSK.Models;
using System.Collections.Generic;
using System.Text;

namespace AWSK.Model {
    /// <summary>
    /// 基地航空隊を表現するクラス
    /// </summary>
    class BasedAirUnit {
        /// <summary>
        /// スロット毎の搭載数
        /// </summary>
        public List<int> SlotList { get; } = new List<int>();

        /// <summary>
        /// スロット毎の装備リスト
        /// (スロットに存在しない場合はnullが入る)
        /// </summary>
        public List<Weapon> WeaponList { get; } = new List<Weapon>();

        /// <summary>
        /// 出撃回数
        /// </summary>
        public int SallyCount { get; set; }
    }
}
