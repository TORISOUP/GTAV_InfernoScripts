using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Inferno.InfernoScripts.Conf;
using Inferno.Utilities;

namespace Inferno.InfernoScripts
{
    public class InfernoAllOnProvider : IDisposable
    {
        public static InfernoAllOnProvider Instance = new();
        private readonly Lazy<InfernoAllOnConfig> _lazyConfig;
        private readonly Subject<Unit> _saveEventSubject = new();

        private InfernoAllOnProvider()
        {
            Instance = this;

            var commandConfigReadWriter = new InfernoConfigReadWriter<InfernoAllOnConfig>();

            _lazyConfig = new Lazy<InfernoAllOnConfig>(
                () =>
                {
                    var conf = commandConfigReadWriter.LoadSettingFile("InfernoMod_AllOn.conf");
                    if (conf == null || conf.AllOn == null)
                    {
                        var newConf = new InfernoAllOnConfig()
                        {
                            AllOn = new Dictionary<string, bool>()
                        };
                        commandConfigReadWriter.SaveSettingFile("InfernoMod_AllOn.conf", newConf);
                        return newConf;
                    }

                    return conf;
                });

            _saveEventSubject
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    if (_lazyConfig.IsValueCreated)
                    {
                        commandConfigReadWriter.SaveSettingFile("InfernoMod_AllOn.conf", _lazyConfig.Value);
                    }
                });
        }

        public bool GetOrCreateAllOnEnable(string name, bool defaultValue)
        {
            var conf = _lazyConfig.Value;
            if (conf.AllOn == null)
            {
                conf.AllOn = new Dictionary<string, bool>();
            }


            if (conf.AllOn.TryGetValue(name, out var e))
            {
                return e;
            }

            _saveEventSubject.OnNext(Unit.Default);
            conf.AllOn.Add(name, defaultValue);
            return defaultValue;
        }

        public void Dispose()
        {
            _saveEventSubject?.Dispose();
        }
    }
}