using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("あっちこっち", "おわり")]
    [ParupunteIsono("あっちこっち")]
    internal class PositionShufle : ParupunteScript
    {
        private Random random = new Random();

        public PositionShufle(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        //高速コルーチン(1フレーム単位で実行)
        private CoroutineSystem _quickCoroutineSystem;

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            StartCoroutine(SwapCoroutine());

            _quickCoroutineSystem = new CoroutineSystem();

            var d = core.OnTickAsObservable
                .Subscribe(_ =>
                {
                    _quickCoroutineSystem?.CoroutineLoop();
                });

            this.OnFinishedAsObservable
                .Subscribe(_ =>
                {
                    d.Dispose();
                    _quickCoroutineSystem.RemoveAllCoroutine();
                    _quickCoroutineSystem = null;
                });
        }

        private IEnumerable<object> SwapCoroutine()
        {
            while (!ReduceCounter.IsCompleted)
            {
                var peds = ChoisePeds(core.CachedPeds);
                if (peds.Item1 != peds.Item2)
                {
                    //ポジションの入れ替えは高速コルーチンで実行
                    _quickCoroutineSystem.AddCoroutine(SwapPedPosition(peds.Item1, peds.Item2));
                }
                yield return WaitForSeconds(1.5f);
            }
        }

        /// <summary>
        /// 市民を2人をランダムに抽出する
        /// </summary>
        private System.Tuple<Ped, Ped> ChoisePeds(IEnumerable<Ped> originalGroup)
        {
            var playerPosition = core.PlayerPed.Position;
            var targetGroup = originalGroup.Concat(new[] { core.PlayerPed }).Where(x =>
                 x.IsSafeExist()
                 && x.IsInRangeOf(playerPosition, 150)
                 && x.IsAlive).ToArray();

            var p1 = targetGroup[random.Next(0, targetGroup.Length)];
            var p2 = targetGroup[random.Next(0, targetGroup.Length)];
            return new System.Tuple<Ped, Ped>(p1, p2);
        }

        private IEnumerable<object> SwapPedPosition(Ped p1, Ped p2)
        {

            var isP1InVehicle = p1.IsInVehicle();
            var p1Pos = p1.Position;
            var v1 = p1.CurrentVehicle;
            var v1seat = GetPedSeat(p1, v1);

            var isP2InVehicle = p2.IsInVehicle();
            var p2Pos = p2.Position;
            var v2 = p2.CurrentVehicle;
            var v2seat = GetPedSeat(p2, v2);

            #region P2を退避
            if (!p1.IsSafeExist() || !p2.IsSafeExist()) yield break;

            if (isP2InVehicle)
            {
                p2.Task.ClearAllImmediately();
                yield return null;
            }
            else
            {
                p2.Position += Vector3.WorldUp * 10;
            }

            #endregion P2を退避

            if (!p1.IsSafeExist() || !p2.IsSafeExist()) yield break;

            // p1 -> p2

            #region P1 -> P2

            if (isP2InVehicle)
            {
                if (!v2.IsSafeExist()) yield break;
                p1.Task.WarpIntoVehicle(v2, v2seat);
            }
            else
            {
                p1.PositionNoOffset = p2Pos;
            }

            #endregion P1 -> P2

            yield return null;
            // p2 -> p1

            #region P2 -> P1

            if (isP1InVehicle)
            {
                if (!v1.IsSafeExist()) yield break;
                p2.Task.WarpIntoVehicle(v1, v1seat);
            }
            else
            {
                p2.PositionNoOffset = p1Pos;
            }

            #endregion P2 -> P1
        }

        private VehicleSeat GetPedSeat(Ped ped, Vehicle veh)
        {
            if (!veh.IsSafeExist()) return VehicleSeat.None;
            var seatList = new[]
            {VehicleSeat.Driver, VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear};

            foreach (var s in seatList)
            {
                var p = veh.GetPedOnSeat(s);
                if (p == ped) return s;
            }

            return VehicleSeat.None;
        }
    }
}
