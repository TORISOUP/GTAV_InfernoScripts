using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using UniRx;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.World
{
    /// <summary>
    /// 人はバットで殴られると爆発する
    /// </summary>
    class BombBat : InfernoScript
    {
        private readonly string Keyword = "batman";
        private List<Ped> pPed = new List<Ped>();

        protected override void Setup()
        {
            pPed.Add(PlayerPed);

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("BombBat:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //interval間隔で実行
            OnThinnedTickAsObservable
                .Where(_ => IsActive)
                .Subscribe(p =>
                {
                    foreach (var ped in CachedPeds)
                    {
                        if (!ped.IsSafeExist()) continue;
                        BomBatAction(ped);
                    }

                    BomBatAction(PlayerPed);
                });
        }

        private void BomBatAction(Ped ped)
        {

            if (ped.HasBeenDamagedBy(Weapon.BAT))
            {

                GTA.World.AddExplosion(ped.Position + Vector3.WorldUp * 0.5f, GTA.ExplosionType.Grenade, 40.0f,
                    0.5f);


                var randomVector = InfernoUtilities.CreateRandomVector();
                ped.ApplyForce(randomVector * Random.Next(10, 20));
                ped.Kill();
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
            else if (ped.HasBeenDamagedBy(Weapon.KNIFE))
            {
                GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Molotov1, 0.1f, 0.0f);
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
            else if (ped.HasBeenDamagedBy(Weapon.GOLFCLUB))
            {
                var randomVector = InfernoUtilities.CreateRandomVector();
                ped.SetToRagdoll(100);
                ped.Velocity = randomVector * 1000;
                ped.ApplyForce(randomVector * Random.Next(2000, 4000));
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
            else if (ped.HasBeenDamagedBy(Weapon.UNARMED) /*&& !ped.HasBeenDamagedByPed(PlayerPed)*/)
            {
                NativeFunctions.ShootSingleBulletBetweenCoords(
                    ped.Position + new Vector3(0, 0, 1),
                    ped.GetBoneCoord(Bone.IK_Head), 1, WeaponHash.StunGun, null, 1.0f);
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
            else if (ped.HasBeenDamagedBy(Weapon.HAMMER))
            {
                if (!ped.IsInRangeOf(PlayerPed.Position, 10)) return;
                Shock(ped);
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);

            }
            else if (ped.HasBeenDamagedBy(Weapon.Poolcue))
            {
                var blowVector = -ped.ForwardVector;
                ped.SetToRagdoll(1);
                ped.Velocity = blowVector * 10;
                ped.ApplyForce(blowVector * Random.Next(20, 40));
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
            }
        }


        private void Shock(Ped damagedPed)
        {
            var playerPos = PlayerPed.Position;
            GTA.World.AddExplosion(playerPos + Vector3.WorldUp * 10, GTA.ExplosionType.Grenade, 0.01f, 0.2f);
            #region Ped
            foreach (var p in CachedPeds.Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(damagedPed.Position, 10)
                     && x.IsAlive
                     && !x.IsCutsceneOnlyPed()))
            {
                p.SetToRagdoll();
                p.ApplyForce(Vector3.WorldUp * 5f);
            }

            #endregion

            #region Vehicle

            foreach (var v in CachedVehicles.Where(
                x => x.IsSafeExist()
                     && x.IsInRangeOf(damagedPed.Position, 10)
                     && x.IsAlive))
            {
                v.ApplyForce(Vector3.WorldUp * 5f, Vector3.RandomXYZ());
            }

            #endregion

        }
    }
}