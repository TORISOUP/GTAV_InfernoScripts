using System;
using System.Reactive.Linq;
using Inferno.InfernoScripts.Event.Isono;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    //ISONO管理マネージャ
    public sealed class IsonoManager : InfernoScript
    {
        private IsonoHttpServer _httpServer;
        protected override bool DefaultAllOnEnable => false;
        protected override string ConfigFileName => "Isono.conf";
        private IsonoConfig _conf;

        protected override void Setup()
        {
            _conf ??= LoadConfig<IsonoConfig>();

            CreateInputKeywordAsObservable("Isono", "isono")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    if (IsActive)
                    {
                        _httpServer.Start();
                    }
                    else
                    {
                        _httpServer.Stop();
                    }
                });

            IsActiveRP.Subscribe(x =>
            {
                try
                {
                    if(_httpServer is { IsDisposed: false })
                    {
                        _httpServer?.Stop();
                        _httpServer?.Dispose();
                    }
                }
                catch (Exception e)
                {
                    LogWrite(e);
                }

                if (x)
                {
                    _httpServer = new IsonoHttpServer(_conf.Port);
                    _httpServer.OnReceivedMessageAsObservable
                        .ObserveOn(InfernoScheduler)
                        .Subscribe(c => InfernoCore.Publish(c));
                    try
                    {
                        _httpServer.Start();
                        DrawText("Isono start");
                    }
                    catch (Exception e)
                    {
                        LogWrite(e);
                        DrawText("Failed to start Isono. Please check the port number.");
                        IsActive = false;
                    }
                }
            });
            
            OnAbortAsync.Subscribe(_ =>
            {
                try
                {
                    _httpServer?.Stop();
                    _httpServer?.Dispose();
                }
                catch
                {
                    //ignore
                }
            });
        }

        public class IsonoConfig : InfernoConfig
        {
            private int _port = 11211;

            public int Port
            {
                get => _port;
                set => _port = value.Clamp(1, 65535);
            }

            public override bool Validate()
            {
                return Port is > 0 and < 65535;
            }
        }


        public override bool UseUI => true;
        public override string DisplayName => IsonoLocalize.Title;
        public override string Description => IsonoLocalize.Description;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.Root;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu subMenu)
        {
            _conf ??= LoadConfig<IsonoConfig>();

            var s = subMenu.AddSlider(
                $"Port: {_conf.Port}",
                IsonoLocalize.Port,
                _conf.Port,
                65535,
                x => { x.Value = _conf.Port; }, item =>
                {
                    _conf.Port = item.Value;
                    item.Title = $"Port: {_conf.Port}";
                });

            IsActiveRP.Subscribe(x => s.Enabled = !x);

            subMenu.AddButton(InfernoCommon.SaveConf, InfernoCommon.SaveConfDescription, _ =>
            {
                SaveConfig(_conf);
                DrawText($"Saved to {ConfigFileName}");
            });
        }
    }
}