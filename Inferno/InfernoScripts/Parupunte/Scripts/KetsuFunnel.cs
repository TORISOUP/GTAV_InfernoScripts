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
    [ParupunteConfigAttribute("ケツファンネル", "弾切れ")]
    [ParupunteIsono("けつふぁんねる")]
    internal class KetsuFunnel : ParupunteScript
    {
        public KetsuFunnel(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            core.PlayerPed.IsExplosionProof = true;
            OnFinishedAsObservable
                .Subscribe(_ => core.PlayerPed.IsExplosionProof = false);

            KetuLoopAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask KetuLoopAsync(CancellationToken ct)
        {
            while (IsActive && !ct.IsCancellationRequested)
            {
                if (!core.PlayerPed.IsInVehicle())
                {
                    var playerPos = core.PlayerPed.Position;

                    var ketsuDir = -core.PlayerPed.ForwardVector;

                    var targetList = core
                        .CachedEntities
                        .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(playerPos, 100))
                        .Where(x => Vector3.Angle(ketsuDir, x.Position - playerPos) < 30)
                        .ToArray();

                    var target = targetList[Random.Next(0, targetList.Length)];

                    var startPoint = core.PlayerPed.Bones[Bone.SkelSpineRoot].Position;

                    var dir = (target.Position - startPoint).Normalized;
                    
                    NativeFunctions.ShootSingleBulletBetweenCoords(
                        startPoint + dir * 1, target.Position, 100, Weapon.RPG, null, 500);
                }

                await DelaySecondsAsync(0.5f, ct);
            }
        }
    }
}