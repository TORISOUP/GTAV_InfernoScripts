using GTA;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Reactive;
using System.Reactive.Subjects;

using GTA.Math;
using Inferno.ChaosMode;
using System;
using System.Collections.Generic;


namespace Inferno.InfernoScripts.World
{
    internal class ChaosHeli : InfernoScript
    {
        private Vehicle _heli = null;
        private Ped _heliDriver = null;
        private List<uint> coroutineIds = new List<uint>();
        private uint _observeHeliCoroutineId;
        private uint _observePlayerCoroutineId;
        private HashSet<Ped> raperingPedList = new HashSet<Ped>();

        //ヘリのドライバー以外の座席
        private readonly List<VehicleSeat> vehicleSeat = new List<VehicleSeat> { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    StopAllChaosHeliCoroutine();
                    DrawText("ChaosHeli:" + (IsActive ? "ON" : "OFF"), 3.0f);
                    if (IsActive)
                    {
                        ResetHeli();
                        _observeHeliCoroutineId = StartCoroutine(ObserveHeliCoroutine());
                        _observePlayerCoroutineId = StartCoroutine(ObservePlayerCoroutine());
                    }
                    else {
                        ReleasePedAndHeli();
                        StopCoroutine(_observeHeliCoroutineId);
                        StopCoroutine(_observePlayerCoroutineId);
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    IsActive = true;
                    if (_heli.IsSafeExist()) return;
                    ResetHeli();
                });

            this.OnAbortAsync
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
                if (PlayerPed.IsSafeExist() && !_heli.IsSafeExist() || _heli.IsDead || !_heli.IsInRangeOf(PlayerPed.Position, 200.0f))
                {
                    ResetHeli();
                }
                yield return WaitForSeconds(40);
            }
        }

        private IEnumerable<Object> ChaosHeliCoroutine()
        {
            yield return RandomWait();

            //ヘリが存在かつMODが有効の間回り続けるコルーチン
            while (IsActive && _heli.IsSafeExist() && _heli.IsAlive)
            {
                if (!PlayerPed.IsSafeExist()) break;

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
            if (!heli.IsSafeExist() || heli.IsDead) return false;

            //ヘリが早過ぎる場合はラペリングしない
            if (heli.Velocity.Length() > 30) return false;

            //助手席はラペリングできない
            if (seat == VehicleSeat.Passenger) return false;
            //現在ラペリング中ならできない
            var ped = heli.GetPedOnSeat(seat);
            if (!ped.IsSafeExist() || !ped.IsHuman || !ped.IsAlive || !PlayerPed.IsSafeExist()) return false;

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
                return;

            if (_heli.IsInRangeOf(targetPosition, 30))
            {
                //プレイヤに近い場合は何もしない
                _heliDriver.Task.ClearAll();
            }
            else
            {
                _heliDriver.Task.ClearAll();
                _heli.DriveTo(_heliDriver, targetPosition, 100, DrivingStyle.IgnoreLights);
            }
        }

        /// <summary>
        /// ラペリング中の市民を監視するコルーチン
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object> PassengerRapeling(Ped ped, VehicleSeat seat)
        {
            if (!ped.IsSafeExist()) yield break;
            //ラペリング降下させる
            ped.TaskRappelFromHeli();
            //降下中は無敵
            ped.IsInvincible = true;

            //一定時間降下するのを見守る
            for (var i = 0; i < 10; i++)
            {
                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist() || ped.IsDead) break;

                yield return WaitForSeconds(1);
            }

            if (!ped.IsSafeExist()) yield break;

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
                if (!PlayerPed.IsSafeExist()) return;
                var player = PlayerPed;
                var playerPosition = player.Position;
                var spawnHeliPosition = playerPosition + new Vector3(0, 0, 40);
                var heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Maverick, spawnHeliPosition);
                if (!heli.IsSafeExist()) return;
                AutoReleaseOnGameEnd(heli);
                heli.SetProofs(false, false, true, true, false, false, false, false);
                heli.MaxHealth = 3000;
                heli.Health = 3000;
                _heli = heli;

                _heliDriver = _heli.CreateRandomPedAsDriver();
                if (_heliDriver.IsSafeExist() && _heliDriver.IsHuman)
                {
                    _heliDriver.SetProofs(true, true, true, true, true, true, true, true);
                    _heliDriver.SetNotChaosPed(true);
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
            if (!_heli.IsSafeExist()) return;
            var p = _heli.CreateRandomPedOnSeat(seat);
            if (p.IsSafeExist()) { AutoReleaseOnGameEnd(p);}
        }
    }
}
