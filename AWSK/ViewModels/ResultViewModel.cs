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
		private readonly Dictionary<int, double> finalAAV;
		private readonly List<List<List<int>>> awsCount;
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
		private PlotModel CreateLastAAVGraphModel(Dictionary<int, double> finalAAV) {
			var graphModel = new PlotModel();
			// X軸・Y軸(第一・第二)を追加する
			graphModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "制空値" });
			graphModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left,   Title = "割合(％)", Key = "Primary"   });
			graphModel.Axes.Add(new LinearAxis { Position = AxisPosition.Right,  Title = "割合(％)", Key = "Secondary" });
			// グラフ要素を追加する(第一Y軸)
			double sum = 0.0;
			var lineSeries1 = new LineSeries {
				Title = "下側確率",
				YAxisKey = "Primary"
			};
			foreach (var data in finalAAV) {
				sum += data.Value;
				lineSeries1.Points.Add(new DataPoint(data.Key, 100.0 * sum));
			}
			graphModel.Series.Add(lineSeries1);
			// グラフ要素を追加する(第二Y軸)
			var lineSeries2 = new LineSeries {
				Title = "確率分布",
				YAxisKey = "Secondary"
			};
			foreach (var data in finalAAV) {
				lineSeries2.Points.Add(new DataPoint(data.Key, 100.0 * data.Value));
			}
			graphModel.Series.Add(lineSeries2);
			//
			graphModel.InvalidatePlot(true);
			return graphModel;
		}
		private PlotModel CreateAwsCountGraphModel(List<List<List<int>>> awsCount) {
            var graphModel = new PlotModel {
                IsLegendVisible = false
            };
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
			string[] columnLabel = new[] { "制空権確保", "航空優勢", "制空均衡", "航空劣勢", "制空権喪失" };
			for (int k = 0; k < 5; ++k) {
                var columnSeries = new ColumnSeries {
                    IsStacked = true,
                    Title = $"{columnLabel[k]}"
                };
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
		private string SetTitleBar(Dictionary<int, double> finalAAV) {
			// 下側確率のデータを取得する
			double sum = 0.0;
			var prob = new List<KeyValuePair<int, double>>();
			foreach (var data in finalAAV) {
				sum += data.Value;
				prob.Add(new KeyValuePair<int, double>(data.Key, 100.0 * sum));
			}
            // 各パーセンテージを初めて超える際の制空値を取得し、文字列として返す
            double[] prob2 = new double[] { 50.0, 70.0, 90.0, 95.0, 99.0 };
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
		public ResultViewModel(Dictionary<int, double> finalAAV, List<List<List<int>>> awsCount) {
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
				double sum1 = 1.0, sum2 = 0.0;
				var temp = new Dictionary<int, List<double>>();
				foreach (var pair in finalAAV.OrderBy((x) => x.Key)) {
					sum2 += pair.Value;
					temp[pair.Key] = new List<double> {pair.Value, sum1, sum2 };
					sum1 -= pair.Value;
				}
				string output = "制空値,確率分布(%),上側確率(%),下側確率(%)\n";
				foreach (var record in temp) {
					output += $"{record.Key},{100.0 * record.Value[0]},{100.0 * record.Value[1]},{100.0 * record.Value[2]}\n";
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
				string output = "航空隊,回数,確保(%),優勢(%),均衡(%),劣勢(%),喪失(%)\n";
				int loopCount = awsCount[0][0].Sum();
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
