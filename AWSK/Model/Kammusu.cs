using System.Collections.Generic;
using static AWSK.Constant;

namespace AWSK.Models
{
  /// <summary>
  /// 艦娘を表現するクラス
  /// </summary>
  class Kammusu
  {
    /// <summary>
    /// 艦船ID
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// 艦船名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 艦種
    /// </summary>
    public KammusuType Type { get; }

    /// <summary>
    /// 素対空値
    /// </summary>
    public int AntiAir { get; }

    /// <summary>
    /// 艦娘か？
    /// </summary>
    public bool KammusuFlg { get; }

    /// <summary>
    /// スロット毎の搭載数
    /// </summary>
    public List<int> SlotList { get; }

    /// <summary>
    /// スロット毎の装備リスト
    /// (スロットに存在しない場合はnullが入る)
    /// </summary>
    public List<Weapon> WeaponList { get; }
  }
}
