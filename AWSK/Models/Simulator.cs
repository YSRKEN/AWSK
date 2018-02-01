using AWSK.Stores;

namespace AWSK.Models
{
	static class Simulator
	{
		// シミュレーションの試行回数
		private static int loopCount = 10000;

		// 航空戦の基地航空隊におけるシミュレーションを行う
		public static void BasedAirUnitSimulation(BasedAirUnitData friend, FleetData enemy) {
			for(int li = 0; li < loopCount; ++li) {
				// 基地航空隊のデータから、スロット毎の搭載数を読み取る
				var friendSlotData = friend.GetSlotData();
				// 敵艦隊のデータから、スロット毎の搭載数を読み取る
				var enemySlotData = enemy.GetSlotData();

				return;
			}
		}
	}
}
