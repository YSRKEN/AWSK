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
using System.Windows.Shapes;
using static AWSK.Constant;

namespace AWSK.View {
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : Window {
        public MainView() {
            InitializeComponent();
            var swVM = this.test.DataContext as SelectWeaponViewModel;
            var mVM = this.DataContext as MainViewModel;
            swVM.Category.Value = WeaponTypeDicShort[mVM.SampleWeapon.Value.Type];
            swVM.Name.Value = mVM.SampleWeapon.Value.Name;
            swVM.MasterLevel.Value = mVM.SampleWeapon.Value.Mas;
            swVM.RefurbishmentLevel.Value = mVM.SampleWeapon.Value.Rf;
            swVM.Category.Subscribe(value => mVM.SampleWeapon.Value.Type = WeaponTypeReverseDicShort[value]);
            swVM.Name.Subscribe(value => mVM.SampleWeapon.Value.Name = value);
            swVM.MasterLevel.Subscribe(value => mVM.SampleWeapon.Value.Mas = value);
            swVM.RefurbishmentLevel.Subscribe(value => mVM.SampleWeapon.Value.Rf = value);
        }
    }
}
