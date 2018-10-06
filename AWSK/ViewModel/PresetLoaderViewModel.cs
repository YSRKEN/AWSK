using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static AWSK.Constant;

namespace AWSK.Model {
    /// <summary>
    /// 敵編成検索画面のViewModel
    /// </summary>
    class PresetLoaderViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PresetLoaderModel model = new PresetLoaderModel();

        public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("敵編成検索画面");

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

        public ReactiveProperty<string> MapImageUrl { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// 敵編成の内容
        /// </summary>
        public ReactiveProperty<string> EnemyInfo { get; } = new ReactiveProperty<string>("");

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
        /// 敵編成の表示を更新する
        /// </summary>
        /// <returns></returns>
        private void RefreshEnemyInfo() {
            if (PointSelectIndex.Value < 0 || PointList.Count <= PointSelectIndex.Value) {
                return;
            }
            EnemyInfo.Value = model.GetEnemyInfo(PointList[PointSelectIndex.Value]);
        }

        /// <summary>
        /// マップ画像のURLを返す
        /// </summary>
        /// <returns>マップ画像のURL(エラー時は空白文字列)</returns>
        private async Task<string> GetMapImageUrl() {
            // 入力バリデーション
            if (MapSelectIndex.Value < 0 || MapList.Count <= MapSelectIndex.Value) {
                return "";
            }

            // ダウンロード開始
            return await model.GetMapImageUrl(MapList[MapSelectIndex.Value]);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PresetLoaderViewModel() {
            Title.Value = "読み込み中...";
            initialize();
            Title.Value = "敵編成検索画面";

            // イベントを登録する
            MapSelectIndex.Subscribe(async value => {
                // マス情報をダウンロードし、リストに登録する
                Title.Value = "読み込み中...";
                await RefreshPointList();
                MapImageUrl.Value = await GetMapImageUrl();
                Title.Value = "敵編成検索画面";
                RefreshEnemyInfo();
            });
            LevelSelectIndex.Subscribe(async value => {
                // マス情報をダウンロードし、リストに登録する
                Title.Value = "読み込み中...";
                await RefreshPointList();
                Title.Value = "敵編成検索画面";
                RefreshEnemyInfo();
            });
            PointSelectIndex.Subscribe(value => {
                RefreshEnemyInfo();
            });
        }
    }
}
