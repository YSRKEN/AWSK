using System.Collections.Generic;
using static AWSK.Constant;

namespace AWSK.Models {
    /// <summary>
    /// 艦娘を表現するクラス
    /// </summary>
    class Kammusu {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Kammusu() {
            Id = 0;
            Name = "なし";
            Level = 1;
            Type = KammusuType.Other;
            AntiAir = 0;
            SlotList = new List<int>();
            WeaponList = new List<Weapon>();
            KammusuFlg = true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">艦船ID</param>
        /// <param name="name">艦船名</param>
        /// <param name="type">艦種</param>
        /// <param name="antiAir">素対空値</param>
        /// <param name="slotList">スロット毎の搭載数</param>
        /// <param name="kammusuFlg">艦娘か？</param>
        public Kammusu(int id, string name, KammusuType type, int antiAir, List<int> slotList, bool kammusuFlg) {
            Id = id;
            Name = name;
            Level = 1;
            Type = type;
            AntiAir = antiAir;
            SlotList = slotList;
            WeaponList = new List<Weapon>();
            foreach(int slot in SlotList) {
                WeaponList.Add(null);
            }
            KammusuFlg = kammusuFlg;
        }

        /// <summary>
        /// 艦船ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 艦船名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// レベル
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 艦種
        /// </summary>
        public KammusuType Type { get; }

        /// <summary>
        /// 素対空値
        /// </summary>
        public int AntiAir { get; }

        /// <summary>
        /// スロット毎の搭載数
        /// </summary>
        public List<int> SlotList { get; }

        /// <summary>
        /// スロット毎の装備リスト
        /// (スロットに存在しない場合はnullが入る)
        /// </summary>
        public List<Weapon> WeaponList { get; }

        /// <summary>
        /// 艦娘か？
        /// </summary>
        public bool KammusuFlg { get; }
    }
}
