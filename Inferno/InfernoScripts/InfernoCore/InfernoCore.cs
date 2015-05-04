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
        public static InfernoCore Instance { get; private set; }

        private static readonly Subject<Unit> OnTickSubject = new Subject<Unit>();
        private static readonly Subject<KeyEventArgs> OnKeyDownSubject = new Subject<KeyEventArgs>();

        private CoroutineSystem coroutineSystem;

        private static ReactiveProperty<Ped[]> pedsNearPlayer = new ReactiveProperty<Ped[]>(Scheduler.Immediate);
        /// <summary>
        /// 周辺市民
        /// </summary>
        public static ReadOnlyReactiveProperty<Ped[]> PedsNearPlayer
        {
            get { return pedsNearPlayer.ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate); }
        }

        private static ReactiveProperty<Vehicle[]> vehiclesNearPlayer = new ReactiveProperty<Vehicle[]>(Scheduler.Immediate);
        /// <summary>
        /// 周辺車両
        /// </summary>
        public static ReadOnlyReactiveProperty<Vehicle[]> VehicleNearPlayer
        {
            get { return vehiclesNearPlayer.ToReadOnlyReactiveProperty(eventScheduler: Scheduler.Immediate); }
        }


        /// <summary>
        /// 100ms周期のTick
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

        /// <summary>
        /// テキスト表示
        /// </summary>
        /// <param name="text"></param>
        public static void SetDrawText(string text)
        {
            //フォント指定
            Function.Call(Hash.SET_TEXT_FONT, new InputArgument[] { 0 });
            //文字スケール
            Function.Call(Hash.SET_TEXT_SCALE, new InputArgument[]
            {
                0.55f,
                0.55f
            });
            //文字色
            Function.Call(Hash.SET_TEXT_COLOUR, new InputArgument[]
            {
                255,    //R?
                255,    //G?
                255,    //B?
                255     //A?
            });
            //中央設置？
            Function.Call(Hash.SET_TEXT_CENTRE, new InputArgument[] { 1 });
            //中央設置？
            Function.Call(Hash.SET_TEXT_CENTRE, new InputArgument[]
            {
                1
            });
            //文字の影poi
            Function.Call(Hash.SET_TEXT_DROPSHADOW, new InputArgument[]
            {
                0,
                0,
                0,
                0,
                0
            });
            //文字のエッジ
            Function.Call(Hash.SET_TEXT_EDGE, new InputArgument[]
            {
                1,
                0,
                0,
                0,
                205
            });
            //文字の種類？
            Function.Call(Hash._SET_TEXT_ENTRY, new InputArgument[] { "STRING" });
            //テキストとして表示する文字
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, new InputArgument[] { text });
            //テキスト描画
            Function.Call(Hash._DRAW_TEXT, new InputArgument[]
            {
                0.5f,
                0.5f
            });
        }

        public InfernoCore()
        {
            Instance = this;
            coroutineSystem = new CoroutineSystem();

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
            OnTickSubject
                .Skip(4).Take(1).Repeat()
                .Subscribe(_ => UpdatePedsAndVehiclesList());

            //コルーチン処理
            OnTickSubject
                .Subscribe(_ => coroutineSystem.CoroutineLoop());
        }

        /// <summary>
        /// 市民と車両のキャッシュ
        /// </summary>
        private void UpdatePedsAndVehiclesList()
        {
            var player = Game.Player.Character;
            if(!player.IsSafeExist()) return;

            pedsNearPlayer.Value = World.GetNearbyPeds(player, 3000);
            vehiclesNearPlayer.Value = World.GetNearbyVehicles(player, 3000);
        }

        /// <summary>
        /// コルーチンの登録
        /// </summary>
        /// <param name="coroutine">コルーチン</param>
        /// <returns>ID</returns>
        public uint AddCrotoutine(IEnumerator coroutine)
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
    }
}
