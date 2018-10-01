using AWSK.Service;
using AWSK.ViewModel;
using System.Windows;
using System.Windows.Controls;
using static AWSK.Constant;

namespace AWSK.Control {
    /// <summary>
    /// SelectWeaponBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectWeaponBox : UserControl {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectWeaponBox() {
            InitializeComponent();
            WeaponName = "";
            MasterLevel = "0";
            //RefurbishmentLevel = 0;
        }

        #region 装備名
        /// <summary>
        /// 装備名を表す依存プロパティ
        /// </summary>
        public static readonly DependencyProperty WeaponNameProperty =
            DependencyProperty.Register("WeaponName", typeof(string), typeof(SelectWeaponBox),
            new FrameworkPropertyMetadata("WeaponName", new PropertyChangedCallback(OnWeaponNameChanged)));

        /// <summary>
        /// 装備名を参照するためのプロパティ
        /// </summary>
        public string WeaponName {
            get => (string)GetValue(WeaponNameProperty);
            set => SetValue(WeaponNameProperty, value);
        }

        /// <summary>
        /// 装備名の変更時に呼ばれるコールバック関数
        /// </summary>
        /// <param name="obj">DependencyObject</param>
        /// <param name="e">DependencyPropertyChangedEventArgs</param>
        private static void OnWeaponNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            // オブジェクトを取得して処理する
            var control = obj as SelectWeaponBox;
            if (control != null) {
                var database = DataBaseService.Instance;
                var weapon = database.FindByWeaponName(control.WeaponName);
                if (weapon != null) {
                    (control.DataContext as SelectWeaponViewModel).Category.Value = WeaponTypeDicShort[weapon.Type];
                    (control.DataContext as SelectWeaponViewModel).Name.Value = control.WeaponName;
                }
            }
        }
        #endregion

        #region 熟練度
        /// <summary>
        /// 熟練度を表す依存プロパティ
        /// </summary>
        public static readonly DependencyProperty MasterLevelProperty =
            DependencyProperty.Register("MasterLevel", typeof(string), typeof(SelectWeaponBox),
            new FrameworkPropertyMetadata("MasterLevel", new PropertyChangedCallback(MasterChanged)));

        /// <summary>
        /// 熟練度を参照するためのプロパティ
        /// </summary>
        public string MasterLevel {
            get => (string)GetValue(MasterLevelProperty);
            set => SetValue(MasterLevelProperty, value);
        }

        /// <summary>
        /// 熟練度の変更時に呼ばれるコールバック関数
        /// </summary>
        /// <param name="obj">DependencyObject</param>
        /// <param name="e">DependencyPropertyChangedEventArgs</param>
        private static void MasterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            // オブジェクトを取得して処理する
            var control = obj as SelectWeaponBox;
            if (control != null) {
                (control.DataContext as SelectWeaponViewModel).MasterLevel.Value = int.Parse(control.MasterLevel);
            }
        }
        #endregion

        #region 改修度
        /// <summary>
        /// 改修度を表す依存プロパティ
        /// </summary>
        public static readonly DependencyProperty RefurbishmentLevelProperty =
            DependencyProperty.Register("RefurbishmentLevel", typeof(string), typeof(SelectWeaponBox),
            new FrameworkPropertyMetadata("RefurbishmentLevel", new PropertyChangedCallback(RefurbishmentLevelChanged)));

        /// <summary>
        /// 改修度を参照するためのプロパティ
        /// </summary>
        public string RefurbishmentLevel {
            get => (string)GetValue(RefurbishmentLevelProperty);
            set => SetValue(RefurbishmentLevelProperty, value);
        }

        /// <summary>
        /// 改修度の変更時に呼ばれるコールバック関数
        /// </summary>
        /// <param name="obj">DependencyObject</param>
        /// <param name="e">DependencyPropertyChangedEventArgs</param>
        private static void RefurbishmentLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            // オブジェクトを取得して処理する
            var control = obj as SelectWeaponBox;
            if (control != null) {
                (control.DataContext as SelectWeaponViewModel).RefurbishmentLevel.Value = int.Parse(control.RefurbishmentLevel);
            }
        }
        #endregion
    }
}
