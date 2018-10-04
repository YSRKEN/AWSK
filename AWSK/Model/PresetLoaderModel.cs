using AWSK.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Model
{
    class PresetLoaderModel
    {
        /// <summary>
        /// 出撃するマップ一覧と、それに対応するURLの対応表
        /// </summary>
        private Dictionary<string, string> mapDic = new Dictionary<string, string>();

        /// <summary>
        /// ダウンロードサービス
        /// </summary>
        private DownloadService download = DownloadService.instance;

        /// <summary>
        /// 出撃マップ一覧を算出して返す
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetMapList() {
            mapDic = await download.downloadMapList();
            return mapDic.Keys.ToList();
        }
    }
}
