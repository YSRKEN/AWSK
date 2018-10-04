using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Model
{
    /// <summary>
    /// 敵編成検索画面のViewModel
    /// </summary>
    class PresetLoaderViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PresetLoaderModel model = new PresetLoaderModel();

        /// <summary>
        ///  出撃マップ一覧
        /// </summary>
        public ReadOnlyReactiveCollection<string> MapList { get; private set; }

        /// <summary>
        /// async/awaitを伴う初期化
        /// </summary>
        private async void initialize() {
            MapList = (await model.GetMapList()).ToReadOnlyReactiveCollection();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PresetLoaderViewModel() {
            initialize();
        }
    }
}
