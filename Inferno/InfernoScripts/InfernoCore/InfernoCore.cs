using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using GTA;
using GTA.Native;


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

        private CoroutineSystem coroutineSystem;
        private int _currentShardId = 0;
        private readonly BehaviorSubject<Ped[]> _pedsNearPlayer = new BehaviorSubject<Ped[]>(default(Ped[]));
        /// <summary>
        /// 周辺市民
        /// </summary>
        public IObservable<Ped[]> PedsNearPlayer => _pedsNearPlayer.AsObservable();

        private readonly BehaviorSubject<Vehicle[]> _vehiclesNearPlayer = new BehaviorSubject<Vehicle[]>(default(Vehicle[]));
        /// <summary>
        /// 周辺車両
        /// </summary>
        public IObservable<Vehicle[]> VehicleNearPlayer => _vehiclesNearPlayer.AsObservable();

        private BehaviorSubject<Ped> playerPed = new BehaviorSubject<Ped>(default(Ped));

        public IObservable<Ped> PlayerPed => playerPed.AsObservable(); 

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
            coroutineSystem = new CoroutineSystem(_debugLogger);

            //50ms周期でイベントを飛ばす
            Interval = 50;
            Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h)
                .Select(_ => Unit.Default)
                .Multicast(OnTickSubject)
                .Connect();

            //コルーチンは直接イベントフックする
            Tick += CoroutineLoop;

            //キー入力
            Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                h => KeyDown -= h)
                .Select(e => e.EventArgs)
                .Multicast(OnKeyDownSubject)
                .Connect();


            //市民と車両の更新
            OnTickAsObservable
                .Skip(4).Take(1).Repeat()
                .Subscribe(_ => UpdatePedsAndVehiclesList());
        }

        private void CoroutineLoop(object sender, EventArgs e)
        {
            try
            {
                //コルーチンは4分割されて実行される
                coroutineSystem.CoroutineLoop(_currentShardId);
                _currentShardId = (_currentShardId + 1) % 2;
            }
            catch (Exception ex)
            {
                LogWrite(ex.StackTrace);
            }
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
                playerPed.OnNext(ped);
                _pedsNearPlayer.OnNext(World.GetNearbyPeds(ped, 500, 300));
                _vehiclesNearPlayer.OnNext(World.GetNearbyVehicles(ped, 500, 300));
            }
            catch (Exception e)
            {
                LogWrite(e.StackTrace);
            }
        }

        /// <summary>
        /// コルーチンの登録
        /// </summary>
        /// <param name="coroutine">コルーチン</param>
        /// <returns>ID</returns>
        public uint AddCrotoutine(IEnumerable<Object> coroutine)
        {
           return coroutineSystem.AddCrotoutine(coroutine);
        }

        /// <summary>
        /// コルーチンの登録解除
        /// </summary>
        /// <param name="id">解除したいコルーチンID</param>
        public void RemoveCoroutine(uint id)
        {
            coroutineSystem.RemoveCoroutine(id);
        }

        public void LogWrite(string message)
        {
            _debugLogger.Log(message);
        }
    }
}
