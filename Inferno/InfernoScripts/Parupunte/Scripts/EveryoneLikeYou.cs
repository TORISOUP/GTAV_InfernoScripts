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
    [ParupunteConfigAttribute("いっぱいちゅき", "よく見たらクソむかつく")]
    [ParupunteIsono("いっぱいちゅき")]
    internal class EveryoneLikeYou : ParupunteScript
    {
        private readonly HashSet<Entity> entityList = new();

        public EveryoneLikeYou(ParupunteCore core, ParupunteConfigElement config) : base(core, config)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            core.PlayerPed.IsInvincible = true;
            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                core.PlayerPed.IsInvincible = false;

                ParupunteEnd();
            });

            UpdateAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask UpdateAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var playerPos = core.PlayerPed.Position;
                foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist()
                                                               && x.IsInRangeOf(playerPos, 30)
                                                               && !entityList.Contains(x)
                                                               && !x.IsCutsceneOnlyPed()))
                {
                    entityList.Add(ped);
                    MoveAsync(ped, ActiveCancellationToken).Forget();
                }

                foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
                                                                   && x.IsInRangeOf(playerPos, 30)
                                                                   && !entityList.Contains(x)
                         ))
                {
                    if (entityList.Add(veh))
                    {
                        MoveAsync(veh, ActiveCancellationToken).Forget();
                    }
                }

                await DelaySecondsAsync(1, ct);
            }
        }

        private async ValueTask MoveAsync(Entity entity, CancellationToken ct)
        {
            while (!ReduceCounter.IsCompleted && !ct.IsCancellationRequested)
            {
                if (!entity.IsSafeExist())
                {
                    return;
                }

                var playerPos = core.PlayerPed.Position;

                if (entity is Ped p)
                {
                    if (p.IsDead)
                    {
                        return;
                    }

                    p.SetToRagdoll();
                }

                //プレイヤに向かうベクトル
                var gotoPlayerVector = playerPos - entity.Position;
                gotoPlayerVector.Normalize();

                var mainPower = Random.Next(5, 10);
                var upPower = Random.Next(2, 5);
                var offset = !entity.IsInRangeOf(playerPos, 30) ? 10 : 0;

                if (entity.IsInRangeOf(playerPos, 5))
                {
                    mainPower = 0;
                    offset = 0;
                    upPower = Random.Next(5, 10);
                }

                entity.ApplyForce(gotoPlayerVector * (mainPower + offset) + Vector3.WorldUp * upPower);

                await DelaySecondsAsync((float)Random.NextDouble() / 1.0f, ct);
            }
        }
    }
}