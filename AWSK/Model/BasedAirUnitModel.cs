using AWSK.Service;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.Model {
    class BasedAirUnitModel {
        /// <summary>
        /// データベースサービス
        /// </summary>
        private DataBaseService database = DataBaseService.Instance;

        /// <summary>
        /// index個目の要素を更新する
        /// </summary>
        /// <param name="index"></param>
        private void RefreshWeapon(string value, int index) {
            if (value != null) {
                var weapon = database.FindByWeaponName(value);
                BasedAirUnit.Value.WeaponList[index] = weapon;
            }
            if (BasedAirUnit.Value.WeaponList[index] != null) {
                BasedAirUnit.Value.WeaponList[index].Mas = MasterLevelIndex[index].Value;
                BasedAirUnit.Value.WeaponList[index].Rf = RefurbishmentLevelIndex[index].Value;
            }
        }

        /// <summary>
        /// 基地航空隊のデータ
        /// </summary>
        public ReactiveProperty<BasedAirUnit> BasedAirUnit { get; }

        /// <summary>
        /// 基地航空隊の番号
        /// </summary>
        public ReactiveProperty<string> BasedAirUnitUnitText { get; } = new ReactiveProperty<string>("第1航空隊");

        /// <summary>
        /// 基地航空隊の発艦回数
        /// </summary>
        public ReactiveProperty<int> SallyCount { get; } = new ReactiveProperty<int>(0);

        /// <summary>
        /// 基地航空隊が有効か？
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsEnabled { get; }

        /// <summary>
        /// 選択している装備名
        /// </summary>
        public List<ReactiveProperty<string>> WeaponName { get; } = new List<ReactiveProperty<string>>();

        /// <summary>
        /// 選択している熟練度
        /// </summary>
        public List<ReactiveProperty<int>> MasterLevelIndex { get; } = new List<ReactiveProperty<int>>();

        /// <summary>
        /// 選択している改修度
        /// </summary>
        public List<ReactiveProperty<int>> RefurbishmentLevelIndex { get; } = new List<ReactiveProperty<int>>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitModel() {
            // 基地航空隊部分を初期化
            BasedAirUnit = new ReactiveProperty<BasedAirUnit>(new BasedAirUnit());
            for(int i = 0; i < BasedAirUnitMaxSize; ++i) {
                BasedAirUnit.Value.WeaponList.Add(null);
            }

            // 装備選択部分を初期化
            for (int i = 0; i < BasedAirUnitMaxSize; ++i) {
                WeaponName.Add(new ReactiveProperty<string>(""));
                MasterLevelIndex.Add(new ReactiveProperty<int>(0));
                RefurbishmentLevelIndex.Add(new ReactiveProperty<int>(0));
            }
            WeaponName[0].Subscribe(value => RefreshWeapon(value, 0));
            WeaponName[1].Subscribe(value => RefreshWeapon(value, 1));
            WeaponName[2].Subscribe(value => RefreshWeapon(value, 2));
            WeaponName[3].Subscribe(value => RefreshWeapon(value, 3));
            MasterLevelIndex[0].Subscribe(value => RefreshWeapon(null, 0));
            MasterLevelIndex[1].Subscribe(value => RefreshWeapon(null, 1));
            MasterLevelIndex[2].Subscribe(value => RefreshWeapon(null, 2));
            MasterLevelIndex[3].Subscribe(value => RefreshWeapon(null, 3));
            RefurbishmentLevelIndex[0].Subscribe(value => RefreshWeapon(null, 0));
            RefurbishmentLevelIndex[1].Subscribe(value => RefreshWeapon(null, 1));
            RefurbishmentLevelIndex[2].Subscribe(value => RefreshWeapon(null, 2));
            RefurbishmentLevelIndex[3].Subscribe(value => RefreshWeapon(null, 3));

            // 特殊なプロパティを初期化
            SallyCount.Subscribe(value => BasedAirUnit.Value.SallyCount = value);
            IsEnabled = SallyCount.Select(c => c != 0).ToReadOnlyReactiveProperty();
        }
    }
}
