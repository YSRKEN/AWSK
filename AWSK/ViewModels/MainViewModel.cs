using Reactive.Bindings;
using System;
using System.Windows;

namespace AWSK.ViewModels
{
	class MainViewModel
	{
		#region コマンド
		// クリップボードからインポート
		public ReactiveCommand ImportClipboardTextCommand { get; } = new ReactiveCommand();
		#endregion

		// コンストラクタ
		public MainViewModel() {
			ImportClipboardTextCommand.Subscribe(_ => MessageBox.Show("スタブ", "AWSK"));
		}
	}
}
