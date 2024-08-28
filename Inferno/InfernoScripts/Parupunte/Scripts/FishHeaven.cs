using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteConfigAttribute("おさかな天国", "おさかな地獄")]
    [ParupunteIsono("おさかなてんごく")]
    internal class FishHeaven : ParupunteScript
    {
        private readonly List<Ped> createdFishList = new();
        private readonly Model fishModel = new(PedHash.TigerShark);
        private readonly HashSet<int> vehicles = new();

        public FishHeaven(ParupunteCore core, ParupunteConfigElement element) : base(core, element)
        {
        }

        public override void OnStart()
        {
            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            ReduceCounter = new ReduceCounter(20 * 1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            //プレイヤ監視
            StartCoroutine(ObservePlayer());
        }

        protected override void OnFinished()
        {
            foreach (var x in createdFishList.Where(x => x.IsSafeExist())) x.Detach();
        }

        protected override void OnUpdate()
        {
            //周辺車両を監視
            var playerPos = core.PlayerPed.Position;
            foreach (var v in core.CachedVehicles.Where(
                         x => x.IsSafeExist()
                              && !vehicles.Contains(x.Handle)
                              && x.IsAlive
                              && x.IsInRangeOf(playerPos, 50)
                              && x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist()
                     ))
            {
                vehicles.Add(v.Handle);
                StartCoroutine(CitizenVehicleCoroutine(v));
            }
        }

        /// <summary>
        /// 周辺車両に魚の載せてしばらくたったら発射する
        /// </summary>
        private IEnumerable<object> CitizenVehicleCoroutine(Vehicle veh)
        {
            yield return null;
            if (!veh.IsSafeExist())
            {
                yield break;
            }

            var f = SpawnFish(veh);
            if (!f.IsSafeExist())
            {
                yield break;
            }

            //しばらくたったら発射する
            foreach (var s in WaitForSeconds(Random.Next(1, 10)))
            {
                //プレイヤが途中で乗り込んだら直ちに発射
                if (veh.IsSafeExist() && veh == core.PlayerPed.CurrentVehicle)
                {
                    StartCoroutine(ShootFish(veh, f));
                    yield break;
                }

                yield return null;
            }

            if (veh.IsSafeExist())
            {
                StartCoroutine(ShootFish(veh, f));
            }
        }

        /// <summary>
        /// プレイヤが車に乗ったか監視する
        /// </summary>
        private IEnumerable<object> ObservePlayer()
        {
            while (IsActive)
            {
                //プレイヤが車に乗ったか監視
                while (!core.GetPlayerVehicle().IsSafeExist()) yield return null;
                //発射したら1秒まってもう一度まつ
                yield return PlayerVehicleCoroutine(core.GetPlayerVehicle());
                yield return WaitForSeconds(1);
            }
        }

        private IEnumerable<object> PlayerVehicleCoroutine(Vehicle playerVehicle)
        {
            //車に乗ったら魚生成
            var p = SpawnFish(playerVehicle);
            if (!p.IsSafeExist())
            {
                yield break;
            }

            //キー入力待機
            while (!core.IsGamePadPressed(GameKey.VehicleHorn))
            {
                //途中で車を降りたら発射
                if (!core.PlayerPed.IsInVehicle())
                {
                    StartCoroutine(ShootFish(core.PlayerPed.CurrentVehicle, p));
                    yield break;
                }

                if (!IsActive)
                {
                    yield break;
                }

                yield return null;
            }

            //魚発射
            StartCoroutine(ShootFish(core.PlayerPed.CurrentVehicle, p));
        }

        /// <summary>
        /// 魚を生成する
        /// </summary>
        private Ped SpawnFish(Vehicle target)
        {
            var f = GTA.World.CreatePed(fishModel, target.Position + Vector3.WorldUp * 10);
            if (!f.IsSafeExist() || !target.IsSafeExist())
            {
                return null;
            }

            f.MarkAsNoLongerNeeded();
            f.AttachTo(target.Bones.Core, Vector3.WorldUp * 1.5f, target.ForwardVector);
            f.IsInvincible = true;
            f.Health = 100;
            createdFishList.Add(f);
            return f;
        }

        /// <summary>
        /// 魚を発射してしばらくしたら爆破する
        /// </summary>
        private IEnumerable<object> ShootFish(Vehicle veh, Ped fish)
        {
            if (!fish.IsSafeExist())
            {
                if (veh.IsSafeExist())
                {
                    vehicles.Remove(veh.Handle);
                }

                yield break;
            }

            fish.Detach();
            fish.IsInvincible = false;
            fish.RequestCollision();
            fish.FreezePosition(false);
            CreateEffect(fish, "ent_sht_electrical_box");

            yield return null;

            if (fish.IsSafeExist())
            {
                fish.Velocity = fish.ForwardVector * (100 + veh.Speed);
            }

            //速度が一定以下になったら爆発
            foreach (var x in WaitForSeconds(10))
            {
                if (!fish.IsSafeExist())
                {
                    if (veh.IsSafeExist())
                    {
                        vehicles.Remove(veh.Handle);
                    }

                    yield break;
                }

                if (fish.Velocity.Length() < 6)
                {
                    break;
                }

                yield return null;
            }

            if (veh.IsSafeExist())
            {
                vehicles.Remove(veh.Handle);
            }

            if (!fish.IsSafeExist())
            {
                yield break;
            }

            GTA.World.AddExplosion(fish.Position, GTA.ExplosionType.Grenade, 1.0f, 1.0f);
        }

        private void CreateEffect(Ped ped, string effect)
        {
            if (!ped.IsSafeExist())
            {
                return;
            }

            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SkelPelvis, scale, 0,
                0, 0);
        }
    }
}