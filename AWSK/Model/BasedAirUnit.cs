using AWSK.Models;
using System.Collections.Generic;

namespace AWSK.Model
{
  /// <summary>
  /// 基地航空隊を表現するクラス
  /// </summary>
  class BasedAirUnit
  {
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
    /// 出撃回数
    /// </summary>
    public int SallyCount { get; }
  }
}
