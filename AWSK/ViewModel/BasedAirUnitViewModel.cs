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
        public ReactiveProperty<BasedAirUnitModel> Model = new ReactiveProperty<BasedAirUnitModel>(new BasedAirUnitModel());

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitViewModel() {
        }
    }
}
