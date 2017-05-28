using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(false , true)]
    [ParupunteIsono("けつるーら")]
    class KetsuWarp : ParupunteScript
    {
        public KetsuWarp(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "行き先を選べ!";
        public override string SubName { get; } = "ケツルーラ";

        public override void OnStart()
        {
            StartCoroutine(IdleCoroutine());
        }

        private IEnumerable<object> IdleCoroutine()
        {
            foreach (var w in WaitForSeconds(10))
            {
                var blip = GTA.World.GetActiveBlips().FirstOrDefault(x => x.Exists());
                if (blip != null)
                {
                    StartCoroutine(MoveToCoroutine());
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerable<object> MoveToCoroutine()
        {
            var target = core.PlayerPed.IsInVehicle() ? (Entity)core.PlayerPed.CurrentVehicle : (Entity)core.PlayerPed;
            target.IsInvincible = true;

            if (target is Ped)
            {
                ((Ped)target).SetToRagdoll();
            }


            while (target.IsSafeExist())
            {
                var targetBlip = GTA.World.GetActiveBlips().FirstOrDefault(x => x.Exists());
                if (targetBlip == null || !targetBlip.Exists())
                {
                    ParupunteEnd();
                    if (target.IsSafeExist())
                    {
                        target.IsInvincible = false;
                    }
                    yield break;
                }

                var goal = targetBlip.Position;
                var current = target.Position;
                var dir = (goal - current).Normalized;

                var toVector = (goal - current);
                var horizontalLength = new Vector3(toVector.X, toVector.Y,0).Length();


                if (horizontalLength < 30)
                {
                    if (target.IsSafeExist())
                    {
                        target.IsInvincible = false;
                    }
                    ParupunteEnd();
                    yield break;
                }


                if (horizontalLength < 50)
                {
                    target.ApplyForce(dir * 150.0f);
                }
                else
                {
                    target.ApplyForce((dir + Vector3.WorldUp * 0.5f) * 60.0f);
                    GTA.World.AddExplosion(target.Position, GTA.ExplosionType.Grenade, 0.5f, 0.5f, true, false);
                }


                yield return WaitForSeconds(0.5f);
            }
            if (target.IsSafeExist())
            {
                target.IsInvincible = false;
            }
            ParupunteEnd();

        }
    }
}
