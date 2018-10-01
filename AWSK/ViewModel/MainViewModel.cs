using AWSK.Model;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.ViewModel {
    /// <summary>
    /// MainViewに対するViewModelクラス
    /// </summary>
    class MainViewModel {
        /// <summary>
        /// Model
        /// </summary>
        private MainModel model = new MainModel();

        public ReactiveProperty<string> SampleProperty { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel() {
            // 各プロパティにインジェクションする
            SampleProperty = model.SampleProperty;
        }
    }
}
