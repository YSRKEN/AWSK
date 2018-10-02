using AWSK.Model;
using AWSK.Models;
using AWSK.Service;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWSK.Constant;

namespace AWSK.ViewModel {
    /// <summary>
    /// MainViewに対するViewModelクラス
    /// </summary>
    class MainViewModel {
        /// <summary>
        /// Model
        /// </summary>
        private readonly MainModel model = new MainModel();

        public ReactiveProperty<Weapon> SampleWeapon { get; }

        public ReactiveCommand SampleCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel() {
            var database = DataBaseService.Instance;
            SampleWeapon = new ReactiveProperty<Weapon>(database.FindByWeaponName("烈風"));
            // 各プロパティにインジェクションする
            SampleCommand.Subscribe(() => {
                Console.WriteLine($"{WeaponTypeDicShort[SampleWeapon.Value.Type]} {SampleWeapon.Value.Name} {SampleWeapon.Value.Mas} {SampleWeapon.Value.Rf}");
            });
        }
    }
}
