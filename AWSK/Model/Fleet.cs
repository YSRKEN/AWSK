using AWSK.Models;
using System.Collections.Generic;

namespace AWSK.Model
{
  /// <summary>
  /// 艦隊を表現するクラス
  /// </summary>
  class Fleet
  {
    /// <summary>
    /// 艦隊に含まれている艦娘一覧
    /// </summary>
    public List<List<Kammusu>> Kammusu { get; }
  }
}
