using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Inferno.InfernoScripts
{
    /// <summary>
    /// ParupunteConfigElementの管理
    /// </summary>
    internal class ParupunteConfigRepository
    {
        protected readonly Encoding _encoding = Encoding.UTF8;
        protected DebugLogger _debugLogger;
        protected string _filePath = @"./scripts/confs/Parupunte.conf";

        protected virtual DebugLogger DebugLogger
        {
            get
            {
                if (_debugLogger != null)
                {
                    return _debugLogger;
                }

                _debugLogger = new DebugLogger(@"Inferno.log");
                return _debugLogger;
            }
        }

        /// <summary>
        /// ファイルから読み込んで設定ファイル返す
        /// </summary>
        public Dictionary<string, ParupunteConfigElement> LoadSettingFile()
        {
            if (!File.Exists(_filePath))
            {
                return new Dictionary<string, ParupunteConfigElement>();
            }

            var readString = "";
            try
            {
                using (var sr = new StreamReader(_filePath, _encoding))
                {
                    readString = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
                return new Dictionary<string, ParupunteConfigElement>();
            }

            try
            {
                var dto = JsonConvert.DeserializeObject<Dictionary<string, ParupunteConfigDto>>(readString);
                return dto.Select(x => new { x.Key, Value = x.Value.ToDomain() })
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception e)
            {
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
                return new Dictionary<string, ParupunteConfigElement>();
            }
        }

        public async ValueTask SaveSettings(Dictionary<string, ParupunteConfigElement> configs)
        {
            var directoryPath = Path.GetDirectoryName(_filePath);
            //存在しないならディレクトリを作る
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var dto = configs.ToDictionary(x => x.Key, x => x.Value.ToDto());

            try
            {
                using (var w = new StreamWriter(_filePath, false, _encoding))
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    var json = JsonConvert.SerializeObject(dto, Formatting.Indented, settings);
                    await w.WriteAsync(json);
                }
            }
            catch (Exception e)
            {
                DebugLogger.Log(e.Message);
                DebugLogger.Log(e.StackTrace);
            }
        }
    }
}