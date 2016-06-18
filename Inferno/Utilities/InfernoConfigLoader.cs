using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Inferno.Utilities
{
    /// <summary>
    /// インフェルノ用設定ファイルのローダー
    /// </summary>
    public class InfernoConfigLoader<T> where T : new()
    {
        protected readonly Encoding _encoding = Encoding.UTF8;
        protected DebugLogger _debugLogger;
        protected string _baseFilePath = "@./scripts/confs/";

        protected virtual DebugLogger DebugLogger
        {
            get
            {
                if (_debugLogger != null) return _debugLogger;
                _debugLogger = new DebugLogger(@"Inferno.log");
                return _debugLogger;
            }
        }

        /// <summary>
        /// ファイルから読み込んで設定ファイルを生成する
        /// </summary>
        /// <param name="filePath">設定ファイルパス</param>
        /// <returns>設定ファイル</returns>
        public T LoadSettingFile(string fileName)
        {
            var filePath = _baseFilePath + fileName;
            //ファイルロード
            var readJson = ReadFile(filePath);
            try
            {
                return JsonConvert.DeserializeObject<T>(readJson);
            }
            catch (Exception e)
            {
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
                //例外発生時はデフォルトの設定ファイルを返す
                return new T();
            }
        }

        /// <summary>
        /// ファイルから中身のstringを読み取る
        /// </summary>
        /// <param name="filePath">ファイル名</param>
        /// <returns>結果</returns>
        protected virtual string ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //存在しないならデフォルト設定ファイルを生成する
                CreateDefaultSettingFile(filePath);
                return "";
            }
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
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
            }
            return readString;
        }

        /// <summary>
        /// デフォルトの設定ファイルを生成する
        /// </summary>
        protected void CreateDefaultSettingFile(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            //存在しないならディレクトリを作る
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //デフォルト設定を吐き出す
            var dto = CreateDefault();

            try
            {
                using (var w = new StreamWriter(filePath, false, _encoding))
                {
                    var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
                    w.WriteAsync(json);
                    w.Flush();
                }
            }
            catch (Exception e)
            {
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
            }
        }

        protected virtual T CreateDefault()
        {
            return new T();
        }
    }
}
