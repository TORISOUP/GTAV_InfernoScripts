using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("あっちこっち", "おわり")]
    [ParupunteIsono("あっちこっち")]
    // クラッシュするので封印
    [ParupunteDebug(isIgnore: true)]
    internal class PositionShuffle : ParupunteScript
    {
        private readonly Random random = new();

        public PositionShuffle(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            SwapLoopAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask SwapLoopAsync(CancellationToken ct)
        {
            while (!ReduceCounter.IsCompleted)
            {
                var (p1, p2) = ChoisePeds(core.CachedPeds);
                if (p1.IsSafeExist() && p2.IsSafeExist() && p1 != p2)
                {
                    SwapPedPositionAsync(p1, p2, ct).Forget();
                }

                await DelaySecondsAsync(1.5f, ct);
            }
        }

        /// <summary>
        /// 市民を2人をランダムに抽出する
        /// </summary>
        private (Ped, Ped) ChoisePeds(IEnumerable<Ped> originalGroup)
        {
            if (!core.PlayerPed.IsSafeExist())
            {
                return (null, null);
            }

            var playerPosition = core.PlayerPed.Position;
            var targetGroup = originalGroup.Concat(new[] { core.PlayerPed })
                .Where(x =>
                    x.IsSafeExist()
                    && x.IsInRangeOf(playerPosition, 150)
                    && x.IsAlive)
                .ToArray();

            var p1 = targetGroup[random.Next(0, targetGroup.Length)];
            var p2 = targetGroup[random.Next(0, targetGroup.Length)];
            return (p1, p2);
        }

        private async ValueTask SwapPedPositionAsync(Ped p1, Ped p2, CancellationToken ct)
        {
            var isP1InVehicle = p1.IsInVehicle();
            var p1Pos = p1.Position;
            var v1 = p1.CurrentVehicle;
            var v1seat = p1.SeatIndex;
            var p1Requried = p1.IsRequiredForMission();

            var isP2InVehicle = p2.IsInVehicle();
            var p2Pos = p2.Position;
            var v2 = p2.CurrentVehicle;
            var v2seat = p1.SeatIndex;
            var p2Requried = p2.IsRequiredForMission();

            #region P2を退避

            if (!p1.IsSafeExist() || !p2.IsSafeExist())
            {
                return;
            }

            p1.IsPersistent = true;
            p2.IsPersistent = true;

            if (isP2InVehicle)
            {
                p2.Task.ClearAllImmediately();
            }
            else
            {
                p2.Position += Vector3.WorldUp * 10;
            }

            await YieldAsync(ct);

            #endregion P2を退避

            if (!p1.IsSafeExist() || !p2.IsSafeExist())
            {
                return;
            }

            // p1 -> p2

            #region P1 -> P2

            if (isP2InVehicle)
            {
                if (!v2.IsSafeExist() || !p1.IsSafeExist())
                {
                    return;
                }

                p1.Task.WarpIntoVehicle(v2, v2seat);
            }
            else
            {
                if (p1.IsSafeExist())
                {
                    p1.PositionNoOffset = p2Pos;
                }
            }

            #endregion P1 -> P2

            await YieldAsync(ct);
            // p2 -> p1

            #region P2 -> P1

            if (isP1InVehicle)
            {
                if (!v1.IsSafeExist() || !p2.IsSafeExist())
                {
                    return;
                }

                p2.Task.WarpIntoVehicle(v1, v1seat);
            }
            else
            {
                if (p2.IsSafeExist())
                {
                    p2.PositionNoOffset = p1Pos;
                }
            }

            #endregion P2 -> P1

            if (p1.IsSafeExist())
            {
                p1.IsPersistent = p1Requried;
            }

            if (p2.IsSafeExist())
            {
                p2.IsPersistent = p2Requried;
            }
        }
    }
}