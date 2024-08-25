using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

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
            StartCoroutine(KetuCoroutine());
        }

        private IEnumerable<object> KetuCoroutine()
        {
            while (IsActive)
            {
                if (!core.PlayerPed.IsInVehicle())
                {
                    var playerPos = core.PlayerPed.Position;

                    var ketsuDir = -core.PlayerPed.ForwardVector;

                    var targetList = core
                        .CachedPeds.Cast<Entity>()
                        .Concat(core.CachedVehicles)
                        .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInRangeOf(playerPos, 100))
                        .Where(x => Vector3.Angle(ketsuDir, x.Position - playerPos) < 30);

                    var target = targetList.ElementAt(Random.Next(targetList.Count()));

                    var startPoint = core.PlayerPed.GetBonePosition(Bone.SkelPelvis);

                    var dir = (target.Position - startPoint).Normalized;

                    NativeFunctions.ShootSingleBulletBetweenCoords(
                        startPoint + dir * 1, target.Position, 100, WeaponHash.RPG, null, 500);
                }

                yield return WaitForSeconds(0.4f);
            }
        }
    }
}