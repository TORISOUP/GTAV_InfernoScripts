using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;
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
        }

        protected override void OnUpdate()
        {
            //タイマが終わるまでカウントし続ける
            if (!ReduceCounter.IsCompleted)
            {
                foreach (
                        var targetPed in
                        core.CachedPeds.Where(
                            x => x.IsSafeExist()
                                 && x.IsAlive
                                 && x.IsHuman
                                 && !x.IsCutsceneOnlyPed()
                                 && x.IsInRangeOf(core.PlayerPed.Position, 100))
                    )
                    //まだの人をリストにくわえる
                {
                    if (_pedList.Add(targetPed))
                    {
                        if (targetPed.IsInVehicle())
                        {
                            targetPed.Task.ClearAllImmediately();
                        }

                        targetPed.CanRagdoll = true;
                        targetPed.SetToRagdoll();
                        targetPed.FreezePosition(false);
                    }
                }

                foreach (var targetPed in _pedList.Where(x => x.IsSafeExist()))
                {
                    //すいこむ
                    var direction = _targetPosition - targetPed.Position;
                    var lenght = direction.Length();
                    direction.Normalize();
                    targetPed.ApplyForce(direction * lenght.Clamp(0, 1.5f));
                }

                return;
            }

            var expolodeCount = 0;

            //バクハツシサン
            foreach (var targetPed in _pedList.Where(x => x.IsSafeExist())
                         .OrderBy(x => x.Position.DistanceToSquared(_targetPosition)))
            {
                targetPed.Kill();
                targetPed.ApplyForce(InfernoUtilities.CreateRandomVector() * 10);
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