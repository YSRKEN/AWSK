using AWSK.ViewModel;
using System.Windows;

namespace AWSK.View {
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : Window {
        public MainView() {
            InitializeComponent();
            var swVM = this.test.DataContext as SelectWeaponViewModel;
            var mVM = this.DataContext as MainViewModel;
            swVM.Weapon.Value = mVM.SampleWeapon.Value;
        }
    }
}
