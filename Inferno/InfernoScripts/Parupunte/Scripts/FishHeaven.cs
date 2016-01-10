using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;

namespace Inferno.InfernoScripts.Parupunte.Scripts
{
    [ParupunteDebug(false,true)]
    internal class FishHeaven : ParupunteScript
    {
        private HashSet<int> vehicles = new HashSet<int>();
        private Model fishModel = new Model(PedHash.TigerShark);
        private Ped playerFish;

        public FishHeaven(ParupunteCore core) : base(core)
        {
        }

        public override string Name { get; } = "おさかな天国";

        public override void OnStart()
        {
            var ptfxName = "core";
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, ptfxName))
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, ptfxName);
            }

            ReduceCounter = new ReduceCounter(30*1000);
            AddProgressBar(ReduceCounter);
            ReduceCounter.OnFinishedAsync.Subscribe(_ => ParupunteEnd());

            this.UpdateAsObservable
                .Where(_ => core.IsGamePadPressed(GameKey.VehicleHorn))
                .Where(_ => playerFish.IsSafeExist())
                .Subscribe(_ =>
                {
                    StartCoroutine(ShootFish(playerFish));
                    playerFish = null;
                });

            this.UpdateAsObservable
                .Where(_ => core.PlayerPed.CurrentVehicle.IsSafeExist() && !playerFish.IsSafeExist())
                .ThrottleFirst(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    playerFish = SpawnFish(core.PlayerPed.CurrentVehicle);
                });
        }

        protected override void OnUpdate()
        {
            var playerPos = core.PlayerPed.Position;
            foreach (var v in core.CachedVehicles.Where(
                x => x.IsSafeExist() && x.IsInRangeOf(playerPos, 50) && !vehicles.Contains(x.Handle)))
            {
                vehicles.Add(v.Handle);
                SpawnFish(v);
            }

        }

        private Ped SpawnFish(Vehicle target)
        {
            var f = GTA.World.CreatePed(fishModel, target.Position + Vector3.WorldUp*10);
            if (!f.IsSafeExist() || !target.IsSafeExist()) return null;
            f.MarkAsNoLongerNeeded();
            f.AttachTo(target, 0, Vector3.WorldUp*1, target.ForwardVector);
            f.IsInvincible = true;
            f.Health = 100;
            return f;
        }

        private IEnumerable<object> ShootFish(Ped fish)
        {
            if(!fish.IsSafeExist()) yield break;
            fish.Detach();
            fish.ApplyForce(fish.ForwardVector*100);
            CreateEffect(fish, "ent_sht_electrical_box");
            foreach (var x in WaitForSeconds(3))
            {
                if (!fish.IsSafeExist()) yield break;
                
                yield return null;
            }
            if (!fish.IsSafeExist()) yield break;
            GTA.World.AddExplosion(fish.Position, GTA.ExplosionType.Grenade, 1.0f, 1.0f);
        }

        private void CreateEffect(Ped ped, string effect)
        {
            if (!ped.IsSafeExist()) return;
            var offset = new Vector3(0.2f, 0.0f, 0.0f);
            var rotation = new Vector3(80.0f, 10.0f, 0.0f);
            var scale = 3.0f;
            Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "core");
            Function.Call<int>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, effect,
                ped, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, (int)Bone.SKEL_Pelvis, scale, 0, 0, 0);
        }
    }

}
