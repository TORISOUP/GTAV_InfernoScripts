﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Properties;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.World
{
    /// <summary>
    /// 人はバットで殴られると爆発する
    /// </summary>
    internal class BombBat : InfernoScript
    {
        private readonly string Keyword = "batman";
        private readonly List<Ped> _startedPeds = new();

        protected override void Setup()
        {
            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable("BombBat", Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("BombBat:" + IsActive);
                });

            IsActiveRP.Subscribe(_ => _startedPeds.Clear());

            CreateTickAsObservable(TimeSpan.FromSeconds(1))
                .Where(_ => IsActive)
                .Subscribe(p =>
                {
                    foreach (var ped in CachedPeds.Where(x =>
                                 x.IsSafeExist() && x.IsAlive && !_startedPeds.Contains(x) &&
                                 x.IsInRangeOf(PlayerPed.Position, 30)))
                    {
                        _startedPeds.Add(ped);
                        ObservePedDamageAsync(ped, ActivationCancellationToken).Forget();
                    }

                    if (PlayerPed.IsSafeExist() && PlayerPed.IsAlive && !_startedPeds.Contains(PlayerPed))
                    {
                        _startedPeds.Add(PlayerPed);
                        ObservePedDamageAsync(PlayerPed, ActivationCancellationToken).Forget();
                    }
                });
        }

        private async ValueTask ObservePedDamageAsync(Ped ped, CancellationToken ct)
        {
            try
            {
                // 分散
                await DelayRandomFrameAsync(1, 30, ct);
                while (ped.IsSafeExist() && ped.IsAlive && !ct.IsCancellationRequested &&
                       PlayerPed.IsSafeExist() && ped.IsInRangeOf(PlayerPed.Position, 35))
                {
                    if (ped.HasBeenDamagedByAnyMeleeWeapon())
                    {
                        if (ped.HasBeenDamagedBy(Weapon.Bat))
                        {
                            GTA.World.AddExplosion(ped.Position + Vector3.WorldUp * 0.5f, GTA.ExplosionType.Grenade,
                                40.0f,
                                0.5f);

                            var randomVector = InfernoUtilities.CreateRandomVector();
                            ped.ApplyForce(randomVector * Random.Next(10, 20));
                            ped.Kill();
                            ped.ClearLastWeaponDamage();
                            return;
                        }
                        else if (ped.HasBeenDamagedBy(Weapon.Knife) || ped.HasBeenDamagedBy(Weapon.Battleaxe) ||
                                 ped.HasBeenDamagedBy(Weapon.Dagger) || ped.HasBeenDamagedBy(Weapon.Hatchet))
                        {
                            GTA.World.AddExplosion(ped.Position, GTA.ExplosionType.Molotov1, 0.1f, 0.0f);
                        }
                        else if (ped.HasBeenDamagedBy(Weapon.GolfClub))
                        {
                            var randomVector = InfernoUtilities.CreateRandomVector();
                            ped.SetToRagdoll(100);
                            ped.Velocity = randomVector * 1000;
                            ped.ApplyForce(randomVector * Random.Next(2000, 4000));
                        }
                        else if (ped.HasBeenDamagedBy(Weapon.Hammer))
                        {
                            if (!ped.IsInRangeOf(PlayerPed.Position, 10))
                            {
                                await YieldAsync(ct);
                                continue;
                            }

                            Shock(ped);
                        }
                        else if (ped.HasBeenDamagedBy(Weapon.Poolcue))
                        {
                            var blowVector = -ped.ForwardVector;
                            ped.SetToRagdoll(1);
                            ped.Velocity = blowVector * 10;
                            ped.ApplyForce(blowVector * Random.Next(20, 40));
                        }
                        else
                        {
                            var damanagedBone = ped.Bones.LastDamaged.Position;

                            NativeFunctions.ShootSingleBulletBetweenCoords(
                                start: damanagedBone,
                                end: ped.Position,
                                damage: 1,
                                weapon: WeaponHash.StunGun, null, 1.0f);
                        }


                        // 少し待ってからフラグをクリアする
                        await DelaySecondsAsync(1, ct);
                        if (ped.IsSafeExist())
                        {
                            ped.ClearLastWeaponDamage();
                        }
                    }

                    await DelayFrameAsync(3, ct);
                }
            }
            finally
            {
                _startedPeds.Remove(ped);
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

        public override bool UseUI => true;
        public override string DisplayName => BombBatLocalize.Name;
        
        public override string Description => BombBatLocalize.Description;

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.World;
    }
}