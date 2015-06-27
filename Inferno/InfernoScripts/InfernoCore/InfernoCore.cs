using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Forms;
using GTA;
using GTA.Native;
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

        private CoroutineSystem coroutineSystem;
        private int _currentShardId = 0;
        private ReactiveProperty<Ped[]> pedsNearPlayer = new ReactiveProperty<Ped[]>(Scheduler.Immediate);
        /// <summary>
        /// 周辺市民
        /// </summary>
        public ReadOnlyReactiveProperty<Ped[]> PedsNearPlayer
        {
            get { return pedsNearPlayer.ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate); }
        }

        private ReactiveProperty<Vehicle[]> vehiclesNearPlayer = new ReactiveProperty<Vehicle[]>(Scheduler.Immediate);
        /// <summary>
        /// 周辺車両
        /// </summary>
        public ReadOnlyReactiveProperty<Vehicle[]> VehicleNearPlayer
        {
            get { return vehiclesNearPlayer.ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate); }
        }

        private ReactiveProperty<Ped> playerPed = new ReactiveProperty<Ped>(Scheduler.Immediate);

        public ReadOnlyReactiveProperty<Ped> PlayerPed => playerPed.ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate); 

        /// <summary>
        /// 25ms周期のTick
        /// </summary>
        public static IObservable<Unit> OnTickAsObservable
        {
            get { return OnTickSubject.AsObservable(); }
        }

        /// <summary>
        /// キー入力
        /// </summary>
        public static IObservable<KeyEventArgs> OnKeyDownAsObservable 
        {
            get { return OnKeyDownSubject.AsObservable(); }
        }

        public InfernoCore()
        {
            Instance = this;
            
            _debugLogger = new DebugLogger(@"InfernoScript.log");
            coroutineSystem = new CoroutineSystem(_debugLogger);

            //25ms周期でイベントを飛ばす
            Interval = 25;
            Observable.FromEventPattern<EventHandler, EventArgs>(h => h.Invoke, h => Tick += h, h => Tick -= h, Scheduler.Immediate)
                .Select(_ => Unit.Default)
                .Multicast(OnTickSubject)
                .Connect();

            //キー入力
            Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => h.Invoke, h => KeyDown += h,
                h => KeyDown -= h, Scheduler.Immediate)
                .Select(e => e.EventArgs)
                .Multicast(OnKeyDownSubject)
                .Connect();


            //市民と車両の更新
            OnTickSubject
                .Skip(9).Take(1).Repeat()
                .Subscribe(_ => UpdatePedsAndVehiclesList());

            //コルーチン処理
            OnTickSubject
                .Subscribe(_ =>
                {
                    try
                    {
                        //コルーチンは4分割されて実行される
                        coroutineSystem.CoroutineLoop(_currentShardId);
                        _currentShardId = (_currentShardId + 1)%4;
                    }
                    catch (Exception e)
                    {
                        LogWrite(e.StackTrace);
                    }
                });
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
                playerPed.Value = ped;
                pedsNearPlayer.Value = World.GetNearbyPeds(ped, 500, 300);
                vehiclesNearPlayer.Value = World.GetNearbyVehicles(ped, 500, 300);
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
