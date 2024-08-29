using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.ChaosMode.WeaponProvider;
using Inferno.InfernoScripts.InfernoCore.Coroutine;
using Inferno.Utilities;

namespace Inferno
{
    internal class Fulton : InfernoScript
    {
        /// <summary>
        /// フルトン回収のコルーチン対象になっているEntity
        /// </summary>
        private readonly HashSet<int> fulutonedEntityList = new();

        private readonly Queue<PedHash> motherBasePeds = new(30);
        private readonly Queue<VehicleHash> motherbaseVeh = new(30);
        private Random random = new();

        /// <summary>
        /// 空に飛んで行く音
        /// </summary>
        private SoundPlayer soundPlayerMove;

        /// <summary>
        /// フルトン回収で人を吊り下げた時の音
        /// </summary>
        private SoundPlayer soundPlayerPedSetup;

        /// <summary>
        /// フルトン回収で車を吊り下げた時の音
        /// </summary>
        private SoundPlayer soundPlayerVehicleSetup;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("fulton")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("Fulton:" + IsActive);

                    if (IsActive)
                    {
                        PlayerPed.GiveWeapon((int)Weapon.STUNGUN, 1);

                        FulutonUpdateLoopAsync(ActivationCancellationToken).Forget();
                    }
                });

             OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F9 && motherbaseVeh.Count > 0)
                .Subscribe(_ => SpawnVehicle());

            OnKeyDownAsObservable
                .Where(x => IsActive && x.KeyCode == Keys.F10 && motherBasePeds.Count > 0)
                .Subscribe(_ => SpawnCitizen());

            //プレイヤが死んだらリストクリア
            OnThinnedTickAsObservable
                .Select(_ => PlayerPed.IsDead)
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ =>
                {
                    if (IsActive)
                    {
                        PlayerPed.GiveWeapon((int)Weapon.STUNGUN, 1);
                    }

                    fulutonedEntityList.Clear();
                });
            SetUpSound();
        }

        /// <summary>
        /// 効果音のロード
        /// </summary>
        private void SetUpSound()
        {
            var filePaths = LoadWavFiles(@"scripts/InfernoSEs");
            var setupWav = filePaths.FirstOrDefault(x => x.Contains("vehicle.wav"));
            if (setupWav != null)
            {
                soundPlayerVehicleSetup = new SoundPlayer(setupWav);
            }

            setupWav = filePaths.FirstOrDefault(x => x.Contains("ped.wav"));
            if (setupWav != null)
            {
                soundPlayerPedSetup = new SoundPlayer(setupWav);
            }

            var moveWav = filePaths.FirstOrDefault(x => x.Contains("move.wav"));
            if (moveWav != null)
            {
                soundPlayerMove = new SoundPlayer(moveWav);
            }
        }

        private string[] LoadWavFiles(string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(targetPath).Where(x => Path.GetExtension(x) == ".wav").ToArray();
        }

        #region 回収

        private async ValueTask FulutonUpdateLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (Function.Call<bool>(Hash.IS_CUTSCENE_ACTIVE))
                {
                    // ここの待機はフレーム数を気にしなくていいのでTask.DelayでOK
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    continue;
                }

                foreach (var entity in CachedEntities
                             .Where(
                                 x => x.IsSafeExist()
                                      && x.IsInRangeOf(PlayerPed.Position, 15.0f)
                                      && !fulutonedEntityList.Contains(x.Handle)
                                      && x.IsAlive
                             ))
                {
                    if (entity.HasBeenDamagedByPed(PlayerPed) && (
                            entity.HasBeenDamagedBy(Weapon.UNARMED) || entity.HasBeenDamagedBy(Weapon.STUNGUN)
                        ))
                    {
                        fulutonedEntityList.Add(entity.Handle);
                        FulutonAsync(entity, ct).Forget();
                        if (entity is Vehicle)
                        {
                            soundPlayerVehicleSetup?.Play();
                        }
                        else
                            //pedの時は遅延させてならす
                        {
                            Observable.Timer(TimeSpan.FromSeconds(0.3f))
                                .Subscribe(_ => soundPlayerPedSetup?.Play())
                                .AddTo(CompositeDisposable);
                        }
                    }
                }

                await DelaySecondsAsync(0.25f, ct);
            }
        }

        private void LeaveAllPedsFromVehicle(Vehicle vec)
        {
            if (!vec.IsSafeExist())
            {
                return;
            }

            foreach (
                var seat in
                new[] { VehicleSeat.Driver, VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear })
            {
                var ped = vec.GetPedOnSeat(seat);
                if (ped.IsSafeExist())
                {
                    ped.Task.ClearAll();
                    ped.Task.ClearSecondary();
                    ped.Task.LeaveVehicle();
                }
            }
        }

        private async ValueTask FulutonAsync(Entity entity, CancellationToken ct)
        {
            //Entityが消え去った後に処理したいので先に情報を保存しておく
            var hash = -1;
            var isPed = false;

            var upForce = new Vector3(0, 0, 1);
            if (entity is Ped)
            {
                var p = entity as Ped;
                p.CanRagdoll = true;
                p.SetToRagdoll();

                isPed = true;
            }
            else
            {
                var v = entity as Vehicle;
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, v, 1);
                LeaveAllPedsFromVehicle(v);
            }

            hash = entity.Model.Hash;
            
            var time = 0f;
            while (time < 3.0f)
            {
                time += DeltaTime;
                if (!entity.IsSafeExist() || entity.IsDead)
                {
                    return;
                }

                entity.ApplyForce(upForce * 0.3f);

                await YieldAsync(ct);
            }


            if (!entity.IsSafeExist() || entity.IsRequiredForMission())
            {
                return;
            }

            if (PlayerPed.CurrentVehicle.IsSafeExist() && PlayerPed.CurrentVehicle.Handle == entity.Handle)
            {
                return;
            }

            //弾みをつける
            await DelaySecondsAsync(0.25f, ct);
            soundPlayerMove?.Play();

            time = 0f;
            while (time < 15.0f)
            {
                time += DeltaTime;


                if (!entity.IsSafeExist() || entity.Position.DistanceTo(PlayerPed.Position) > 100)
                {
                    if (PlayerPed.CurrentVehicle.IsSafeExist() && PlayerPed.CurrentVehicle.Handle == entity.Handle)
                    {
                        return;
                    }

                    if (isPed)
                    {
                        motherBasePeds.Enqueue((PedHash)hash);
                        Game.Player.Money -= 100;
                        if (entity.IsSafeExist())
                        {
                            entity.Delete();
                        }
                    }
                    else
                    {
                        motherbaseVeh.Enqueue((VehicleHash)hash);
                        Game.Player.Money -= 1000;
                    }

                    DrawText($"回収済: {motherBasePeds.Count}人/{motherbaseVeh.Count}台");
                    return;
                }

                if (entity.IsDead)
                {
                    return;
                }

                var force = upForce * 1.0f / Game.FPS * 500.0f;
                if (entity is Ped)
                {
                    force = upForce * 1.0f / Game.FPS * 800.0f;
                }

                entity.ApplyForce(force);

                await YieldAsync(ct);
            }
        }

        #endregion 回収

        #region 生成

        private void SpawnCitizen()
        {
            var hash = motherBasePeds.Dequeue();
            DrawText($"残り: {motherBasePeds.Count}人/{motherbaseVeh.Count}台");

            var p = World.CreatePed(new Model(hash), PlayerPed.Position.AroundRandom2D(3.0f) + new Vector3(0, 0, 0.5f));
            if (!p.IsSafeExist())
            {
                return;
            }

            p.SetNotChaosPed(true);
            PlayerPed.PedGroup.Add(p, false);
            p.MaxHealth = 500;
            p.Health = p.MaxHealth;

            var weaponhash = (int)ChaosModeWeapons.GetRandomWeapon();
            p.SetDropWeaponWhenDead(false); //武器を落とさない
            p.GiveWeapon(weaponhash, 1000); //指定武器所持
            p.EquipWeapon(weaponhash); //武器装備
            FriendAsync(p, ActivationCancellationToken).Forget();
        }

        /// <summary>
        /// 生成した味方を監視するコルーチン
        /// </summary>
        private async ValueTask FriendAsync(Ped ped, CancellationToken ct)
        {
            try
            {
                while (ped.IsSafeExist() && ped.IsAlive && ped.IsInRangeOf(PlayerPed.Position, 100))
                {
                    await DelaySecondsAsync(1, ct);
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.MarkAsNoLongerNeeded();
                }
            }
        }

        private void SpawnVehicle()
        {
            var hash = motherbaseVeh.Dequeue();
            var vehicleGxtEntry = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, (int)hash);
            DrawText(NativeFunctions.GetGXTEntry(vehicleGxtEntry));
            StartCoroutine(SpawnVehicleCoroutine(new Model(hash), PlayerPed.Position.AroundRandom2D(20)));
        }

        private IEnumerable<object> SpawnVehicleCoroutine(Model model, Vector3 targetPosition)
        {
            var car = World.CreateVehicle(model, targetPosition + new Vector3(0, 0, 20));
            if (!car.IsSafeExist())
            {
                yield break;
            }

            var upVector = new Vector3(0, 0, 1.0f);
            car.FreezePosition(false);
            car.Velocity = new Vector3();
            World.AddExplosion(targetPosition, GTA.ExplosionType.Flare, 1.0f, 0.0f);

            foreach (var s in WaitForSeconds(10))
            {
                if (!car.IsSafeExist())
                {
                    yield break;
                }

                car.ApplyForce(upVector);
                if (!car.IsInAir)
                {
                    break;
                }

                yield return null;
            }

            if (!car.IsSafeExist())
            {
                yield break;
            }

            car.MarkAsNoLongerNeeded();
        }

        #endregion 生成
    }
}