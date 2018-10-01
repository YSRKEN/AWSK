using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSK.Model;

namespace AWSK.ViewModel {
    /// <summary>
    /// SelectWeaponBoxに対するViewModelクラス
    /// </summary>
    class SelectWeaponViewModel {
        /// <summary>
        /// Model
        /// </summary>
        private SelectWeaponModel model = new SelectWeaponModel();

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
        public ReactiveProperty<int> MasterLevel { get; }

        /// <summary>
        /// 熟練度リスト
        /// </summary>
        public ReadOnlyReactiveCollection<string> MasterLevelList { get; }

        /// <summary>
        /// 改修度
        /// </summary>
        public ReactiveProperty<int> RefurbishmentLevel { get; }

        /// <summary>
        /// 改修度リスト
        /// </summary>
        public ReadOnlyReactiveCollection<string> RefurbishmentLevelList { get; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectWeaponViewModel() {
            // 各プロパティにインジェクションする
            this.Category = model.Category;
            this.CategoryList = model.CategoryList;
            this.Name = model.Name;
            this.NameList = model.NameList;
            this.MasterLevel = model.MasterLevel;
            this.MasterLevelList = model.MasterLevelList;
            this.RefurbishmentLevel = model.RefurbishmentLevel;
            this.RefurbishmentLevelList = model.RefurbishmentLevelList;
        }
    }
}
