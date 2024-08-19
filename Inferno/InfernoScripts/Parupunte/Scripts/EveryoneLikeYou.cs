using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Math;


namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("いっぱいちゅき", "よく見たらクソむかつく")]
    [ParupunteIsono("いっぱいちゅき")]
    class EveryoneLikeYou : ParupunteScript
    {
        private HashSet<Entity> entityList = new HashSet<Entity>();

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
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist()
                                                           && x.IsInRangeOf(playerPos, 30)
                                                           && !entityList.Contains(x)
                                                           && !x.IsCutsceneOnlyPed()))
            {

                entityList.Add(ped);
                StartCoroutine(MoveCoroutine(ped));

            }
            foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist()
                                                               && x.IsInRangeOf(playerPos, 30)
                                                               && !entityList.Contains(x)
            ))
            {
                if (!entityList.Contains(veh))
                {
                    entityList.Add(veh);
                    StartCoroutine(MoveCoroutine(veh));
                }
            }
        }

        private IEnumerable<object> MoveCoroutine(Entity entity)
        {
            while (!ReduceCounter.IsCompleted)
            {
                if (!entity.IsSafeExist()) yield break;
                var playerPos = core.PlayerPed.Position;

                if (entity is Ped)
                {
                    var p = entity as Ped;
                    if (p.IsDead) yield break;
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

                yield return WaitForSeconds((float)Random.NextDouble() / 1.0f);
            }
        }
    }
}
