using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Inferno.ChaosMode
{
    /// <summary>
    /// カオスモード用設定ファイルのローダー
    /// </summary>
    public class ChaosModeSettingLoader
    {
        readonly Encoding _encoding = Encoding.GetEncoding("Shift_JIS");
        private readonly DebugLogger _debugLogger;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChaosModeSettingLoader()
        {
            _debugLogger = new DebugLogger(@"ChaosMod.log");
        }

    
        /// <summary>
        /// ファイルから読み込んで設定ファイルを生成する
        /// </summary>
        /// <param name="filePath">設定ファイルパス</param>
        /// <returns>設定ファイル</returns>
        public ChaosModeSetting LoadSettingFile(string filePath)
        {
            //ファイルロード
            var readJson = ReadFile(filePath);

            var jss = new JavaScriptSerializer();
            try
            {
                var dto = jss.Deserialize<ChaosModeSettingDTO>(readJson);
                return new ChaosModeSetting(dto);
            }
            catch (Exception e)
            {
                _debugLogger.Log(e.Message);
                _debugLogger.Log(e.StackTrace);
                //例外発生時はデフォルトの設定ファイルを返す
                return new ChaosModeSetting(new ChaosModeSettingDTO());
            }

        }
        

        /// <summary>
        /// ファイルから中身のstringを読み取る
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>結果</returns>
        protected virtual string ReadFile(string filePath)
        {
            if (!File.Exists(filePath)) return "";
            var readString = "";
            try
            {
                using (var sr = new StreamReader(filePath, _encoding))
                {
                    readString = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _debugLogger.Log(e.Message);
                _debugLogger.Log(e.StackTrace);
            }
            return readString;
        }
        
    }
}
