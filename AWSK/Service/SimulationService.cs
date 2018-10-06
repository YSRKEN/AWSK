using AWSK.Model;
using AWSK.Models;
using MersenneTwister;
using System;
using System.Collections.Generic;
using System.Linq;
using static AWSK.Constant;

namespace AWSK.Service {
    /// <summary>
    /// シミュレーションを行うサービスクラス
    /// </summary>
    class SimulationService {
        /// <summary>
        /// 自分自身の唯一のinstance
        /// </summary>
        private static SimulationService singleton = null;

        // 乱数の起点
        private Random random = DsfmtRandom.Create();

        // St1撃墜用のメモ。killedSlot[搭載数][制空状況]＝[可能性の一覧]
        private readonly List<List<List<int>>> killedSlot;

        /// <summary>
        /// St1撃墜用のメモを事前計算する
        /// </summary>
        private List<List<List<int>>> CalcKilledSlot() {
            // 制空定数
            int[] awStatusCoeff = new int[] { 1, 3, 5, 7, 10 };

            // 事前計算
            var killedSlot = new List<List<List<int>>>();
            for (int slot = 0; slot <= MaxSlotSize; ++slot) {
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

        /// <summary>
        /// privateコンストラクタ
        /// </summary>
        private SimulationService() {
            // 事前計算を行う
            killedSlot = CalcKilledSlot();
        }

        /// <summary>
        /// 1スロットに対してSt1撃墜を行う(敵艦隊)
        /// </summary>
        /// <param name="slot">元のスロット数</param>
        /// <param name="aws">制空状態</param>
        /// <returns>撃墜したスロット数(乱数)</returns>
        private int CalcKilledSlot(int slot, AirWarStatus aws) {
            var temp = killedSlot[slot][(int)aws];
            return temp[random.Next(temp.Count)];
        }

        /// <summary>
        /// 艦隊の全スロットに対してSt1撃墜を行う(敵艦隊)
        /// </summary>
        /// <param name="fleet">艦隊の情報</param>
        /// <param name="slotData">現在の搭載数</param>
        /// <param name="aws">制空状態</param>
        private void LostEnemySlotBySt1(Fleet fleet, ref List<List<List<int>>> slotData, AirWarStatus aws) {
            // 計算
            for (int ui = 0; ui < fleet.KammusuList.Count; ++ui) {
                var kammusuList = fleet.KammusuList[ui];
                for (int ki = 0; ki < kammusuList.Count; ++ki) {
                    var kammusu = kammusuList[ki];
                    for (int wi = 0; wi < kammusu.WeaponList.Count; ++wi) {
                        // St1撃墜を計算して書き戻す
                        slotData[ui][ki][wi] = CalcKilledSlot(slotData[ui][ki][wi], aws);
                    }
                }
            }
        }

        /// <summary>
        /// 唯一のinstanceを返す(Singletonパターン)
        /// </summary>
        /// <returns></returns>
        public static SimulationService Instance {
            get {
                if (singleton == null) {
                    singleton = new SimulationService();
                }
                return singleton;
            }
        }

        /// <summary>
        /// 制空状況を判断する
        /// </summary>
        /// <param name="friend">自制空値</param>
        /// <param name="enemy">敵制空値</param>
        /// <returns>判定結果</returns>
        public AirWarStatus JudgeAirWarStatus(int friend, int enemy) {
            if (friend >= enemy * 3) {
                return AirWarStatus.Best;
            } else if (friend * 2 >= enemy * 3) {
                return AirWarStatus.Good;
            } else if (friend * 3 > enemy * 2) {
                return AirWarStatus.Even;
            } else if (friend * 3 > enemy) {
                return AirWarStatus.Bad;
            } else {
                return AirWarStatus.Worst;
            }
        }

        /// <summary>
        /// 装備リストから戦闘行動半径を計算する
        /// </summary>
        /// <param name="weaponList">装備リスト</param>
        /// <returns>戦闘行動半径</returns>
        public int CalcBAURange(List<Weapon> weaponList) {
            // 全ての装備における戦闘行動半径が0なら0を返す
            if (weaponList.Where(w => w.BasedAirUnitRange != 0).Count() == 0)
                return 0;

            // 航空隊の最低戦闘行動半径を算出する
            int minBAURange = weaponList.Min(w => w.BasedAirUnitRange);
            
            // 航空隊に偵察機が含まれるなら延長後の半径を返す
            // 含まれないならそのままの延長前の半径を返す
            var list = weaponList.Where(weapon => weapon.IsSearcher);
            if (list.Count() == 0) {
                return minBAURange;
            }
            int maxBAURange2 = list.Max(w => w.BasedAirUnitRange);
            if (maxBAURange2 <= minBAURange)
                return minBAURange;
            double addRange = Math.Sqrt(maxBAURange2 - minBAURange);
            int newBAURange = Math.Min((int)Math.Round(minBAURange + addRange), minBAURange + 3);
            return newBAURange;
        }

        /// <summary>
        /// 装備1つにおける制空値を計算する
        /// </summary>
        /// <param name="weapon">装備</param>
        /// <param name="slotSize">搭載数</param>
        /// <param name="wsFlg">水偵を制空値計算に含める場合はtrue</param>
        /// <returns></returns>
        public int CalcAntiAirValue(Weapon weapon, int slotSize, bool wsFlg) {
            // 航空戦に参加しない装備の場合はスルー
            if (!weapon.HasAAV(wsFlg)) {
                return 0;
            }

            // 熟練度補正を計算
            double antiAirBonus = Math.Sqrt(1.0 * MasBonus[weapon.Mas] / 10);
            if (weapon.IsFighter) {
                // 艦戦・水戦・陸戦・局戦は+25
                antiAirBonus += PfWfBonus[weapon.Mas];
            } else if (weapon.Type == WeaponType.WB) {
                // 水爆は+9
                antiAirBonus += WbBonus[weapon.Mas];
            }

            // 最終的な制空値を算出
            return (int)(weapon.CorrectedAntiAir * Math.Sqrt(slotSize) + antiAirBonus);
        }

        /// <summary>
        /// 艦娘1隻における制空値を計算する
        /// </summary>
        /// <param name="weaponList">各装備の情報</param>
        /// <param name="slotData">現在の搭載数</param>
        /// <param name="wsFlg">水偵を制空値計算に含める場合はtrue</param>
        /// <returns></returns>
        public int CalcAntiAirValue(List<Weapon> weaponList, List<int> slotData, bool wsFlg = false) {
            int sum = 0;
            for (int wi = 0; wi < weaponList.Count; ++wi) {
                if (weaponList[wi] == null)
                    continue;

                // 加算
                sum += CalcAntiAirValue(weaponList[wi], slotData[wi], wsFlg);
            }
            return sum;
        }

        /// <summary>
        /// 艦隊全体における制空値を計算する
        /// </summary>
        /// <param name="fleet">艦隊の情報</param>
        /// <param name="slotData">現在の搭載数</param>
        /// <param name="wsFlg">水偵を制空値計算に含める場合はtrue</param>
        /// <returns></returns>
        public int CalcAntiAirValue(Fleet fleet, List<List<List<int>>> slotData, bool wsFlg = false) {
            int sum = 0;
            for (int ui = 0; ui < fleet.KammusuList.Count; ++ui) {
                var kammusuList = fleet.KammusuList[ui];
                for (int ki = 0; ki < kammusuList.Count; ++ki) {
                    var kammusu = kammusuList[ki];
                    sum += CalcAntiAirValue(kammusu.WeaponList, slotData[ui][ki], wsFlg);
                }
            }
            return sum;
        }

        /// <summary>
        /// 基地航空隊を敵艦隊にぶつけた際のシミュレーションを行う
        /// (シミュレーション対象：St1まで)
        /// </summary>
        /// <param name="friend">基地航空隊の情報</param>
        /// <param name="enemy">敵艦隊の情報</param>
        /// <param name="simulationCount">試行回数</param>
        /// <param name="finalAAV">敵の最終制空値を、制空値=>確率で表現したもの</param>
        /// <param name="awsCount">制空状況のカウント。[航空隊番号][攻撃回数][各制空状態]</param>
        public void BasedAirUnitSimulation(
            BasedAirUnitGroup friend,
            Fleet enemy,
            int simulationCount,
            out Dictionary<int, double> finalAAV,
            out List<List<List<int>>> awsCount) {
            // 出力先を準備する
            finalAAV = new Dictionary<int, double>();
            awsCount = new List<List<List<int>>>();
            for (int si = 0; si < friend.BasedAirUnitList.Count; ++si) {
                var basedAirUnit = friend.BasedAirUnitList[si];
                var temp1 = new List<List<int>>();
                for (int ci = 0; ci < basedAirUnit.SallyCount; ++ci) {
                    var temp2 = new List<int> { 0, 0, 0, 0, 0 };
                    temp1.Add(temp2);
                }
                awsCount.Add(temp1);
            }

            // 「基地航空隊の制空値を記録した配列」を事前に準備する
            var friendAntiAirValue = new List<int>();
            for (int si = 0; si < friend.BasedAirUnitList.Count; ++si) {
                friendAntiAirValue.Add(CalcAntiAirValue(friend.BasedAirUnitList[si].WeaponList, friend.SlotList[si]));
            }

            // シミュレーションを行う
            for (int li = 0; li < simulationCount; ++li) {
                // 基地航空隊・敵艦隊のデータから、スロット毎の搭載数を読み取る
                var enemySlotData = enemy.SlotList;

                // 指定した回数だけ基地航空隊をぶつける
                for (int si = 0; si < friend.BasedAirUnitList.Count; ++si) {
                    var basedAirUnit = friend.BasedAirUnitList[si];
                    for (int ci = 0; ci < basedAirUnit.SallyCount; ++ci) {
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

            // この段階では、finalAAVの値が「制空値=>それになった回数」となっており、また制空値でソートされていない。
            // そこで、LINQを使って制空値キーでソートし、また「制空値=>それになった確率」と構築し直す
            finalAAV = finalAAV.OrderBy((x) => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value / simulationCount);
        }
    }
}
