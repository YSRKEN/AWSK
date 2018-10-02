using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AWSK.Constant;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using AWSK.Service;
using System.Collections;
using AWSK.Models;

namespace AWSK.Model {
    /// <summary>
    /// SelectWeaponBoxに対するModelクラス
    /// </summary>
    class SelectWeaponModel {

        #region プロパティ
        /// <summary>
        /// 装備種
        /// </summary>
        public ReactiveProperty<string> Category { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// 装備種リスト
        /// </summary>
        public ReadOnlyReactiveCollection<string> CategoryList { get; }

        /// <summary>
        /// 装備名
        /// </summary>
        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// 装備名リスト
        /// </summary>
        private ObservableCollection<string> nameList = new ObservableCollection<string>();
        public ReadOnlyReactiveCollection<string> NameList { get; }

        /// <summary>
        /// 熟練度
        /// </summary>
        public ReactiveProperty<int> MasterLevel { get; } = new ReactiveProperty<int>(0);

        /// <summary>
        /// 熟練度リスト
        /// </summary>
        public ReadOnlyReactiveCollection<string> MasterLevelList { get; }

        /// <summary>
        /// 改修度
        /// </summary>
        public ReactiveProperty<int> RefurbishmentLevel { get; } = new ReactiveProperty<int>(0);

        /// <summary>
        /// 改修度リスト
        /// </summary>
        public ReadOnlyReactiveCollection<string> RefurbishmentLevelList { get; }

        /// <summary>
        /// 装備
        /// </summary>
        public ReactiveProperty<Weapon> Weapon { get; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectWeaponModel() {
            // 装備種リストを初期化
            CategoryList = new ObservableCollection<string>(
                BAUWeaponTypeSet.Select(t => WeaponTypeDicShort[t])
            ).ToReadOnlyReactiveCollection();

            // 装備名リストを「装備種の選択に追従するように」初期化
            NameList = nameList.ToReadOnlyReactiveCollection();
            Category.Subscribe(value => {
                if (value == null)
                    return;
                var database = DataBaseService.Instance;
                nameList.Clear();
                nameList.Add("");
                database.FindByType(WeaponTypeReverseDicShort[value]).ForEach(w => nameList.Add(w.Name));
                Name.Value = nameList[0];
            });

            // 熟練度リストを初期化
            MasterLevelList = new ObservableCollection<string>(MasStringList).ToReadOnlyReactiveCollection();

            // 改修度リストを初期化
            RefurbishmentLevelList = new ObservableCollection<string>(Enumerable.Range(0, 11).Select(i => i.ToString())).ToReadOnlyReactiveCollection();

            // 装備を初期化
            Weapon = new ReactiveProperty<Weapon>(new Weapon());
            Category.Subscribe(value => {
                if (value == null)
                    return;
                Weapon.Value.Type = WeaponTypeReverseDicShort[value];
            });
            Name.Subscribe(value => {
                if (value == null)
                    return;
                Weapon.Value.Name = value;
            });
            MasterLevel.Subscribe(value => {
                Weapon.Value.Mas = value;
            });
            RefurbishmentLevel.Subscribe(value => {
                Weapon.Value.Rf = value;
            });
        }
    }
}
