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
        public ReactiveProperty<string> TestProperty { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel() {
            // 各プロパティにインジェクションする
            SampleProperty = model.SampleProperty;

            TestProperty.Subscribe(value => {
                Console.WriteLine(value);
            });
        }
    }
}
