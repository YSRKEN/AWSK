using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Model
{
    /// <summary>
    /// 敵編成検索画面のViewModel
    /// </summary>
    class PresetLoaderViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private PresetLoaderModel model = new PresetLoaderModel();
    }
}
