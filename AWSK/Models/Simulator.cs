using AWSK.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AWSK.Models
{
	static class Simulator
	{
		// シミュレーションの試行回数
		private static int loopCount = 10000;
		// 乱数の起点
		private static Random random = new Random();

		// 制空値を計算する(1艦)
		private static int CalcAntiAirValue(List<WeaponData> weaponList, List<int> slotData) {
			int sum = 0;
			for(int wi = 0; wi < weaponList.Count; ++wi) {
				var weapon = weaponList[wi];
				// 制空計算に参加する装備(艦戦・艦攻・艦爆・爆戦・噴式・水戦・水爆)かを判断する
				if (!weapon.HasAAV)
					continue;
				// 補正後対空値
				double correctedAA = 1.0 * weapon.AntiAir + 1.0 * weapon.Rf + 1.5 * weapon.Intercept;
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
		// 制空状況を判断する
		enum AirWarStatus { Best, Good, Even, Bad, Worst }
		private static AirWarStatus JudgeAirWarStatus(int friend, int enemy) {
			if(friend >= enemy * 3) {
				return AirWarStatus.Best;
			}else if(friend * 2 >= enemy * 3) {
				return AirWarStatus.Good;
			}else if(friend * 3 >= enemy * 2) {
				return AirWarStatus.Even;
			}else if(friend * 3 >= enemy) {
				return AirWarStatus.Bad;
			} else {
				return AirWarStatus.Worst;
			}
		}
		// St1撃墜を行う(敵艦隊)
		private static void LostEnemySlotBySt1(FleetData fleet, ref List<List<List<int>>> slotData, AirWarStatus aws) {
			// 制空定数
			var awStatusCoeff = new int[]{ 1, 3, 5, 7, 10 };
			// 計算
			for (int ui = 0; ui < fleet.Kammusu.Count; ++ui) {
				for (int ki = 0; ki < fleet.Kammusu[ui].Count; ++ki) {
					var kammusu = fleet.Kammusu[ui][ki];
					for (int wi = 0; wi < kammusu.Weapon.Count; ++wi) {
						var weapon = kammusu.Weapon[wi];
						int slot = slotData[ui][ki][wi];
						// St1撃墜を計算して書き戻す
						int rand1 = random.Next(12 - awStatusCoeff[(int)aws]);
						int rand2 = random.Next(12 - awStatusCoeff[(int)aws]);
						int killedSlot = (int)Math.Floor(slot * (0.35m * rand1 + 0.65m * rand2) / 10);
						slotData[ui][ki][wi] -= killedSlot;
					}
				}
			}
		}

		// 航空戦の基地航空隊におけるシミュレーションを行う
		public static void BasedAirUnitSimulation(
			BasedAirUnitData friend,
			FleetData enemy,
			out Dictionary<int, int> finalAAV,
			out List<List<List<int>>> awsCount) {
			// シミュレーションを行う
			finalAAV = new Dictionary<int, int>();
			awsCount = new List<List<List<int>>>();
			for (int si = 0; si < friend.SallyCount.Count; ++si) {
				var temp1 = new List<List<int>>();
				for (int ci = 0; ci < friend.SallyCount[si]; ++ci) {
					var temp2 = new List<int> { 0, 0, 0, 0, 0 };
					temp1.Add(temp2);
				}
				awsCount.Add(temp1);
			}
			for(int li = 0; li < loopCount; ++li) {
				// 基地航空隊・敵艦隊のデータから、スロット毎の搭載数を読み取る
				var friendSlotData = friend.GetSlotData();
				var enemySlotData = enemy.GetSlotData();
				// 指定した回数だけ基地航空隊をぶつける
				for (int si = 0; si < friend.SallyCount.Count; ++si) {
					for (int ci = 0; ci < friend.SallyCount[si]; ++ci) {
						// 基地航空隊・敵艦隊の制空値を計算する
						int friendAntiAirValue = CalcAntiAirValue(friend.Weapon[si], friendSlotData[si]);
						int enemyAntiAirValue = CalcAntiAirValue(enemy, enemySlotData);
						// 制空状況を判断する
						var airWarStatus = JudgeAirWarStatus(friendAntiAirValue, enemyAntiAirValue);
						++awsCount[si][ci][(int)airWarStatus];
						// St1撃墜を行う
						LostEnemySlotBySt1(enemy, ref enemySlotData, airWarStatus);
					}
				}
				// 最終制空値を読み取る
				int aav = CalcAntiAirValue(enemy, enemySlotData);
				if (finalAAV.ContainsKey(aav)) {
					++finalAAV[aav];
				} else {
					finalAAV[aav] = 1;
				}
			}
			finalAAV = finalAAV.OrderBy((x) => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}
