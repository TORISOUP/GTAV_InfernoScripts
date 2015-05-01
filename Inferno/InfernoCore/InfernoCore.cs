using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using GTA;
using Reactive.Bindings;

namespace Inferno
{
    /// <summary>
    /// インフェルノスクリプト中で統一して使う機能や処理を押し込めたもの
    /// </summary>
    public sealed class InfernoCore : Script
    {
        private static readonly Subject<Unit> OnTickSubject = new Subject<Unit>();
        private static readonly Subject<KeyEventArgs> OnKeyDownSubject = new Subject<KeyEventArgs>();



        private static ReactiveProperty<Ped[]> pedsNearPlayer = new ReactiveProperty<Ped[]>();
        /// <summary>
        /// プレイや周辺の市民
        /// </summary>
        public static ReadOnlyReactiveProperty<Ped[]> PedsNearPlayer
        {
            get { return pedsNearPlayer.ToReadOnlyReactiveProperty(); }
        }

        private static ReactiveProperty<Vehicle[]> vehiclesNearPlayer = new ReactiveProperty<Vehicle[]>();

        /// <summary>
        /// プレイヤ周辺の車両
        /// </summary>
        public static ReadOnlyReactiveProperty<Vehicle[]> VehicleNearPlayer
        {
            get { return vehiclesNearPlayer.ToReadOnlyReactiveProperty(); }
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


        public InfernoCore()
        {
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
                .Subscribe(_ => UpdatePedsAndVehiclesList());
        }

        /// <summary>
        /// 市民と車両のキャッシュ
        /// </summary>
        private void UpdatePedsAndVehiclesList()
        {
            var player = Game.Player.Character;
            if(player==null || !player.Exists()) return;

            pedsNearPlayer.Value = World.GetNearbyPeds(player, 1000);
            vehiclesNearPlayer.Value = World.GetNearbyVehicles(player, 1000);
        }

    }
}
