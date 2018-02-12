using AWSK.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using MersenneTwister;

namespace AWSK.Models
{
	static class Simulator
	{
		// 乱数の起点
		private static Random random = DsfmtRandom.Create();

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
						int slot = slotData[ui][ki][wi];
						// St1撃墜を計算して書き戻す
						int rand1 = random.Next(12 - awStatusCoeff[(int)aws]);
						int rand2 = random.Next(12 - awStatusCoeff[(int)aws]);
						int killedSlot = slot * (35 * rand1 + 65 * rand2) / 1000;
						slotData[ui][ki][wi] -= killedSlot;
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
		// 制空値を計算する(1装備)
		// calcFlgがtrueなら、水上偵察機の制空値も反映するようにする
		public static int CalcAntiAirValue(WeaponData weapon, int slot, bool calcFlg = false) {
			// 制空計算に参加する装備(艦戦・艦攻・艦爆・爆戦・噴式・水戦・水爆)かを判断する
			// calcFlgがtrueなら、水上偵察機も制空計算に反映する
			if (calcFlg) {
				if (!weapon.HasAAV2)
					return 0;
			} else {
				if (!weapon.HasAAV)
					return 0;
			}
			// 補正後対空値
			double correctedAA = 1.0 * weapon.AntiAir + 1.5 * weapon.Intercept;
			//改修効果補正(艦戦・水戦・陸戦は★×0.2、爆戦は★×0.25だけ追加。局戦は★×0.2とした)
			if ((weapon.Type[0] == 3 && weapon.Type[2] == 6)
				|| (weapon.Type[0] == 5 && weapon.Type[1] == 36)
				|| (weapon.Type[0] == 22 && weapon.Type[2] == 48)) {
				correctedAA += 0.2 * weapon.Rf;
			} else if (weapon.Type[0] == 3 && weapon.Type[2] == 7 && weapon.Type[4] == 12) {
				correctedAA += 0.25 * weapon.Rf;
			}
			// 補正後制空能力
			int correctedAAV = (int)(Math.Floor(correctedAA * Math.Sqrt(slot) + weapon.AntiAirBonus));
			return correctedAAV;
		}
		// 制空値を計算する(1艦)
		// calcFlgがtrueなら、水上偵察機の制空値も反映するようにする
		public static int CalcAntiAirValue(List<WeaponData> weaponList, List<int> slotData, bool calcFlg = false) {
			int sum = 0;
			for (int wi = 0; wi < weaponList.Count; ++wi) {
				// 加算
				sum += CalcAntiAirValue(weaponList[wi], slotData[wi]);
			}
			return sum;
		}
		// 制空値を計算する(艦隊全体)
		// calcFlgがtrueなら、水上偵察機の制空値も反映するようにする
		public static int CalcAntiAirValue(FleetData fleet, List<List<List<int>>> slotData, bool calcFlg = false) {
			int sum = 0;
			for (int ui = 0; ui < fleet.Kammusu.Count; ++ui) {
				for (int ki = 0; ki < fleet.Kammusu[ui].Count; ++ki) {
					var kammusu = fleet.Kammusu[ui][ki];
					sum += CalcAntiAirValue(kammusu.Weapon, slotData[ui][ki], calcFlg);
				}
			}
			return sum;
		}
		// 戦闘行動半径を計算する
		public static int CalcBAURange(List<WeaponData> weaponList) {
			// 全ての装備における戦闘行動半径が0なら0を返す
			if (weaponList.Where(w => w.BAURange != 0).Count() == 0)
				return 0;
			// 航空隊の最低戦闘行動半径を算出する
			int minBAURange = weaponList.Min(w => w.BAURange);
			// 航空隊に偵察機が含まれるなら延長後の半径を返す
			// 含まれないならそのままの延長前の半径を返す
			var list = weaponList.Where(w =>
				((w.Type[0] == 5 && w.Type[1] == 7)
				|| w.Type[0] == 17)
			);
			if (list.Count() > 0) {
				int maxBAURange2 = list.Max(w => w.BAURange);
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
			BasedAirUnitData friend,
			FleetData enemy,
			int simulationCount,
			out Dictionary<int, double> finalAAV,
			out List<List<List<int>>> awsCount) {
			// 出力先を準備する
			finalAAV = new Dictionary<int, double>();	//最終的な制空値のデータ
			awsCount = new List<List<List<int>>>();	//制空状況をカウントする配列
			for (int si = 0; si < friend.SallyCount.Count; ++si) {
				var temp1 = new List<List<int>>();
				for (int ci = 0; ci < friend.SallyCount[si]; ++ci) {
					var temp2 = new List<int> { 0, 0, 0, 0, 0 };
					temp1.Add(temp2);
				}
				awsCount.Add(temp1);
			}
			// テンポラリな変数を用意する
			var friendAntiAirValue = new List<int>();	//基地航空隊の制空値を記録した配列
			for (int si = 0; si < friend.SallyCount.Count; ++si) {
				friendAntiAirValue.Add(CalcAntiAirValue(friend.Weapon[si], friend.GetSlotData()[si]));
			}
			var slotProb = new List<List<Dictionary<int, double>>>();
			for (int ki = 0; ki < enemy.Kammusu.First().Count; ++ki) {
				var list = new List<Dictionary<int, double>>();
				for(int wi = 0; wi < enemy.Kammusu.First()[ki].Weapon.Count; ++wi) {
					var dic = new Dictionary<int, double>();
					list.Add(dic);
				}
				slotProb.Add(list);
			}
			// シミュレーションを行う
			for (int li = 0; li < simulationCount; ++li) {
				// 基地航空隊・敵艦隊のデータから、スロット毎の搭載数を読み取る
				var enemySlotData = enemy.GetSlotData();
				// 指定した回数だけ基地航空隊をぶつける
				for (int si = 0; si < friend.SallyCount.Count; ++si) {
					for (int ci = 0; ci < friend.SallyCount[si]; ++ci) {
						// 敵艦隊の制空値を計算する
						int enemyAntiAirValue = CalcAntiAirValue(enemy, enemySlotData, true);
						// 制空状況を判断する
						var airWarStatus = JudgeAirWarStatus(friendAntiAirValue[si], enemyAntiAirValue);
						++awsCount[si][ci][(int)airWarStatus];
						// St1撃墜を行う
						LostEnemySlotBySt1(enemy, ref enemySlotData, airWarStatus);
					}
				}
				// スロット情報を登録する
				for (int ki = 0; ki < enemy.Kammusu.First().Count; ++ki) {
					for (int wi = 0; wi < enemy.Kammusu.First()[ki].Weapon.Count; ++wi) {
						int key = enemySlotData[0][ki][wi];
						if (slotProb[ki][wi].ContainsKey(key)) {
							slotProb[ki][wi][key] += 1.0;
						} else {
							slotProb[ki][wi][key] = 1.0;
						}
					}
				}
				// 最終制空値を読み取る
				/*int aav = CalcAntiAirValue(enemy, enemySlotData, true);
				if (finalAAV.ContainsKey(aav)) {
					++finalAAV[aav];
				} else {
					finalAAV[aav] = 1;
				}*/
			}
			// スロットの各数をシミュレーション回数で割る(誤差対策)
			for (int ki = 0; ki < enemy.Kammusu.First().Count; ++ki) {
				for (int wi = 0; wi < enemy.Kammusu.First()[ki].Weapon.Count; ++wi) {
					var list = slotProb[ki][wi].ToList();
					foreach (var pair in list) {
						slotProb[ki][wi][pair.Key] /= simulationCount;
					}
				}
			}
			// スロット毎に制空値の分布を算出し、畳み込む
			finalAAV[0] = 1.0;
			for (int ki = 0; ki < enemy.Kammusu.First().Count; ++ki) {
				for (int wi = 0; wi < enemy.Kammusu.First()[ki].Weapon.Count; ++wi) {
					// 搭載数分布から制空値分布を作成
					var aavDic = new Dictionary<int, double>();
					var slotProbtemp = slotProb[ki][wi];
					foreach(var pair in slotProbtemp) {
						int aav = CalcAntiAirValue(enemy.Kammusu.First()[ki].Weapon[wi], pair.Key, true);
						if (aavDic.ContainsKey(aav)) {
							aavDic[aav] += pair.Value;
						} else {
							aavDic[aav] = pair.Value;
						}
					}
					// 制空値分布を畳み込む
					var aavDic2 = new Dictionary<int, double>();
					foreach (var pair1 in finalAAV) {
						foreach (var pair2 in aavDic) {
							int key = pair1.Key + pair2.Key;
							double value = pair1.Value * pair2.Value;
							if (aavDic2.ContainsKey(key)) {
								aavDic2[key] += value;
							} else {
								aavDic2[key] = value;
							}
						}
					}
					// 上書きする
					finalAAV = aavDic2;
				}
			}
			finalAAV = finalAAV.OrderBy((x) => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}
