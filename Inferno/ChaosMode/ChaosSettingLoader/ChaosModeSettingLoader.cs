using Inferno.ChaosMode.WeaponProvider;
using System.Linq;
using Inferno.Utilities;

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
            dto.WeaponList = ChaosModeWeapons.AllWeapons.Select(x => x.ToString()).ToArray();
            dto.WeaponListForDriveBy = ChaosModeWeapons.DriveByWeapons.Select(x => x.ToString()).ToArray();
            return dto;
        }
    }
}
