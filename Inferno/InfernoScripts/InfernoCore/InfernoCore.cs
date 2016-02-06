using GTA;
using System;
using System.Windows.Forms;
using UniRx;

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

        /// <summary>
        /// 周辺市民
        /// </summary>
        public ReactiveProperty<Ped[]> PedsNearPlayer = new ReactiveProperty<Ped[]>();

        /// <summary>
        /// 周辺車両
        /// </summary>
        public ReactiveProperty<Vehicle[]> VehicleNearPlayer = new ReactiveProperty<Vehicle[]>();

        /// <summary>
        /// プレイヤ
        /// </summary>
        public ReactiveProperty<Ped> PlayerPed = new ReactiveProperty<Ped>();

        /// <summary>
        /// プレイヤの乗ってる車両
        /// </summary>
        public ReactiveProperty<Vehicle> PlayerVehicle = new ReactiveProperty<Vehicle>(); 

        /// <summary>
        /// 25ms周期のTick
        /// </summary>
        public static UniRx.IObservable<Unit> OnTickAsObservable => OnTickSubject.AsObservable();

        /// <summary>
        /// キー入力
        /// </summary>
        public static UniRx.IObservable<KeyEventArgs> OnKeyDownAsObservable => OnKeyDownSubject.AsObservable();

        public InfernoCore()
        {
            Instance = this;

            _debugLogger = new DebugLogger(@"InfernoScript.log");

            //100ms周期でイベントを飛ばす
            Interval = 100;
            Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
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
            try
            {
                var player = Game.Player;
                var ped = player?.Character;
                if (!ped.IsSafeExist()) return;
                PlayerPed.Value = ped;
                PedsNearPlayer.Value = World.GetNearbyPeds(ped, 500);
                VehicleNearPlayer.Value = World.GetNearbyVehicles(ped, 500);
                PlayerVehicle.Value = ped?.CurrentVehicle;
            }
            catch (Exception e)
            {
                LogWrite(e.StackTrace);
            }
        }

        public void LogWrite(string message)
        {
            _debugLogger.Log(message);
        }
    }
}
