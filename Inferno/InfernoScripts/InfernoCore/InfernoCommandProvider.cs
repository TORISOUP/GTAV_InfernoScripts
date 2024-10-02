using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Inferno.InfernoScripts.Conf;
using Inferno.Utilities;

namespace Inferno.InfernoScripts
{
    public class InfernoCommandProvider : IDisposable
    {
        public static InfernoCommandProvider Instance = new();
        private readonly Lazy<InfernoCommandConfig> _lazyConfig;
        private readonly Subject<Unit> _saveEventSubject = new();

        private InfernoCommandProvider()
        {
            Instance = this;

            var commandConfigReadWriter = new InfernoConfigReadWriter<InfernoCommandConfig>();

            _lazyConfig = new Lazy<InfernoCommandConfig>(
                () =>
                {
                    var conf = commandConfigReadWriter.LoadSettingFile("InfernoMod_Command.conf");
                    if (conf == null || conf.CommandList == null)
                    {
                        var newConf = new InfernoCommandConfig()
                        {
                            CommandList = new Dictionary<string, string>()
                        };
                        commandConfigReadWriter.SaveSettingFile("InfernoMod_Command.conf", newConf);
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
                        commandConfigReadWriter.SaveSettingFile("InfernoMod_Command.conf", _lazyConfig.Value);
                    }
                });
        }

        public string GetCommand(string commandName, string defaultValue)
        {
            var conf = _lazyConfig.Value;
            if (conf.CommandList == null)
            {
                conf.CommandList = new Dictionary<string, string>();
            }


            if (conf.CommandList.TryGetValue(commandName, out var command))
            {
                return command;
            }

            _saveEventSubject.OnNext(Unit.Default);
            conf.CommandList.Add(commandName, defaultValue);
            return defaultValue;
        }

        public void Dispose()
        {
            _saveEventSubject?.Dispose();
        }
    }
}