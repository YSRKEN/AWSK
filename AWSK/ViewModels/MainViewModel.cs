using AWSK.Stores;
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


		public static async void DownloadData() {
			if(await DataStore.DownloadDataAsync()) {
				MessageBox.Show("ダウンロードに成功しました。", "AWSK");
			} else {
				MessageBox.Show("ダウンロードに失敗しました。", "AWSK");
			}
		}

		// コンストラクタ
		public MainViewModel() {
			DataStore.Initialize();
			DownloadData();
			ImportClipboardTextCommand.Subscribe(_ => MessageBox.Show("スタブ", "AWSK"));
		}
	}
}
