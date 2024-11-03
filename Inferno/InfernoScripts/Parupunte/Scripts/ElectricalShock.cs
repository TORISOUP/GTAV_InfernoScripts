using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("エレクトリカルショック", "おわり")]
    [ParupunteIsono("でんき")]
    internal class ElectricalShock : ParupunteScript
    {
        public ElectricalShock(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            ReduceCounter = new ReduceCounter(20 * 1000);

            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            ElectricalAsync(ActiveCancellationToken).Forget();
        }

        private async ValueTask ElectricalAsync(CancellationToken ct)
        {
            if (!core.PlayerPed.IsSafeExist())
            {
                ParupunteEnd();
                return;
            }

            while (IsActive && !ct.IsCancellationRequested)
            {
                var pos = core.PlayerPed.Position;
                var bones = new[]
                {
                    Bone.IKHead,
                    Bone.IKLeftFoot,
                    Bone.IKLeftHand,
                    Bone.IKRightFoot,
                    Bone.IKRightHand
                };
                foreach (var ped in core.CachedPeds.Where(x => x.IsSafeExist() && x.IsInRangeOf(pos, 30)))
                {
                    var vec = (ped.Position - pos).Normalized;
                    var random1 = (float)Random.NextDouble() / 2.0f;

                    if (ped.IsInVehicle())
                    {
                        //窓を割って頭に向かってスタンガンを撃つ
                        Function.Call(Hash.SMASH_VEHICLE_WINDOW, ped.CurrentVehicle, 0);
                        Function.Call(Hash.SMASH_VEHICLE_WINDOW, ped.CurrentVehicle, 1);
                        Function.Call(Hash.SMASH_VEHICLE_WINDOW, ped.CurrentVehicle, 2);
                        Function.Call(Hash.SMASH_VEHICLE_WINDOW, ped.CurrentVehicle, 3);

                        NativeFunctions.ShootSingleBulletBetweenCoords(
                            pos + new Vector3(0, 0, random1) + vec,
                            ped.GetBonePosition(Bone.IKHead), 1, WeaponHash.StunGun, null, 1.0f);
                    }
                    else
                    {
                        //適当な体の部位に向かって撃つ
                        var target = bones[Random.Next(0, bones.Length)];
                        NativeFunctions.ShootSingleBulletBetweenCoords(
                            pos + new Vector3(0, 0, random1) + vec,
                            ped.GetBonePosition(target), 1, WeaponHash.StunGun, null, 1.0f);
                    }
                }

                await DelaySecondsAsync(0.7f, ct);
            }
        }
    }
}