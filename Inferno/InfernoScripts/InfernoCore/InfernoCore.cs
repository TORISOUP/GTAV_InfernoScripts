using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using GTA;
using Inferno.InfernoScripts.Event;
using Reactive.Bindings;

namespace Inferno
{
    /// <summary>
    /// インフェルノスクリプト中で統一して使う機能や処理を押し込めたもの
    /// </summary>
    public sealed class InfernoCore : Script
    {
        private static readonly Subject<Unit> OnTickSubject = new();
        private static readonly Subject<KeyEventArgs> OnKeyDownSubject = new();
        private static readonly Subject<IEventMessage> EventMessageSubject = new();
        private readonly DebugLogger _debugLogger;

        private readonly AsyncSubject<Unit> _disposeSubject = new();

        private readonly ReactiveProperty<Entity[]> _missionEntity = new();

        private readonly ReactiveProperty<Entity[]> _nearEntities = new();

        private readonly ReactiveProperty<Ped[]> _nearPeds = new();

        private readonly ReactiveProperty<Vehicle[]> _nearVehicles = new();

        private readonly ReactiveProperty<Ped> _playerPed = new();

        private readonly ReactiveProperty<Vehicle> _playerVehicle = new();

        private DateTimeOffset _lastUpdate;

        public InfernoCore()
        {
            Instance = this;

            _debugLogger = new DebugLogger(@"InfernoScript.log");

            _lastUpdate = DateTimeOffset.Now;

            //100ms周期でイベントを飛ばす
            Interval = 100;
            Observable
                .FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .TakeUntil(_disposeSubject)
                .Multicast(OnTickSubject)
                .Connect();

            //キー入力
            Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                    h => KeyDown -= h)
                .Select(e => e.EventArgs)
                .TakeUntil(_disposeSubject)
                .Multicast(OnKeyDownSubject)
                .Connect();

            //市民と車両の更新
            OnTickAsObservable
                .Subscribe(_ => UpdatePedsAndVehiclesList());

            Aborted += (_, _) => Destroy();
        }

        public static InfernoCore Instance { get; private set; }

        /// <summary>
        /// 発行されたイベントメッセージ
        /// </summary>
        public static IObservable<IEventMessage> OnReceivedEventMessage => EventMessageSubject;

        /// <summary>
        /// 周辺市民
        /// </summary>
        public IReadOnlyReactiveProperty<Ped[]> PedsNearPlayer => _nearPeds;

        /// <summary>
        /// 周辺車両
        /// </summary>
        public IReadOnlyReactiveProperty<Vehicle[]> VehiclesNearPlayer => _nearVehicles;

        /// <summary>
        /// 周辺Entity
        /// </summary>
        public IReadOnlyReactiveProperty<Entity[]> EntitiesNearPlayer => _nearEntities;

        /// <summary>
        /// ミッションEntity
        /// </summary>
        public IReadOnlyReactiveProperty<Entity[]> MissionEntities => _missionEntity;

        /// <summary>
        /// プレイヤ
        /// </summary>
        public IReadOnlyReactiveProperty<Ped> PlayerPed => _playerPed;


        /// <summary>
        /// プレイヤの乗ってる車両
        /// </summary>
        public IReadOnlyReactiveProperty<Vehicle> PlayerVehicle => _playerVehicle;

        /// <summary>
        /// 25ms周期のTick
        /// </summary>
        public static IObservable<Unit> OnTickAsObservable => OnTickSubject.AsObservable();

        /// <summary>
        /// キー入力
        /// </summary>
        public static IObservable<KeyEventArgs> OnKeyDownAsObservable => OnKeyDownSubject.AsObservable();

        /// <summary>
        /// イベントメッセージを発行する
        /// </summary>
        /// <param name="message"></param>
        public static void Publish(IEventMessage message)
        {
            EventMessageSubject.OnNext(message);
        }

        /// <summary>
        /// 市民と車両のキャッシュ
        /// </summary>
        private void UpdatePedsAndVehiclesList()
        {
            // 1秒おきにキャッシュ更新
            if (DateTimeOffset.Now - _lastUpdate < TimeSpan.FromMilliseconds(1000))
            {
                return;
            }

            _lastUpdate = DateTimeOffset.Now;

            try
            {
                var player = Game.Player;
                var ped = player?.Character;
                if (!ped.IsSafeExist())
                {
                    return;
                }

                _playerPed.Value = ped;
                _nearEntities.Value = World.GetNearbyEntities(ped.Position, 500)?.Where(x => x != ped).ToArray() ??
                                      Array.Empty<Entity>();

                _nearPeds.Value = _nearEntities.Value.OfType<Ped>().ToArray();
                _nearVehicles.Value = _nearEntities.Value.OfType<Vehicle>().ToArray();
                _playerVehicle.Value = ped?.CurrentVehicle;

                if (_nearEntities.Value.Any(x => x.IsRequiredForMission()))
                {
                    _missionEntity.Value =
                        _nearEntities.Value.Where(x => x.IsRequiredForMission() && x != ped).ToArray();
                }
                else
                {
                    if (_missionEntity.Value == null || _missionEntity.Value.Length > 0)
                    {
                        _missionEntity.Value = Array.Empty<Entity>();
                    }
                }
            }
            catch (Exception e)
            {
                LogWrite(e.ToString());
            }
        }

        public void LogWrite(string message)
        {
            lock (_debugLogger)
            {
                _debugLogger.Log(message);
            }
        }

        private void Destroy()
        {
            try
            {
                _debugLogger.Dispose();
                _nearPeds.Dispose();
                _nearVehicles.Dispose();
                _nearEntities.Dispose();
                _missionEntity.Dispose();
                _playerPed.Dispose();
                _playerVehicle.Dispose();
                _disposeSubject.OnNext(Unit.Default);
                _disposeSubject.OnCompleted();
                _disposeSubject.Dispose();
            }
            catch
            {
                //
            }
        }
    }
}