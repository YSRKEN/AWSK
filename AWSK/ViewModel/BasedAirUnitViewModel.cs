using AWSK.Model;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.ViewModel {
    class BasedAirUnitViewModel {
        /// <summary>
        /// Model
        /// </summary>
        private BasedAirUnitModel model = new BasedAirUnitModel();

        /// <summary>
        /// 基地航空隊のデータ
        /// </summary>
        public ReactiveProperty<BasedAirUnit> BasedAirUnit { get; }

        /// <summary>
        /// 基地航空隊の番号
        /// </summary>
        public ReactiveProperty<string> BasedAirUnitUnitText { get; }

        /// <summary>
        /// 基地航空隊の発艦回数
        /// </summary>
        public ReactiveProperty<int> SallyCount { get; }

        /// <summary>
        /// 基地航空隊が有効か？
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsEnabled { get; }

        /// <summary>
        /// 選択している装備名
        /// </summary>
        public List<ReactiveProperty<string>> WeaponName { get; }

        /// <summary>
        /// 選択している熟練度
        /// </summary>
        public List<ReactiveProperty<int>> MasterLevelIndex { get; }

        /// <summary>
        /// 選択している改修度
        /// </summary>
        public List<ReactiveProperty<int>> RefurbishmentLevelIndex { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitViewModel() {
            this.BasedAirUnit = model.BasedAirUnit;
            this.BasedAirUnitUnitText = model.BasedAirUnitUnitText;
            this.SallyCount = model.SallyCount;
            this.IsEnabled = model.IsEnabled;
            this.WeaponName = model.WeaponName;
            this.MasterLevelIndex = model.MasterLevelIndex;
            this.RefurbishmentLevelIndex = model.RefurbishmentLevelIndex;
        }
    }
}
