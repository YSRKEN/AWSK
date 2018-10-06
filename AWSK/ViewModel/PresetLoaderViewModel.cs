using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static AWSK.Constant;

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
        /// 出撃マップの選択
        /// </summary>
        public ReactiveProperty<int> MapSelectIndex { get; } = new ReactiveProperty<int>(-1);

        /// <summary>
        /// 難易度一覧
        /// </summary>
        public ObservableCollection<string> LevelList { get; } = new ObservableCollection<string>(MapLevelDoc.Keys);

        /// <summary>
        /// 難易度の選択
        /// </summary>
        public ReactiveProperty<int> LevelSelectIndex { get; } = new ReactiveProperty<int>(0);

        /// <summary>
        ///  マス一覧
        /// </summary>
        public ObservableCollection<string> PointList { get; } = new ObservableCollection<string>();

        /// <summary>
        ///  マスの選択
        /// </summary>
        public ReactiveProperty<int> PointSelectIndex { get; } = new ReactiveProperty<int>(0);

        /// <summary>
        /// async/awaitを伴う初期化
        /// </summary>
        private async void initialize() {
            // マップ情報を取得
            var list = await model.GetMapList();
            if (list.Count == 0) {
                MessageBox.Show("マップ情報をダウンロードできませんでした。", "AWSK", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // マップ情報をリストに登録する
            foreach (string mapName in list) {
                MapList.Add(mapName);
            }
        }

        /// <summary>
        /// マス情報をダウンロードし、リストに登録する
        /// </summary>
        private async Task RefreshPointList() {
            // 入力バリデーション
            if (MapSelectIndex.Value < 0 || MapList.Count <= MapSelectIndex.Value || LevelSelectIndex.Value < 0 || LevelList.Count <= LevelSelectIndex.Value) {
                return;
            }

            // ダウンロード開始
            PointList.Clear();
            var list = await model.GetPointList(MapList[MapSelectIndex.Value], LevelList[LevelSelectIndex.Value]);
            if (list.Count == 0) {
                MessageBox.Show("マス情報をダウンロードできませんでした。", "AWSK", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (string pointName in list) {
                PointList.Add(pointName);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PresetLoaderViewModel() {
            initialize();

            // イベントを登録する
            MapSelectIndex.Subscribe(async value => {
                // マス情報をダウンロードし、リストに登録する
                await RefreshPointList();
            });
            LevelSelectIndex.Subscribe(async value => {
                // マス情報をダウンロードし、リストに登録する
                await RefreshPointList();
            });
        }
    }
}
