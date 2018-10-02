using AWSK.Model;
using AWSK.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.ViewModel {
    class BasedAirUnitViewModel {
        /// <summary>
        /// model
        /// </summary>
        private BasedAirUnitModel model = new BasedAirUnitModel();

        public List<ReactiveProperty<Weapon>> WeaponList;

        public ReactiveProperty<string> BasedAirUnitUnitText;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitViewModel() {
            WeaponList = model.WeaponList;
            BasedAirUnitUnitText = model.BasedAirUnitUnitText;
        }
    }
}
