using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;

namespace Inferno.InfernoScripts.World
{
    internal class ChaosHeli : InfernoScript
    {
        private readonly WeaponHash[] _driveByWeapons =
        {
            WeaponHash.Pistol,
            WeaponHash.APPistol,
            WeaponHash.CombatPistol,
            WeaponHash.HeavyPistol,
            WeaponHash.Pistol50,
            WeaponHash.FlareGun,
            WeaponHash.Revolver,
            WeaponHash.MicroSMG,
            WeaponHash.MachinePistol,
            WeaponHash.CompactRifle,
            WeaponHash.SawnOffShotgun,
            WeaponHash.DoubleBarrelShotgun,
            WeaponHash.StunGun,
            WeaponHash.Minigun,
            WeaponHash.AssaultShotgun,
            WeaponHash.CompactGrenadeLauncher,
            WeaponHash.CompactEMPLauncher,
            WeaponHash.PistolMk2,
            WeaponHash.SMGMk2
        };

        private readonly VehicleHash[] Helis = new[]
        {
            VehicleHash.Akula,
            VehicleHash.Buzzard,
            VehicleHash.Buzzard2,
            VehicleHash.Cargobob,
            VehicleHash.Cargobob2,
            VehicleHash.Cargobob3,
            VehicleHash.Cargobob4,
            VehicleHash.Conada,
            VehicleHash.Frogger,
            VehicleHash.Hunter,
            VehicleHash.Maverick,
            VehicleHash.Havok,
            VehicleHash.Volatus
        };

        //ヘリのドライバー以外の座席
        private readonly List<VehicleSeat> _vehicleSeat = new()
            { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        private Vehicle _heli;
        private Ped _heliDriver;
        private bool _isNearPlayer = false;
        private CancellationTokenSource _heliCts;
        private Blip _heliBlip = null;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("ChaosHeli:" + (IsActive ? "ON" : "OFF"));
                });

            OnAllOnCommandObservable
                .Subscribe(_ => IsActive = true);

            IsActiveRP.Subscribe(_ =>
            {
                _heliCts?.Cancel();
                _heliCts?.Dispose();
                _heliCts = null;
                if (_heliBlip?.Exists() ?? false)
                {
                    _heliBlip.Delete();
                }

                if (IsActive)
                {
                    ResetHeli();
                    ObserveHeliAsync(ActivationCancellationToken).Forget();
                    ObservePlayerAsync(ActivationCancellationToken).Forget();
                }
                else
                {
                    ReleasePedAndHeli();
                }
            });


