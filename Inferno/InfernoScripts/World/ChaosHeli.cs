using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;

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
            VehicleHash.Volatol
        };

        private readonly List<uint> coroutineIds = new();

        private readonly HashSet<Ped> raperingPedList = new();

        //ヘリのドライバー以外の座席
        private readonly List<VehicleSeat> vehicleSeat = new()
            { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        private Vehicle _heli;
        private Ped _heliDriver;
        private uint _observeHeliCoroutineId;
        private uint _observePlayerCoroutineId;
        private bool _isNearPlayer = false;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    StopAllChaosHeliCoroutine();
                    DrawText("ChaosHeli:" + (IsActive ? "ON" : "OFF"));
                    if (IsActive)
                    {
                        ResetHeli();
                        _observeHeliCoroutineId = StartCoroutine(ObserveHeliCoroutine());
                        _observePlayerCoroutineId = StartCoroutine(ObservePlayerCoroutine());
                    }
                    else
                    {
                        ReleasePedAndHeli();
                        StopCoroutine(_observeHeliCoroutineId);
                        StopCoroutine(_observePlayerCoroutineId);
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    IsActive = true;
                    if (_heli.IsSafeExist())
                    {
                        return;
                    }

                    ResetHeli();
                });

            OnAbortAsync
                .Subscribe(_ => ReleasePedAndHeli());
        }

        /// <summary>
        /// プレイヤを監視するコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerable<object> ObservePlayerCoroutine()
        {
            while (IsActive)
            {
                if (PlayerPed.IsSafeExist() && !PlayerPed.IsAlive)
                {
                    ResetHeli();
                }

                yield return WaitForSeconds(2);
            }
        }

        /// <summary>
        /// ヘリが追いつけない状態になっていないか監視するコルーチン
        /// </summary>
        /// <returns></returns>
        private IEnumerable<object> ObserveHeliCoroutine()
        {
            while (IsActive)
            {
                if ((PlayerPed.IsSafeExist() && !_heli.IsSafeExist()) || _heli.IsDead ||
                    !_heli.IsInRangeOf(PlayerPed.Position, 200.0f))
                {
                    ResetHeli();
                }

                yield return WaitForSeconds(40);
            }
        }

        private IEnumerable<object> ChaosHeliCoroutine()
        {
            yield return RandomWait();

            //ヘリが存在かつMODが有効の間回り続けるコルーチン
            while (IsActive && _heli.IsSafeExist() && _heli.IsAlive)
            {
                if (!PlayerPed.IsSafeExist())
                {
                    break;
                }

                var targetPosition = PlayerPed.Position + new Vector3(0, 0, 10);

                //ヘリがプレイヤから離れすぎていた場合は追いかける
                MoveHeli(_heliDriver, targetPosition);

                SpawnPassengersToEmptySeat();

                yield return WaitForSeconds(1);
            }

            ReleasePedAndHeli();
        }

        /// <summary>
        /// 現在ラペリング可能な状態であるか調べる
        /// </summary>
        /// <returns>trueでラペリング許可</returns>
        private bool CheckRapeling(Vehicle heli, VehicleSeat seat)
        {
            //ヘリが壊れていたり存在しないならラペリングできない
            if (!heli.IsSafeExist() || heli.IsDead)
            {
                return false;
            }

            //ヘリが早過ぎる場合はラペリングしない
            if (heli.Velocity.Length() > 30)
            {
                return false;
            }

            //助手席はラペリングできない
            if (seat == VehicleSeat.Passenger)
            {
                return false;
            }

            //現在ラペリング中ならできない
            var ped = heli.GetPedOnSeat(seat);
            if (!ped.IsSafeExist() || !ped.IsHuman || !ped.IsAlive || !PlayerPed.IsSafeExist())
            {
                return false;
            }

            var playerPosition = PlayerPed.Position;

            //プレイヤの近くならラペリングする
            return heli.IsInRangeOf(playerPosition, 50.0f);
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

            if (_heli.IsInRangeOf(targetPosition, 30))
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
                    _heli.DriveTo(_heliDriver, targetPosition, 100, DrivingStyle.IgnoreLights);
                    _isNearPlayer = false;
                }
            }
        }

        /// <summary>
        /// ラペリング中の市民を監視するコルーチン
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<object> PassengerRapeling(Ped ped, VehicleSeat seat)
        {
            if (!ped.IsSafeExist())
            {
                yield break;
            }

            //ラペリング降下させる
            ped.TaskRappelFromHeli();
            //降下中は無敵
            ped.IsInvincible = true;

            //一定時間降下するのを見守る
            for (var i = 0; i < 10; i++)
            {
                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist() || ped.IsDead)
                {
                    break;
                }

                yield return WaitForSeconds(1);
            }

            if (!ped.IsSafeExist())
            {
                yield break;
            }

            ped.IsInvincible = false;
            ped.Health = 100;
            ped.MarkAsNoLongerNeeded();
        }

        /// <summary>
        /// ヘリのリセット
        /// </summary>
        private void ResetHeli()
        {
            StopAllChaosHeliCoroutine();
            ReleasePedAndHeli();
            CreateChaosHeli();
            raperingPedList.Clear();
        }

        /// <summary>
        /// 同乗者生成
        /// </summary>
        private void SpawnPassengersToEmptySeat()
        {
            foreach (var seat in vehicleSeat)
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
                var spawnHeliPosition = playerPosition + new Vector3(0, 0, 40);
                var heli = GTA.World.CreateVehicle(Helis[Random.Next(0, Helis.Length)], spawnHeliPosition);
                if (!heli.IsSafeExist())
                {
                    return;
                }

                AutoReleaseOnGameEnd(heli);
                heli.SetProofs(false, false, true, true, false, false, false, false);
                heli.MaxHealth = 3000;
                heli.Health = 3000;
                _heli = heli;

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

                //カオスヘリのコルーチン開始
                var id = StartCoroutine(ChaosHeliCoroutine());
                coroutineIds.Add(id);
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
            foreach (var seat in vehicleSeat)
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
        /// すべてのカオスヘリ用のコルーチン停止
        /// </summary>
        private void StopAllChaosHeliCoroutine()
        {
            foreach (var id in coroutineIds)
            {
                StopCoroutine(id);
            }

            coroutineIds.Clear();
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

            var p = _heli.CreateRandomPedOnSeat(seat);
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
                         x.IsSafeExist() && x != p && x.IsAlive && x.IsInRangeOf(p.Position, 100)))
            {
                if(CachedMissionEntities.Value.Any(x => x.Position.DistanceTo2D(target.Position) < 30.0f))
                {
                    continue;
                }
                p.Task.FightAgainst(target);
            }

            p.Task.FightAgainst(PlayerPed);
        }
    }
}