using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Inferno.Utilities
{
    /// <summary>
    /// インフェルノ用設定ファイルのReadWriter
    /// </summary>
    public class InfernoConfigReadWriter<T> where T : new()
    {
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly string _baseFilePath = @"./scripts/inferno_configs/";
        protected virtual IDebugLogger DebugLogger => Inferno.DebugLogger.Instance;
        

        /// <summary>
        /// ファイルから読み込んで設定ファイルを生成する
        /// </summary>
        /// <param name="filePath">設定ファイルパス</param>
        /// <returns>設定ファイル</returns>
        public T LoadSettingFile(string fileName)
        {
            var filePath = _baseFilePath + fileName;
            var readJson = "";

            //ファイルロード
            readJson = ReadFile(filePath);

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
        
        public void SaveSettingFile(string fileName, T setting)
        {
            var filePath = _baseFilePath + fileName;
            try
            {
                using var w = new StreamWriter(filePath, false, _encoding);
                var json = JsonConvert.SerializeObject(setting, Formatting.Indented);
                w.Write(json);
                w.Flush();
            }
            catch (Exception e)
            {
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
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
                using var sr = new StreamReader(filePath, _encoding);
                readString = sr.ReadToEnd();
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
                using var w = new StreamWriter(filePath, false, _encoding);
                var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
                w.WriteAsync(json);
                w.Flush();
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