            OnAbortAsync
                .Subscribe(_ =>
                {
                    if (_heliBlip?.Exists() ?? false)
                    {
                        _heliBlip.Delete();
                    }

                    ReleasePedAndHeli();
                });
        }

        /// <summary>
        /// プレイヤを監視する
        /// </summary>
        /// <returns></returns>
        private async ValueTask ObservePlayerAsync(CancellationToken ct)
        {
            var playerIsDead = true;
            while (IsActive && !ct.IsCancellationRequested)
            {
                if (PlayerPed.IsSafeExist())
                {
                    if (playerIsDead && PlayerPed.IsAlive)
                    {
                        playerIsDead = false;
                        ResetHeli();
                    }
                    else if (PlayerPed.IsDead)
                    {
                        playerIsDead = true;
                    }
                }

                await DelaySecondsAsync(2f, ct);
            }
        }

        /// <summary>
        /// ヘリが追いつけない状態になっていないか監視する
        /// </summary>
        /// <returns></returns>
        private async ValueTask ObserveHeliAsync(CancellationToken ct)
        {
            while (IsActive && !ct.IsCancellationRequested)
            {
                if ((PlayerPed.IsSafeExist() && !_heli.IsSafeExist()) || _heli.IsDead ||
                    !_heli.IsInRangeOfIgnoreZ(PlayerPed.Position, 200.0f))
                {
                    ResetHeli();
                }

                await DelaySecondsAsync(40, ct);
            }
        }

        private async ValueTask ChaosHeliAsync(CancellationToken ct)
        {
            await DelayRandomSecondsAsync(0, 2, ct);

            //ヘリが存在かつMODが有効の間回り続ける
            while (IsActive && _heli.IsSafeExist() && _heli.IsAlive)
            {
                if (!PlayerPed.IsSafeExist())
                {
                    break;
                }

                var targetPosition = PlayerPed.Position.Around(20) + new Vector3(0, 0, 10);

                //ヘリがプレイヤから離れすぎていた場合は追いかける
                MoveHeli(_heliDriver, targetPosition);

                SpawnPassengersToEmptySeat();

                await DelaySecondsAsync(5, ct);
            }

            ReleasePedAndHeli();
        }


        /// <summary>
        /// ヘリを指定座標に移動させる
        /// </summary>
        /// <param name="heliDriver">ドライバ</param>
        /// <param name="targetPosition">目標地点</param>
        private void MoveHeli(Ped heliDriver, Vector3 targetPosition)
        {
            if (!_heli.IsSafeExist() || !heliDriver.IsSafeExist() || !heliDriver.IsAlive)
            {
                return;
            }

            if (_heli.IsInRangeOfIgnoreZ(targetPosition, 70))
            {
                //プレイヤに近い場合は攻撃する

                // フラグが切り替わったときに実行
                if (!_isNearPlayer)
                {
                    _heliDriver.Task.ClearAll();
                    FightAgainstNearPeds(_heliDriver);
                    // 車で攻撃するか
                    _heliDriver.SetCombatAttributes(52, true);
                    // 車両の武器を使用するか
                    _heliDriver.SetCombatAttributes(53, true);

                    _isNearPlayer = true;
                }
            }
            else
            {
                if (_isNearPlayer)
                {
                    _heliDriver.Task.ClearAll();
                    _isNearPlayer = false;
                }

                _heli.DriveTo(_heliDriver, targetPosition, 100, DrivingStyle.IgnoreLights);
            }
        }


        /// <summary>
        /// ヘリのリセット
        /// </summary>
        private void ResetHeli()
        {
            _heliCts?.Cancel();
            _heliCts?.Dispose();
            _heliCts = new CancellationTokenSource();
            if (_heliBlip?.Exists() ?? false)
            {
                _heliBlip.Delete();
            }

            ReleasePedAndHeli();
            CreateChaosHeli();
        }

        /// <summary>
        /// 同乗者生成
        /// </summary>
        private void SpawnPassengersToEmptySeat()
        {
            foreach (var seat in _vehicleSeat)
            {
                //ヘリが存在し座席に誰もいなかったら市民再生成
                if (_heli.IsSafeExist() && _heli.IsSeatFree(seat))
                {
                    CreatePassenger(seat);
                }
            }
        }

        /// <summary>
        /// カオスヘリ生成
        /// </summary>
        private void CreateChaosHeli()
        {
            try
            {
                if (!PlayerPed.IsSafeExist())
                {
                    return;
                }

                var player = PlayerPed;
                var playerPosition = player.Position;
                var spawnHeliPosition = playerPosition.Around(100) + new Vector3(0, 0, 40);
                var heli = GTA.World.CreateVehicle(Helis[Random.Next(0, Helis.Length)], spawnHeliPosition);
                if (!heli.IsSafeExist())
                {
                    return;
                }

                AutoReleaseOnGameEnd(heli);
                heli.SetProofs(false, false, false, false, false, false, false, false);
                heli.MaxHealth = 3000;
                heli.Health = 3000;
                _heli = heli;

                _heliBlip = _heli.AddBlip();
                if (_heliBlip?.Exists() ?? false)
                {
                    _heliBlip.Color = BlipColor.GreyDark;
                }

                _heliDriver = GTA.World.CreatePed(new Model(PedHash.LamarDavis), _heli.Position + Vector3.WorldUp * 10);
                if (_heliDriver.IsSafeExist() && _heliDriver.IsHuman)
                {
                    _heliDriver.SetIntoVehicle(_heli, VehicleSeat.Driver);
                    _heliDriver.SetProofs(true, true, true, true, true, true, true, true);
                    _heliDriver.SetNotChaosPed(true);
                    // 車で攻撃するか
                    _heliDriver.SetCombatAttributes(52, true);
                    // 車両の武器を使用するか
                    _heliDriver.SetCombatAttributes(53, true);
                }

                SpawnPassengersToEmptySeat();

                //カオスヘリ
                ChaosHeliAsync(_heliCts.Token).Forget();
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }
        }

        /// <summary>
        /// ヘリとドライバーと同乗者の開放
        /// </summary>
        private void ReleasePedAndHeli()
        {
            //ヘリの解放前に座席に座っている市民を解放する
            foreach (var seat in _vehicleSeat)
            {
                if (_heli.IsSafeExist())
                {
                    //座席にいる市民取得
                    var ped = _heli.GetPedOnSeat(seat);
                    if (ped.IsSafeExist())
                    {
                        ped.MarkAsNoLongerNeeded();
                    }
                }
            }

            //ドライバー開放
            if (_heliDriver.IsSafeExist())
            {
                //無敵化解除
                _heliDriver.SetProofs(false, false, false, false, false, false, false, false);
                //ヘリのドライバー解放
                _heliDriver.MarkAsNoLongerNeeded();
            }

            if (_heli.IsSafeExist())
            {
                //ヘリ解放
                _heli.SetProofs(false, false, false, false, false, false, false, false);
                _heli.PetrolTankHealth = -100;
                _heli.MarkAsNoLongerNeeded();
            }
        }

        /// <summary>
        /// 同乗者作成
        /// </summary>
        /// <param name="seat"></param>
        private void CreatePassenger(VehicleSeat seat)
        {
            if (!_heli.IsSafeExist())
            {
                return;
            }

            var p = _heli.CreatePedOnSeat(seat, new Model(PedHash.LamarDavis02));
            if (p.IsSafeExist())
            {
                AutoReleaseOnGameEnd(p);

                p.SetNotChaosPed(true);
                // ドライブバイを許可するか
                p.SetCombatAttributes(2, true);
                // 車から降りることができるか
                p.SetCombatAttributes(3, true);

                p.Task.ClearAll();
                p.Weapons.Give(_driveByWeapons[Random.Next(0, _driveByWeapons.Length)], 999, false, true);
                // 最適な武器を選択するか
                p.SetCombatAttributes(54, true);

                FightAgainstNearPeds(p);
                p.Accuracy = 5;
            }
        }

        private void FightAgainstNearPeds(Ped p)
        {
            foreach (var target in CachedPeds.Where(x =>
                         x.IsSafeExist() && x != p && x.IsAlive && x.IsInRangeOfIgnoreZ(p.Position, 100)))
            {
                if (CachedMissionEntities.Value.Any(x => x.Position.DistanceTo2D(target.Position) < 30.0f))
                {
                    continue;
                }

                p.Task.FightAgainst(target);
            }

            p.Task.FightAgainst(PlayerPed);
        }
        
        
        public override bool UseUI => true;
        public override string DisplayText => IsLangJpn ? "カオスヘリ" : "Harassing helicopter";

        public override bool CanChangeActive => true;
        public override MenuIndex MenuIndex => MenuIndex.World;
    }
}