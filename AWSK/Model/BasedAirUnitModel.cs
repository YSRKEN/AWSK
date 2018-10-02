using AWSK.Models;
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
        private readonly DataBaseService database = DataBaseService.Instance;

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

        public List<ReactiveProperty<Weapon>> WeaponList;

        /// <summary>
        /// 基地航空隊が有効か？
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsEnabled { get; }

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
            WeaponList = new List<ReactiveProperty<Weapon>>();
            for (int i = 0; i < BasedAirUnitMaxSize; ++i) {
                WeaponList.Add(new ReactiveProperty<Weapon>());
            }
            for (int i = 0; i < BasedAirUnitMaxSize; ++i) {
                WeaponList[i].Subscribe(value => {
                    BasedAirUnit.Value.WeaponList[i] = value;
                });
            }

            // 特殊なプロパティを初期化
            SallyCount.Subscribe(value => BasedAirUnit.Value.SallyCount = value);
            IsEnabled = SallyCount.Select(c => c != 0).ToReadOnlyReactiveProperty();
        }
    }
}
