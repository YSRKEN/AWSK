using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Linq;

namespace AWSK.ViewModels
{
	class ResultViewModel
	{
		public ReactiveProperty<PlotModel> LastAAVGraphModel { get; } = new ReactiveProperty<PlotModel>();
		public ReactiveProperty<PlotModel> AwsCountGraphModel { get; } = new ReactiveProperty<PlotModel>();

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

		// コンストラクタ
		public ResultViewModel() { }
		public ResultViewModel(Dictionary<int, int> finalAAV, List<List<List<int>>> awsCount) {
			LastAAVGraphModel.Value = CreateLastAAVGraphModel(finalAAV);
			AwsCountGraphModel.Value = CreateAwsCountGraphModel(awsCount);
		}
	}
}
