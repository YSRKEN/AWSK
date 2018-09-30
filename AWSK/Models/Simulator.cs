using System;
using System.Collections.Generic;
using System.Linq;
using MersenneTwister;
using AWSK.Service;
using AWSK.Model;

namespace AWSK.Models {
    static class Simulator
	{
		// 乱数の起点
		private static Random random = DsfmtRandom.Create();
		// St1撃墜用のメモ。killedSlot[搭載数][制空状況]＝[可能性の一覧]
		private static List<List<List<int>>> killedSlot = PreCalcKilledSlot();
		// St1撃墜の幅
		private static List<int> st1Range;

        private static SimulationService simulation = SimulationService.instance;

        // 制空状況を判断する
        enum AirWarStatus { Best, Good, Even, Bad, Worst, Size }
		private static AirWarStatus JudgeAirWarStatus(int friend, int enemy) {
			if(friend >= enemy * 3) {
				return AirWarStatus.Best;
			}else if(friend * 2 >= enemy * 3) {
				return AirWarStatus.Good;
			}else if(friend * 3 > enemy * 2) {
				return AirWarStatus.Even;
			}else if(friend * 3 > enemy) {
				return AirWarStatus.Bad;
			} else {
				return AirWarStatus.Worst;
			}
		}
		// St1撃墜を行うための準備
		private static List<List<List<int>>> PreCalcKilledSlot() {
			// 制空定数
			int[] awStatusCoeff = new int[] { 1, 3, 5, 7, 10 };
			// 事前計算
			st1Range = new List<int>();
			for (int aws = 0; aws < (int)AirWarStatus.Size; ++aws) {
				st1Range.Add((12 - awStatusCoeff[aws]) * (12 - awStatusCoeff[aws]));
			}
			var killedSlot = new List<List<List<int>>>();
			for(int slot = 0; slot <= 300; ++slot) {
				var list1 = new List<List<int>>();
				for (int aws = 0; aws < (int)AirWarStatus.Size; ++aws) {
					var list2 = new List<int>();
					for (int i = 0; i <= 11 - awStatusCoeff[aws]; ++i) {
						for (int j = 0; j <= 11 - awStatusCoeff[aws]; ++j) {
							list2.Add(slot - slot * (35 * i + 65 * j) / 1000);
						}
					}
					list1.Add(list2);
				}
				killedSlot.Add(list1);
			}
			return killedSlot;
		}
		// St1撃墜を行う(敵艦隊)
		private static int CalcKilledSlot(int slot, AirWarStatus aws) {
			return killedSlot[slot][(int)aws][random.Next(st1Range[(int)aws])];
		}
		private static void LostEnemySlotBySt1(Fleet fleet, ref List<List<List<int>>> slotData, AirWarStatus aws) {
			// 計算
			for (int ui = 0; ui < fleet.KammusuList.Count; ++ui) {
				for (int ki = 0; ki < fleet.KammusuList[ui].Count; ++ki) {
					var kammusu = fleet.KammusuList[ui][ki];
					for (int wi = 0; wi < kammusu.WeaponList.Count; ++wi) {
						// St1撃墜を計算して書き戻す
						slotData[ui][ki][wi] = CalcKilledSlot(slotData[ui][ki][wi], aws);
					}
				}
			}
		}

