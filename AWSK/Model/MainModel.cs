using AWSK.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Model
{
    class MainModel
    {
        /// <summary>
        /// 右クリックから敵編成検索画面を開く
        /// </summary>
        public ReactiveCommand OpenPresetLoaderCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 敵艦隊を設定するDelegate
        /// </summary>
        private SetEnemyFleetDelegate setEnemyFleetDelegate;

        /// <summary>
        /// 敵編成検索画面を開く
        /// </summary>
        private void OpenPresetLoader() {
            var view = new View.PresetLoaderView { DataContext = new PresetLoaderViewModel(setEnemyFleetDelegate) };
            view.Show();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainModel(SetEnemyFleetDelegate setEnemyFleetDelegate) {
            this.setEnemyFleetDelegate = setEnemyFleetDelegate;
            OpenPresetLoaderCommand.Subscribe(OpenPresetLoader);
        }
    }
}
