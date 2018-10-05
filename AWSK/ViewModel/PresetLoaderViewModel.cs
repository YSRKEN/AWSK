using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AWSK.Model {
    /// <summary>
    /// 敵編成検索画面のViewModel
    /// </summary>
    class PresetLoaderViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PresetLoaderModel model = new PresetLoaderModel();

        /// <summary>
        ///  出撃マップ一覧
        /// </summary>
        public ObservableCollection<string> MapList { get; } = new ObservableCollection<string>();

        /// <summary>
        /// async/awaitを伴う初期化
        /// </summary>
        private async void initialize() {
            // マップ一覧を取得
            var list = await model.GetMapList();
            if (list.Count == 0) {
                MessageBox.Show("マップ情報をダウンロードできませんでした。", "AWSK", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (string mapName in list) {
                MapList.Add(mapName);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PresetLoaderViewModel() {
            initialize();
        }
    }
}
