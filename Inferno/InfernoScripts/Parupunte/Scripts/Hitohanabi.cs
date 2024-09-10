using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ひとはなび", "きたねぇ花火だ")]
    [ParupunteIsono("ひとはなび")]
    internal class Hitohanabi : ParupunteScript
    {
        private Vector3 _targetPosition;
        private HashSet<Ped> _pedList;

        public Hitohanabi(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnSetUp()
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(5000);
            AddProgressBar(ReduceCounter);
            _targetPosition = core.PlayerPed.Position.Around(30) + new Vector3(0, 0, 15);
            _pedList = new HashSet<Ped>();

            ReduceCounter.OnFinishedAsync.Subscribe(_ => Explode());
            CheckAroundPedAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask CheckAroundPedAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && !ReduceCounter.IsCompleted)
            {
                foreach (
                        var targetPed in
                        core.CachedPeds.Where(
                            x => x.IsSafeExist()
                                 && x.IsAlive
                                 && !x.IsCutsceneOnlyPed()
                                 && x.IsInRangeOf(core.PlayerPed.Position, 100))
                    )
                    //まだの人をリストにくわえる
                {
                    if (_pedList.Add(targetPed))
                    {
                        targetPed.Task.ClearAllImmediately();
                        targetPed.CanRagdoll = true;
                        targetPed.SetToRagdoll();
                        targetPed.FreezePosition(false);

                        PedLoopAsync(targetPed, ct).Forget();
                    }
                }

                await Delay100MsAsync(ct);
            }
        }

        private async ValueTask PedLoopAsync(Ped ped, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && !ReduceCounter.IsCompleted)
            {
                if (!ped.IsSafeExist())
                {
                    return;
                }

                if (!Function.Call<bool>(Hash.DOES_ENTITY_HAVE_PHYSICS, ped))
                {
                    Function.Call(Hash.ACTIVATE_PHYSICS, ped);
                }

                //すいこむ
                var direction = _targetPosition - ped.Position;
                direction.Normalize();
                ped.ApplyForce(direction * 10f, Vector3.RandomXY(),
                    ForceType.MaxForceRot);
                await YieldAsync(ct);
            }
        }

        private void Explode()
        {
            var expolodeCount = 0;

            //バクハツシサン
            foreach (var targetPed in _pedList.Where(x => x.IsSafeExist())
                         .OrderBy(x => x.Position.DistanceToSquared(_targetPosition)))
            {
                if (!Function.Call<bool>(Hash.DOES_ENTITY_HAVE_PHYSICS, targetPed))
                {
                    Function.Call(Hash.ACTIVATE_PHYSICS, targetPed);
                }
                targetPed.Kill();
                targetPed.Task.ClearAllImmediately();
                targetPed.ApplyForce(Vector3.RandomXYZ() * 100f, Vector3.RandomXY(),
                    ForceType.MaxForceRot);
                if (expolodeCount++ < 5)
                {
                    GTA.World.AddExplosion(targetPed.Position, GTA.ExplosionType.FireWork, 2.0f, 1.0f);
                }
            }

            GTA.World.AddExplosion(_targetPosition, GTA.ExplosionType.Tanker, 2.0f, 1.0f);


            //終了
            ParupunteEnd();
        }
    }
}