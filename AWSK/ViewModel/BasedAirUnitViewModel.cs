using AWSK.Model;
using AWSK.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.ViewModel {
    class BasedAirUnitViewModel {
        /// <summary>
        /// model
        /// </summary>
        private BasedAirUnitModel model = new BasedAirUnitModel();

        private List<ReactiveProperty<Weapon>> WeaponList;

        public List<SelectWeaponViewModel> SelectWeaponViewModelList { get; }

        public ReactiveProperty<string> BasedAirUnitUnitText { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitViewModel() {
            WeaponList = model.WeaponList;
            SelectWeaponViewModelList = new List<SelectWeaponViewModel>() {
                new SelectWeaponViewModel(),
                new SelectWeaponViewModel(),
                new SelectWeaponViewModel(),
                new SelectWeaponViewModel(),
            };
            for (int i = 0; i < BasedAirUnitMaxSize; ++i) {
                SelectWeaponViewModelList[i].Weapon.Value = WeaponList[i].Value;
            }
            BasedAirUnitUnitText = model.BasedAirUnitUnitText;
        }
    }
}
