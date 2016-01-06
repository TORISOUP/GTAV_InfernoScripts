using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA; using UniRx;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    class Tempest : ParupunteScript
    {
        HashSet<Entity> entityList = new HashSet<Entity>();

        public Tempest(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "テンペスト";
        
        public override void OnSetUp()
        {
        }

        protected override void OnFinished()
        {
            GTA.World.Weather = Weather.Clear;
        }

        public override void OnStart()
        {
            GTA.World.Weather = Weather.ThunderStorm;
            ReduceCounter = new ReduceCounter(20*1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(playerPos, 50)))
            {
                if (!entityList.Contains(ped))
                {
                    entityList.Add(ped);
                    StartCoroutine(TemepenstCoroutine(ped));
                }
            }
            foreach (var veh in core.CachedVehicles.Where(x => x.IsSafeExist() && x.IsInRangeOf(playerPos, 50)))
            {
                if (!entityList.Contains(veh))
                {
                    entityList.Add(veh);
                    StartCoroutine(TemepenstCoroutine(veh));
                }
            }
        }

        private IEnumerable<object> TemepenstCoroutine(Entity entity)
        {
            while (!ReduceCounter.IsCompleted)
            {
                if(!entity.IsSafeExist()) yield break;
                if (!entity.IsInRangeOf(core.PlayerPed.Position, 100)) yield return null;
                if (entity is Ped)
                {
                    var p = entity as Ped;
                    p.SetToRagdoll();
                }

                var playerPos = core.PlayerPed.Position;
                //プレイヤに向かうベクトル
                var gotoPlayerVector = playerPos - entity.Position;
                var lenght = gotoPlayerVector.Length();
                gotoPlayerVector.Normalize();


                var angle = lenght > 10 ? 89.2f : 90;
                var rotatedVector = Quaternion.RotationAxis(Vector3.WorldUp, angle)
                    .ApplyVector(gotoPlayerVector);

                var mainPower = entity is Ped ? 5 : 2;
                var upPower = entity is Ped ? 3 : 1.2f;
                entity.ApplyForce(rotatedVector * mainPower + Vector3.WorldUp * upPower);

                yield return null;
            }
        } 
    }
}
