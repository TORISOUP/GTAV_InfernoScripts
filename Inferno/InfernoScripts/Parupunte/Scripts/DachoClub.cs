using System.Collections.Generic;
using System.Linq;
using GTA.Math;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(true)]
    internal class DachoClub : ParupunteScript
    {
        public DachoClub(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "ダチョウ倶楽部";
        public override string EndMessage { get; } = "ありがとうございました";

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20*1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            //プレイヤの着地を監視
            this.UpdateAsObservable
                .Select(_ => !core.PlayerPed.IsInAir)
                .DistinctUntilChanged()
                .Where(x => x)
                .Skip(1)
                .Subscribe(_ =>
                {
                    StartCoroutine(JumpCoroutine());

                });

        }

        private IEnumerable<object> JumpCoroutine()
        {
            //ワンテンポ遅らせたかったけど微妙だった
            yield return WaitForSeconds(0.1f);
            var playerPos = core.PlayerPed.Position;
            GTA.World.AddExplosion(playerPos + Vector3.WorldUp * 10, GTA.ExplosionType.Grenade,0.01f, 0.5f);
            #region Ped
            foreach (var p in core.CachedPeds.Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(playerPos, 100)
                     && x.IsAlive
                     && !x.IsCutsceneOnlyPed()))
            {
                p.SetToRagdoll();
                p.ApplyForce(Vector3.WorldUp*10f);
            }

            #endregion

            #region Vehicle

            foreach (var v in core.CachedVehicles.Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(playerPos, 100)
                     && x.IsAlive))
            {
                v.ApplyForce(Vector3.WorldUp * 5f,Vector3.RandomXYZ() );
            }

            #endregion

        }
    }

}

