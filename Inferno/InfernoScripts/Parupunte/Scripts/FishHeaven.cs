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
            ObservePlayerAsync(ActiveCancellationToken).Forget();
            UpdateAsync(ActiveCancellationToken).Forget();
        }

        protected override void OnFinished()
        {
            foreach (var x in createdFishList.Where(x => x.IsSafeExist()))
            {
                x.Detach();
            }
        }

        private async ValueTask UpdateAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                //周辺車両を監視
                var playerPos = core.PlayerPed.Position;
                foreach (var v in core.CachedVehicles.Where(
                             x => x.IsSafeExist()
                                  && !vehicles.Contains(x.Handle)
                                  && x.IsAlive
                                  && x.IsInRangeOf(playerPos, 50)
                         ))
                {
                    var driver = v.Driver;
                    if (!driver.IsSafeExist() || driver.IsPlayer)
                    {
                        continue;
                    }

                    vehicles.Add(v.Handle);
                    CitizenVehicleAsync(v, ct).Forget();
                }

                await Delay100MsAsync(ct);
            }
        }

        /// <summary>
        /// 周辺車両に魚の載せてしばらくたったら発射する
        /// </summary>
        private async ValueTask CitizenVehicleAsync(Vehicle veh, CancellationToken ct)
        {
            await YieldAsync(ct);
            if (!veh.IsSafeExist())
            {
                return;
            }

            var f = SpawnFish(veh);
            if (!f.IsSafeExist())
            {
                return;
            }

            float targetTime = Random.Next(1, 10);
            while (targetTime > 0 && !ct.IsCancellationRequested)
            {
                targetTime -= core.DeltaTime;

                //プレイヤが途中で乗り込んだら直ちに発射
                if (veh.IsSafeExist() && veh == core.PlayerPed.CurrentVehicle)
                {
                    break;
                }

                await Delay100MsAsync(ct);
            }


            if (veh.IsSafeExist())
            {
                ShootFishAsync(veh, f, ct).Forget();
            }
        }

        /// <summary>
        /// プレイヤが車に乗ったか監視する
        /// </summary>
        private async ValueTask ObservePlayerAsync(CancellationToken ct)
        {
            while (IsActive && !ct.IsCancellationRequested)
            {
                var playerVehicle = core.GetPlayerVehicle();
                //プレイヤが車に乗ったか監視
                while (!playerVehicle.IsSafeExist())
                {
                    await Delay100MsAsync(ct);
                }

                //発射したら1秒まってもう一度まつ
                await PlayerVehicleAsync(playerVehicle, ct);
                await DelaySecondsAsync(1, ct);
            }
        }

        private async ValueTask PlayerVehicleAsync(Vehicle playerVehicle, CancellationToken ct)
        {
            //車に乗ったら魚生成
            var p = SpawnFish(playerVehicle);
            if (!p.IsSafeExist())
            {
                return;
            }

            //キー入力待機
            while (!core.IsGamePadPressed(GameKey.VehicleHorn))
            {
                //途中で車を降りたら発射
                if (!core.PlayerPed.IsInVehicle())
                {
                    ShootFishAsync(core.PlayerPed.CurrentVehicle, p, ct).Forget();
                    return;
                }

                if (!IsActive)
                {
                    return;
                }

                await Delay100MsAsync(ct);
            }

            //魚発射
            await ShootFishAsync(core.PlayerPed.CurrentVehicle, p, ct);
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
        private async ValueTask ShootFishAsync(Vehicle veh, Ped fish, CancellationToken ct)
        {
            if (!fish.IsSafeExist())
            {
                if (veh.IsSafeExist())
                {
                    vehicles.Remove(veh.Handle);
                }

                return;
            }

            fish.Detach();
            fish.IsInvincible = false;
            fish.RequestCollision();
            fish.FreezePosition(false);
            CreateEffect(fish, "ent_sht_electrical_box");

            await YieldAsync(ct);

            if (fish.IsSafeExist())
            {
                fish.Velocity = fish.ForwardVector * (100 + veh.Speed);
            }

            //速度が一定以下になったら爆発
            var targetTime = core.ElapsedTime + 10;
            while (core.ElapsedTime < targetTime && !ct.IsCancellationRequested)
            {
                if (!fish.IsSafeExist())
                {
                    if (veh.IsSafeExist())
                    {
                        vehicles.Remove(veh.Handle);
                    }

                    return;
                }

                if (fish.Velocity.Length() < 6)
                {
                    break;
                }

                await YieldAsync(ct);
            }

            if (veh.IsSafeExist())
            {
                vehicles.Remove(veh.Handle);
            }

            if (!fish.IsSafeExist())
            {
                return;
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