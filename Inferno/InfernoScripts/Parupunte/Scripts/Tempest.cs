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
    [ParupunteConfigAttribute("テンペスト")]
    [ParupunteIsono("てんぺすと")]
    internal class Tempest : ParupunteScript
    {
        private readonly HashSet<Entity> _entityList = new();

        public Tempest(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        protected override void OnFinished()
        {
            GTA.World.Weather = Weather.Clear;
            _entityList.Clear();
        }

        public override void OnStart()
        {
            GTA.World.Weather = Weather.ThunderStorm;
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist()
                                                           && x.IsInRangeOf(playerPos, 50)
                                                           && !_entityList.Contains(x)
                                                           && !x.IsCutsceneOnlyPed()))
            {
                _entityList.Add(ped);
                TempestAsync(ped, ActiveCancellationToken).Forget();
            }

            foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
                                                               && x.IsInRangeOf(playerPos, 50)
                                                               && !_entityList.Contains(x)
                     ))
            {
                if (_entityList.Add(veh))
                {
                    TempestAsync(veh, ActiveCancellationToken).Forget();
                }
            }
        }

        private async ValueTask TempestAsync(Entity entity, CancellationToken ct)
        {
            while (!ReduceCounter.IsCompleted && !ct.IsCancellationRequested)
            {
                if (!entity.IsSafeExist())
                {
                    return;
                }


                if (entity is Ped p)
                {
                    p.SetToRagdoll();
                }
                else if (entity is Vehicle v)
                {
                    if (core.PlayerPed.CurrentVehicle == v)
                    {
                        await Delay100MsAsync(ct);
                        continue;
                    }
                }

                var playerPos = core.PlayerPed.Position;
                //プレイヤに向かうベクトル
                var gotoPlayerVector = playerPos - entity.Position;
                var lenght = gotoPlayerVector.Length();
                gotoPlayerVector.Normalize();

                var angle = lenght > 10 ? 89.2f : 90;
                var rotatedVector = Quaternion.RotationAxis(Vector3.WorldUp, angle)
                    .ApplyVector(gotoPlayerVector);

                var mainPower = entity is Ped ? 8 : 5;
                var upPower = entity is Ped ? 1 : 1f;
                var toPlayer = lenght < 10 ? 0 : 4.0f;
                entity.ApplyForce(rotatedVector * mainPower + Vector3.WorldUp * upPower + gotoPlayerVector * toPlayer);

                await YieldAsync(ct);
            }
        }
    }
}