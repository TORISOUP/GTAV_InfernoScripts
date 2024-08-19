using GTA;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Windows.Forms;
using Inferno.InfernoScripts.Event;
using Reactive.Bindings;


namespace Inferno
{
    /// <summary>
    /// インフェルノスクリプト中で統一して使う機能や処理を押し込めたもの
    /// </summary>
    public sealed class InfernoCore : Script
    {
        private DebugLogger _debugLogger;

        public static InfernoCore Instance { get; private set; }

        private static readonly Subject<Unit> OnTickSubject = new Subject<Unit>();
        private static readonly Subject<KeyEventArgs> OnKeyDownSubject = new Subject<KeyEventArgs>();
        private static readonly Subject<IEventMessage> EventMessageSubject = new Subject<IEventMessage>();

        private DateTimeOffset _lastUpdate;

        /// <summary>
        /// イベントメッセージを発行する
        /// </summary>
        /// <param name="message"></param>
        public static void Publish(IEventMessage message)
        {
            EventMessageSubject.OnNext(message);
        }

        /// <summary>
        /// 発行されたイベントメッセージ
        /// </summary>
        public static IObservable<IEventMessage> OnRecievedEventMessage => EventMessageSubject;

        /// <summary>
        /// 周辺市民
        /// </summary>
        public IReadOnlyReactiveProperty<Ped[]> PedsNearPlayer => _nearPeds;

        private readonly ReactiveProperty<Ped[]> _nearPeds = new ReactiveProperty<Ped[]>();

        /// <summary>
        /// 周辺車両
        /// </summary>
        public IReadOnlyReactiveProperty<Vehicle[]> VehicleNearPlayer => _nearVehicle;

        private readonly ReactiveProperty<Vehicle[]> _nearVehicle = new ReactiveProperty<Vehicle[]>();


        /// <summary>
        /// プレイヤ
        /// </summary>
        public IReadOnlyReactiveProperty<Ped> PlayerPed => _playerPed;

        private readonly ReactiveProperty<Ped> _playerPed = new ReactiveProperty<Ped>();


        /// <summary>
        /// プレイヤの乗ってる車両
        /// </summary>
        public IReadOnlyReactiveProperty<Vehicle> PlayerVehicle => _playerVehicle;

        private readonly ReactiveProperty<Vehicle> _playerVehicle = new ReactiveProperty<Vehicle>();

        /// <summary>
        /// 25ms周期のTick
        /// </summary>
        public static IObservable<Unit> OnTickAsObservable => OnTickSubject.AsObservable();

        /// <summary>
        /// キー入力
        /// </summary>
        public static IObservable<KeyEventArgs> OnKeyDownAsObservable => OnKeyDownSubject.AsObservable();

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
                .Multicast(OnTickSubject)
                .Connect();

            //キー入力
            Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                    h => KeyDown -= h)
                .Select(e => e.EventArgs)
                .Multicast(OnKeyDownSubject)
                .Connect();

            //市民と車両の更新
            OnTickAsObservable
                .Subscribe(_ => UpdatePedsAndVehiclesList());
        }

        /// <summary>
        /// 市民と車両のキャッシュ
        /// </summary>
        private void UpdatePedsAndVehiclesList()
        {
            // 1秒おきにキャッシュ更新
            if (DateTimeOffset.Now - _lastUpdate < TimeSpan.FromMilliseconds(1000)) return;
            _lastUpdate = DateTimeOffset.Now;

            try
            {
                var player = Game.Player;
                var ped = player?.Character;
                if (!ped.IsSafeExist()) return;
                _playerPed.Value = ped;
                _nearPeds.Value = World.GetNearbyPeds(ped, 500) ?? Array.Empty<Ped>();
                _nearVehicle.Value = World.GetNearbyVehicles(ped, 500) ?? Array.Empty<Vehicle>();
                _playerVehicle.Value = ped?.CurrentVehicle;
            }
            catch (Exception e)
            {
                LogWrite(e.Message + "\n" + e.StackTrace);
            }
        }

        public void LogWrite(string message)
        {
            _debugLogger.Log(message);
        }
    }
}