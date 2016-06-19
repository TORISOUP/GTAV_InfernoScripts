using Inferno.ChaosMode.WeaponProvider;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Inferno.Utilities;
using Newtonsoft.Json;

namespace Inferno.ChaosMode
{
    /// <summary>
    /// カオスモード用設定ファイルのローダー
    /// </summary>
    public class ChaosModeSettingLoader : InfernoConfigLoader<ChaosModeSettingDTO>
    {
        /// <summary>
        /// ファイルから読み込んで設定ファイルを生成する
        /// </summary>
        /// <param name="fileName">設定ファイルパス</param>
        /// <returns>設定ファイル</returns>
        public new ChaosModeSetting LoadSettingFile(string fileName)
        {
            return new ChaosModeSetting(base.LoadSettingFile(fileName));
        }

        protected override ChaosModeSettingDTO CreateDefault()
        {
            //デフォルト設定を吐き出す
            var dto = new ChaosModeSettingDTO();
            var chaosModeWeapons = new ChaosModeWeapons();
            dto.WeaponList = chaosModeWeapons.ExcludeClosedWeapons.Select(x => x.ToString()).ToArray();
            dto.WeaponListForDriveBy = chaosModeWeapons.DriveByWeapons.Select(x => x.ToString()).ToArray();
            return dto;
        }
    }
}
