using AWSK.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Model
{
    class PresetLoaderModel {
        /// <summary>
        /// 出撃するマップ一覧と、それに対応するURLの対応表
        /// </summary>
        private Dictionary<string, string> mapDic = null;

        private Dictionary<string, Fleet> pointDic = null;

        /// <summary>
        /// ダウンロードサービス
        /// </summary>
        private DownloadService download = DownloadService.instance;

        /// <summary>
        /// 出撃マップ一覧を算出して返す
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetMapList() {
            try {
                if (mapDic == null) {
                    mapDic = await download.downloadMapList();
                }
                return mapDic.Keys.ToList();
            } catch (Exception e) {
                Console.WriteLine(e);
                return new List<string>();
            }
        }

        public async Task<List<string>> GetPointList(string mapName, string levelName) {
            // マップ情報が取れていないか、渡された入力がおかしい場合に弾く
            if (mapDic == null || !mapDic.ContainsKey(mapName)) {
                return new List<string>();
            }

            // マス一覧を返す
            try {
                pointDic = await download.downloadPointList(mapDic[mapName], levelName);
                return pointDic.Keys.ToList();
            } catch (Exception e) {
                Console.WriteLine(e);
                return new List<string>();
            }
        }
    }
}
