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

namespace AWSK.Model {
    /// <summary>
    /// SelectWeaponBoxに対するModelクラス
    /// </summary>
    class SelectWeaponModel {

        #region プロパティ
        /// <summary>
        /// 装備種
        /// </summary>
        public ReactiveProperty<string> Category { get; }

        /// <summary>
        /// 装備種リスト
        /// </summary>
        public ReadOnlyReactiveCollection<string> CategoryList { get; }

        /// <summary>
        /// 装備名
        /// </summary>
        public ReactiveProperty<string> Name { get; }

        /// <summary>
        /// 装備名リスト
        /// </summary>
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
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectWeaponModel() {
            // 装備種、装備種リストを初期化
            CategoryList = new ObservableCollection<string>(
                BAUWeaponTypeSet.Select(t => WeaponTypeDic[t])
            ).ToReadOnlyReactiveCollection();
            Category = new ReactiveProperty<string>(CategoryList[0]);
        }
    }
}
