﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA.Math;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("ダチョウ倶楽部", "ありがとうございました")]
    [ParupunteIsono("だちょうくらぶ")]
    internal class DachoClub : ParupunteScript
    {
        public DachoClub(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            //プレイヤの着地を監視
            OnUpdateAsObservable
                .Select(_ => !core.PlayerPed.IsInAir)
                .DistinctUntilChanged()
                .Where(x => x)
                .Skip(1)
                .ThrottleFirst(TimeSpan.FromSeconds(1), core.InfernoScheduler)
                .Subscribe(_ => JumpAsync(ActiveCancellationToken).Forget());
        }

        private async ValueTask JumpAsync(CancellationToken ct)
        {
            await Delay100MsAsync(ct);
            var playerPos = core.PlayerPed.Position;
            GTA.World.AddExplosion(playerPos + Vector3.WorldUp * 10, GTA.ExplosionType.Grenade, 0.01f, 0.5f);

            #region Ped

            foreach (var p in core.CachedPeds.Where(
                         x => x.IsSafeExist()
                              && x.IsInRangeOf(playerPos, 100)
                              && x.IsAlive
                              && !x.IsCutsceneOnlyPed()))
            {
                p.SetToRagdoll();
                p.ApplyForce(Vector3.WorldUp * 10f);
            }

            #endregion

            #region Vehicle

            foreach (var v in core.CachedVehicles.Where(
                         x => x.IsSafeExist()
                              && x.IsInRangeOf(playerPos, 100)
                              && x.IsAlive))
            {
                v.ApplyForce(Vector3.WorldUp * 5f, Vector3.RandomXYZ());
            }

            #endregion
        }
    }
}