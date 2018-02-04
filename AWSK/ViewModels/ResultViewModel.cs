using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows;

namespace AWSK.ViewModels
{
	class ResultViewModel
	{
		// 制空値情報および制空状況情報
		private Dictionary<int, int> finalAAV;
		private List<List<List<int>>> awsCount;
		// タイトルバー
		public ReactiveProperty<string> TitleStr { get; } = new ReactiveProperty<string>("計算結果");
		// 制空値情報のグラフおよび制空状況情報のグラフ
		public ReactiveProperty<PlotModel> LastAAVGraphModel { get; } = new ReactiveProperty<PlotModel>();
		public ReactiveProperty<PlotModel> AwsCountGraphModel { get; } = new ReactiveProperty<PlotModel>();
		// 各種コマンド
		public ReactiveCommand CopyAAVPictureCommand { get; } = new ReactiveCommand();
		public ReactiveCommand CopyAAVTextCommand { get; } = new ReactiveCommand();
		public ReactiveCommand CopyAwsPictureCommand { get; } = new ReactiveCommand();
		public ReactiveCommand CopyAwsTextCommand { get; } = new ReactiveCommand();

		// グラフモデルを作成する
		private PlotModel CreateLastAAVGraphModel(Dictionary<int, int> finalAAV) {
			var graphModel = new PlotModel();
			// X軸・Y軸を追加する
			graphModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "制空値" });
			graphModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "割合(％)" });
			// グラフ要素を追加する
			int allSum = finalAAV.Values.Sum();
			int sum = 0;
			var lineSeries = new LineSeries();
			foreach (var data in finalAAV) {
				sum += data.Value;
				lineSeries.Points.Add(new DataPoint(data.Key, 100.0 * sum / allSum));
			}
			lineSeries.Title = "下側確率";
			graphModel.Series.Add(lineSeries);
			graphModel.InvalidatePlot(true);
			return graphModel;
		}
		private PlotModel CreateAwsCountGraphModel(List<List<List<int>>> awsCount) {
			var graphModel = new PlotModel();
			graphModel.IsLegendVisible = false;
			// 横軸・縦軸を追加する
			graphModel.Axes.Add(new LinearAxis {
				Position = AxisPosition.Left, Title = "割合(％)", Minimum = 0, Maximum = 100});
			{
				var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom };
				var labelList = new List<string>();
				for (int si = 0; si < awsCount.Count; ++si) {
					for (int ci = 0; ci < awsCount[si].Count; ++ci) {
						labelList.Add($"{si + 1}-{ci + 1}");
					}
				}
				categoryAxis.ItemsSource = labelList;
				graphModel.Axes.Add(categoryAxis);
			}
			// グラフ要素を追加する
			// 各制空状態(k)毎に入力することに注意
			var columnLabel = new[] { "制空権確保", "航空優勢", "制空均衡", "航空劣勢", "制空権喪失" };
			for (int k = 0; k < 5; ++k) {
				var columnSeries = new ColumnSeries();
				columnSeries.IsStacked = true;
				columnSeries.Title = $"{columnLabel[k]}";
				for (int si = 0; si < awsCount.Count; ++si) {
					for (int ci = 0; ci < awsCount[si].Count; ++ci) {
						int all_sum = awsCount[si][ci].Sum();
						// 各制空状態毎に入力する
						columnSeries.Items.Add(new ColumnItem(100.0 * awsCount[si][ci][k] / all_sum));
					}
				}
				graphModel.Series.Add(columnSeries);
			}
			graphModel.InvalidatePlot(true);
			return graphModel;
		}
		// タイトルバーを設定
		private string SetTitleBar(Dictionary<int, int> finalAAV) {
			// 下側確率のデータを取得する
			int allSum = finalAAV.Values.Sum();
			int sum = 0;
			var prob = new List<KeyValuePair<int, double>>();
			foreach (var data in finalAAV) {
				sum += data.Value;
				prob.Add(new KeyValuePair<int, double>(data.Key, 100.0 * sum / allSum));
			}
			// 各パーセンテージを初めて超える際の制空値を取得し、文字列として返す
			var prob2 = new double[] { 50.0, 70.0, 90.0, 95.0, 99.0 };
			string output = "計算結果(";
			int index = 0;
			foreach(double prob2_ in prob2) {
				if (index != 0)
					output += "　";
				output += $"{prob2_}％：{prob.Find(p => p.Value >= prob2_).Key}";
				++index;
			}
			output += ")";
			return output;
		}

		// コンストラクタ
		public ResultViewModel() { }
		public ResultViewModel(Dictionary<int, int> finalAAV, List<List<List<int>>> awsCount) {
			// グラフを描画
			LastAAVGraphModel.Value = CreateLastAAVGraphModel(finalAAV);
			AwsCountGraphModel.Value = CreateAwsCountGraphModel(awsCount);
			// データをバックアップ
			this.finalAAV = finalAAV;
			this.awsCount = awsCount;
			// タイトルバーを設定
			TitleStr.Value = SetTitleBar(finalAAV);
			// コマンドを設定
			CopyAAVPictureCommand.Subscribe(_ => {
				// 制空値のグラフを画像としてクリップボードにコピー
				var pngExporter = new OxyPlot.Wpf.PngExporter();
				var bitmapSource = pngExporter.ExportToBitmap(LastAAVGraphModel.Value);
				Clipboard.SetImage(bitmapSource);
			});
			CopyAAVTextCommand.Subscribe(_ => {
				// 制空値の下側確率の情報をテキストとしてクリップボードにコピー
				int loopCount = finalAAV.Values.Sum();
				int sum1 = loopCount, sum2 = 0;
				var temp = new List<List<int>>();
				foreach (var pair in finalAAV.OrderBy((x) => x.Key)) {
					sum2 += pair.Value;
					temp.Add(new List<int> { pair.Key, pair.Value, sum1, sum2 });
					sum1 -= pair.Value;
				}
				string output = "制空値,確率分布(%),上側確率(%),下側確率(%)\n";
				foreach (var record in temp) {
					output += $"{record[0]},{100.0 * record[1] / loopCount},{100.0 * record[2] / loopCount},{100.0 * record[3] / loopCount}\n";
				}
				Clipboard.SetText(output);
			});
			CopyAwsPictureCommand.Subscribe(_ => {
				// 制空状況のグラフを画像としてクリップボードにコピー
				var pngExporter = new OxyPlot.Wpf.PngExporter();
				var bitmapSource = pngExporter.ExportToBitmap(AwsCountGraphModel.Value);
				Clipboard.SetImage(bitmapSource);
			});
			CopyAwsTextCommand.Subscribe(_ => {
				// 制空状況の詳細をテキストとしてクリップボードにコピー
				int loopCount = finalAAV.Values.Sum();
				string output = "航空隊,回数,確保(%),優勢(%),均衡(%),劣勢(%),喪失(%)\n";
				for (int si = 0; si < awsCount.Count; ++si) {
					for (int ci = 0; ci < awsCount[si].Count; ++ci) {
						output += $"{si + 1},{ci + 1},";
						output += $"{100.0 * awsCount[si][ci][0] / loopCount},";
						output += $"{100.0 * awsCount[si][ci][1] / loopCount},";
						output += $"{100.0 * awsCount[si][ci][2] / loopCount},";
						output += $"{100.0 * awsCount[si][ci][3] / loopCount},";
						output += $"{100.0 * awsCount[si][ci][4] / loopCount}\n";
					}
				}
				Clipboard.SetText(output);
			});
		}
	}
}
