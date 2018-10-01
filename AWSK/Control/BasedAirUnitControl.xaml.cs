using AWSK.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AWSK.Control {
    /// <summary>
    /// BasedAirUnitControl.xaml の相互作用ロジック
    /// </summary>
    public partial class BasedAirUnitControl : UserControl {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasedAirUnitControl() {
            InitializeComponent();
            BasedAirUnitUnitText = "第1航空隊";
        }

        #region 部隊名
        /// <summary>
        /// 部隊名を表す依存プロパティ
        /// </summary>
        public static readonly DependencyProperty BasedAirUnitUnitTextProperty =
            DependencyProperty.Register("BasedAirUnitUnitText", typeof(string), typeof(BasedAirUnitControl),
            new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnBasedAirUnitUnitTextChanged)));

        /// <summary>
        /// 部隊名を参照するためのプロパティ
        /// </summary>
        public string BasedAirUnitUnitText {
            get => (string)GetValue(BasedAirUnitUnitTextProperty);
            set => SetValue(BasedAirUnitUnitTextProperty, value);
        }

        /// <summary>
        /// 部隊名の変更時に呼ばれるコールバック関数
        /// </summary>
        /// <param name="obj">DependencyObject</param>
        /// <param name="e">DependencyPropertyChangedEventArgs</param>
        private static void OnBasedAirUnitUnitTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            // オブジェクトを取得して処理する
            var control = obj as BasedAirUnitControl;
            if (control != null) {
                (control.DataContext as BasedAirUnitViewModel).BasedAirUnitUnitText.Value = control.BasedAirUnitUnitText;
            }
        }
        #endregion
    }
}
