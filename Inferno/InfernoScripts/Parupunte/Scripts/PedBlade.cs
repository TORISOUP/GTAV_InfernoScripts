using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("回　転　寿　司", "ち　ら　し　寿　司")]
    [ParupunteIsono("かいてんずし")]
    internal sealed class PedBlade : ParupunteScript
    {
        private readonly HashSet<Ped> _joinedPeds = new();

        public PedBlade(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(15 * 1000);
            AddProgressBar(ReduceCounter);

            ReduceCounter.OnFinishedAsync.Subscribe(_ =>
            {
                foreach (var p in _joinedPeds.Where(x => x.IsSafeExist()))
                {
                    if (!Function.Call<bool>(Hash.DOES_ENTITY_HAVE_PHYSICS, p))
                    {
                        Function.Call(Hash.ACTIVATE_PHYSICS, p);
                    }
                    p.ApplyForce(Vector3.RandomXYZ() * 100);
                }
                
                _joinedPeds.Clear();
                ParupunteEnd();
            });
            MainLoopAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask MainLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                foreach (var p in core.CachedPeds
                             .Where(
                                 x =>
                                     x.IsSafeExist() &&
                                     x.IsAlive &&
                                     x.IsInRangeOf(core.PlayerPed.Position, 100) &&
                                     !x.IsRequiredForMission() &&
                                     !_joinedPeds.Contains(x)
                             ))
                {
                    _joinedPeds.Add(p);
                    PedLoopAsync(p, ct).Forget();
                }


                await DelaySecondsAsync(1, ct);
            }
        }

        private async ValueTask PedLoopAsync(Ped ped, CancellationToken ct)
        {
            ped.Task.ClearAllImmediately();
            ped.Ragdoll(-1);
            var randomAngle = Random.NextDouble() * Math.PI * 2;

            while (!ct.IsCancellationRequested)
            {
                if (!ped.IsSafeExist()) return;
                if (core.PlayerPed.IsSafeExist() && core.PlayerPed != ped)
                {
                    if (ped.IsDead && Random.Next(0, 100) < 5)
                    {
                        var l = (core.PlayerPed.Position - ped.Position).Length();
                        if (l > 10)
                        {
                            GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.GrenadeL, 1.0f, 0.0f);
                        }
                    }

                    var le = ReduceCounter.Rate * 20 + 2f;
                    ped.Ragdoll(-1);

                    ped.RotationVelocity = new Vector3(0, 0, 10f);
                    var targetPosition = core.PlayerPed.Position + Vector3.WorldUp
                                                                 +
                                                                 Quaternion.RotationAxis(
                                                                     Vector3.WorldUp,
                                                                     (core.ElapsedTime / 1.5f) * 2f * (float)Math.PI +
                                                                     (float)randomAngle) * (Vector3.UnitX * le);

                    if (ReduceCounter.Rate > 0.3f)
                    {
                        ped.PositionNoOffset = targetPosition;
                    }

                    else
                    {
                        if (!Function.Call<bool>(Hash.DOES_ENTITY_HAVE_PHYSICS, ped))
                        {
                            Function.Call(Hash.ACTIVATE_PHYSICS, ped);
                        }
                        var targetVector = targetPosition - ped.Position;
                        ped.ApplyForce(targetVector.Normalized * 50f);
                    }
                    // if (Random.Next(0, 100) < 50)
                    // {
                    //     ped.RotationVelocity = new Vector3(0, 0, 100f);
                    // }
                    // else
                    // {
                    //     var targetPosition = core.PlayerPed.Position + Vector3.WorldUp
                    //                                                  + Quaternion.RotationAxis(
                    //                                                      Vector3.WorldUp,
                    //                                                      (core.ElapsedTime / 3f) * 2f * (float)Math.PI +
                    //                                                      (float)randomAngle) * (Vector3.UnitX * 20);
                    //
                    //
                    //     var targetVector = targetPosition - ped.Position;
                    //     ped.ApplyForce(targetVector.Normalized * 100f);
                    //     ped.Ragdoll(-1);
                    // }
                }

                await YieldAsync(ct);
            }
        }
    }
}