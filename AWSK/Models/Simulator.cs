using AWSK.Stores;
using System;
using System.Collections.Generic;

namespace AWSK.Models
{
	static class Simulator
	{
		// シミュレーションの試行回数
		private static int loopCount = 10000;

		// 制空値を計算する(1艦)
		private static int CalcAntiAirValue(List<WeaponData> weaponList, List<int> slotData) {
			int sum = 0;
			for(int wi = 0; wi < weaponList.Count; ++wi) {
				var weapon = weaponList[wi];
				// 補正後対空値
				double correctedAA = 1.0 * weapon.AntiAir + 1.0 * weapon.Rf + 1.5 * 0;
				// 補正後制空能力
				int correctedAAV = (int)(Math.Floor(correctedAA * Math.Sqrt(slotData[wi]) + weapon.AntiAirBonus));
				// 加算
				sum += correctedAAV;
			}
			return sum;
		}

		// 制空値を計算する(艦隊全体)
		private static int CalcAntiAirValue(FleetData fleet, List<List<List<int>>> slotData) {
			int sum = 0;
			for(int ui = 0; ui < fleet.Kammusu.Count; ++ui) {
				for(int ki = 0; ki < fleet.Kammusu[ui].Count; ++ki) {
					var kammusu = fleet.Kammusu[ui][ki];
					sum += CalcAntiAirValue(kammusu.Weapon, slotData[ui][ki]);
				}
			}
			return sum;
		}

		// 航空戦の基地航空隊におけるシミュレーションを行う
		public static void BasedAirUnitSimulation(BasedAirUnitData friend, FleetData enemy) {
			for(int li = 0; li < loopCount; ++li) {
				// 基地航空隊・敵艦隊のデータから、スロット毎の搭載数を読み取る
				var friendSlotData = friend.GetSlotData();
				var enemySlotData = enemy.GetSlotData();
				// 指定した回数だけ基地航空隊をぶつける
				for (int si = 0; si < friend.SallyCount.Count; ++si) {
					for (int ci = 0; ci < friend.SallyCount[si]; ++si) {
						// 基地航空隊・敵艦隊の制空値を計算する
						int friendAntiAirValue = CalcAntiAirValue(friend.Weapon[si], friendSlotData[si]);
						int enemyAntiAirValue = CalcAntiAirValue(enemy, enemySlotData);
						continue;
					}
				}
				return;
			}
		}
	}
}
