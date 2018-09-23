using AWSK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSK.Service {
    /// <summary>
    /// データベースを操作するクラス
    /// </summary>
    class DataBaseService {
        /// <summary>
        /// 自分自身の唯一のinstance
        /// </summary>
        static DataBaseService singleton = null;

        /// <summary>
        /// 接続用文字列
        /// </summary>
        const string CONNECTION_STRING = @"Data Source=GameData2.db";

        /// <summary>
        /// privateコンストラクタ
        /// </summary>
        DataBaseService() { }

        /// <summary>
        /// 唯一のinstanceを返す(Singletonパターン)
        /// </summary>
        /// <returns></returns>
        public static DataBaseService instance {
            get {
                if (singleton == null) {
                    singleton = new DataBaseService();
                }
                return singleton;
            }
        }

        /// <summary>
        /// 装備情報を挿入・上書きする
        /// </summary>
        /// <param name="weapon">装備情報</param>
        public void Save(Weapon weapon) {
            // スタブ
            return;
        }

        /// <summary>
        /// 艦娘情報を挿入・上書きする
        /// </summary>
        /// <param name="kammusu">艦娘情報</param>
        public void Save(Kammusu kammusu) {
            // スタブ
            return;
        }

        /// <summary>
        /// 装備情報を装備IDから検索して返す
        /// </summary>
        /// <param name="id">装備ID</param>
        /// <returns>装備情報。未ヒットの場合はnull</returns>
        public Weapon findByWeaponId(int id) {
            // スタブ
            return null;
        }

        /// <summary>
        /// 艦娘情報を艦船IDから検索して返す
        /// </summary>
        /// <param name="id">艦船ID</param>
        /// <returns>艦娘情報。未ヒットの場合はnull</returns>
        public Kammusu findByKammusuId(int id) {
            // スタブ
            return null;
        }
    }
}
