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
		public ReactiveProperty<PlotModel> GraphModel { get; } = new ReactiveProperty<PlotModel>();

		// グラフモデルを作成する
		private PlotModel CreateGraphModel(Dictionary<int, int> finalAAV, List<List<List<int>>> awsCount) {
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
			return graphModel;
		}

		// コンストラクタ
		public ResultViewModel() { }
		public ResultViewModel(Dictionary<int, int> finalAAV, List<List<List<int>>> awsCount) {
			GraphModel.Value = CreateGraphModel(finalAAV, awsCount);
		}
	}
}
