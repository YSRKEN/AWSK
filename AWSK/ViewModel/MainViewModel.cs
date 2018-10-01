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

        public ReactiveProperty<string> SampleProperty { get; } = new ReactiveProperty<string>("瑞雲");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel() {
            // 各プロパティにインジェクションする
        }
    }
}
