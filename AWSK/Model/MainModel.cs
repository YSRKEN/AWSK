using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Model {
    /// <summary>
    /// MainViewに対するModelクラス
    /// </summary>
    class MainModel {
        public ReactiveProperty<string> SampleProperty { get; } = new ReactiveProperty<string>("烈風");

        public MainModel() {
            SampleProperty.Subscribe(value => {
                Console.WriteLine(value);
            });
        }
    }
}