		// 制空状況を文字列で返す
		public static string JudgeAirWarStatusStr(int friend, int enemy) {
			switch(JudgeAirWarStatus(friend, enemy)) {
			case AirWarStatus.Best:
				return "確保";
			case AirWarStatus.Good:
				return "優勢";
			case AirWarStatus.Even:
				return "均衡";
			case AirWarStatus.Bad:
				return "劣勢";
			case AirWarStatus.Worst:
				return "喪失";
			default:
				return "";
			}
		}
		// 制空値を計算する(1艦)
		// calcFlgがtrueなら、水上偵察機の制空値も反映するようにする
		public static int CalcAntiAirValue(List<Weapon> weaponList, List<int> slotData, bool calcFlg = false) {
			int sum = 0;
			for (int wi = 0; wi < weaponList.Count; ++wi) {
				// 加算
				sum += simulation.CalcAntiAirValue(weaponList[wi], slotData[wi], calcFlg);
			}
			return sum;
		}
		// 制空値を計算する(艦隊全体)
		// calcFlgがtrueなら、水上偵察機の制空値も反映するようにする
		public static int CalcAntiAirValue(Fleet fleet, List<List<List<int>>> slotData, bool calcFlg = false) {
			int sum = 0;
			for (int ui = 0; ui < fleet.KammusuList.Count; ++ui) {
				for (int ki = 0; ki < fleet.KammusuList[ui].Count; ++ki) {
					var kammusu = fleet.KammusuList[ui][ki];
					sum += CalcAntiAirValue(kammusu.WeaponList, slotData[ui][ki], calcFlg);
				}
			}
			return sum;
		}
		// 戦闘行動半径を計算する
		public static int CalcBAURange(List<Weapon> weaponList) {
			// 全ての装備における戦闘行動半径が0なら0を返す
			if (weaponList.Where(w => w.BasedAirUnitRange != 0).Count() == 0)
				return 0;
			// 航空隊の最低戦闘行動半径を算出する
			int minBAURange = weaponList.Min(w => w.BasedAirUnitRange);
			// 航空隊に偵察機が含まれるなら延長後の半径を返す
			// 含まれないならそのままの延長前の半径を返す
			var list = weaponList.Where(weapon => weapon.IsSearcher);
			if (list.Count() > 0) {
				int maxBAURange2 = list.Max(w => w.BasedAirUnitRange);
				if (maxBAURange2 <= minBAURange)
					return minBAURange;
				double addRange = Math.Sqrt(maxBAURange2 - minBAURange);
				int newBAURange = Math.Min((int)Math.Round(minBAURange + addRange), minBAURange + 3);
				return newBAURange;
			} else {
				return minBAURange;
			}
		}
		// 航空戦の基地航空隊におけるシミュレーションを行う
		public static void BasedAirUnitSimulation(
			BasedAirUnitGroup friend,
			Fleet enemy,
			int simulationCount,
			out Dictionary<int, double> finalAAV,
			out List<List<List<int>>> awsCount) {
			// 出力先を準備する
			finalAAV = new Dictionary<int, double>();	//最終的な制空値のデータ
			awsCount = new List<List<List<int>>>();	//制空状況をカウントする配列
			for (int si = 0; si < friend.BasedAirUnitList.Count; ++si) {
				var temp1 = new List<List<int>>();
				for (int ci = 0; ci < friend.BasedAirUnitList[si].SallyCount; ++ci) {
					var temp2 = new List<int> { 0, 0, 0, 0, 0 };
					temp1.Add(temp2);
				}
				awsCount.Add(temp1);
			}
			// テンポラリな変数を用意する
			var friendAntiAirValue = new List<int>();	//基地航空隊の制空値を記録した配列
			for (int si = 0; si < friend.BasedAirUnitList.Count; ++si) {
				friendAntiAirValue.Add(CalcAntiAirValue(friend.BasedAirUnitList[si].WeaponList, friend.SlotList[si]));
			}
			// シミュレーションを行う
			for (int li = 0; li < simulationCount; ++li) {
				// 基地航空隊・敵艦隊のデータから、スロット毎の搭載数を読み取る
				var enemySlotData = enemy.SlotList;
				// 指定した回数だけ基地航空隊をぶつける
				for (int si = 0; si < friend.BasedAirUnitList.Count; ++si) {
					for (int ci = 0; ci < friend.BasedAirUnitList[si].SallyCount; ++ci) {
						// 敵艦隊の制空値を計算する
						int enemyAntiAirValue = CalcAntiAirValue(enemy, enemySlotData, true);
						// 制空状況を判断する
						var airWarStatus = JudgeAirWarStatus(friendAntiAirValue[si], enemyAntiAirValue);
						++awsCount[si][ci][(int)airWarStatus];
						// St1撃墜を行う
						LostEnemySlotBySt1(enemy, ref enemySlotData, airWarStatus);
					}
				}
				// 最終制空値を読み取る
				int aav = CalcAntiAirValue(enemy, enemySlotData, true);
				if (finalAAV.ContainsKey(aav)) {
					++finalAAV[aav];
				} else {
					finalAAV[aav] = 1;
				}
			}
			finalAAV = finalAAV.OrderBy((x) => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value / simulationCount);
		}
	}
}
