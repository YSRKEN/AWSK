using System.Collections.Generic;
using static AWSK.Constant;

namespace AWSK.Models
{
  /// <summary>
  /// 装備を表現するクラス
  /// </summary>
  class Weapon
  {
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
