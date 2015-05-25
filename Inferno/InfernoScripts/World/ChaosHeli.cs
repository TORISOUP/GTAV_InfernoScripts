﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using Inferno.ChaosMode;

namespace Inferno.InfernoScripts.World
{
    class ChaosHeli : InfernoScript
    {

        private bool _isActive = false;
        private Vehicle _Heli = null;
        private Ped _HeliDrive = null;
        private Ped[] _Passenger = new Ped[3];

        private HashSet<int> chaosHeliIntoPedList = new HashSet<int>();
        private List<uint> coroutineIds = new List<uint>();

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    chaosHeliIntoPedList.Clear();
                    StopAllChaosHeliCoroutine();
                    DrawText("ChaosHeli:" + (_isActive ? "ON" : "OFF"), 3.0f);
                    if (_isActive)
                    {
                        SpawnHeli();
                    }
                    else
                    {
                        ReleasePedAndHeli();
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    _isActive = true;
                    if (!_Heli.IsSafeExist())
                    {
                        SpawnHeli();
                    }
                });

            //ヘリの移動処理
            CreateTickAsObservable(500)
                .Where(_ => _isActive)
                .Subscribe(_ => MoveHeli());

            //40秒ごとにヘリが墜落or離れすぎてないかを調べる
            CreateTickAsObservable(40000)
                .Where(_ => _isActive)
                .Subscribe(_ =>
                {
                if (!_Heli.IsSafeExist() || _Heli.IsDead || !_Heli.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist())
                    {
                        ReSpawnHeli();
                    }
                    else
                    {
                        var player = this.GetPlayer();
                        var playerPosition = player.Position;
                        //離れ過ぎてたら生成し直し
                        if (!_Heli.IsInRangeOf(player.Position, 200.0f))
                        {
                            _Heli.PetrolTankHealth = -1.0f;
                            ReSpawnHeli();
                        }
                    }
                });

            //病院から復活したらヘリ再生成
            CreateTickAsObservable(2000)
                    .Select(_ => this.GetPlayer())
                    .Where(p => p.IsSafeExist() && _Heli.IsSafeExist() && _isActive)
                    .Select(p => !p.IsAlive)
                    .DistinctUntilChanged()
                    .Where(isAlive => !isAlive)
                    .Subscribe(_ => ReSpawnHeli());
        }

        /// <summary>
        /// ヘリ移動
        /// </summary>
        private void MoveHeli(){
            try
            {
                if (_Heli.IsSafeExist() && _Heli.IsAlive && _HeliDrive.IsAlive)
                {
                    var player = this.GetPlayer();
                    var playerPosition = player.Position;
                    _Heli.DriveTo(_HeliDrive, playerPosition, 100.0f);

                    for (int i = 0; i < 3; i++)
                    {
                        //市民がいないor降りてたら新たに乗車させ直し
                        if (_Passenger[i].IsSafeExist() && !_Passenger[i].IsInVehicle(_Heli))
                        {
                            VehicleSeat seat = (VehicleSeat)i;
                            CreatePassenger(seat);
                        }

                        //ラペリング降下のコルーチン
                        chaosHeliIntoPedList.Add(_Passenger[i].Handle);
                        var id = StartCoroutine(PassengerRapeling(_Passenger[i]));
                        coroutineIds.Add(id);   
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }          
        }

        /// <summary>
        /// ラペリング中の市民を監視するコルーチン
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object> PassengerRapeling(Ped ped)
        {
            if (!ped.IsSafeExist()) yield break;
            if (!_Heli.IsSafeExist()) yield break;

            if (Random.Next(0, 100) > 5) yield break;

            //プレイヤーの位置取得
            var playerPosition = this.GetPlayer().Position;

            if (_Heli.IsInRangeOf(playerPosition, 30.0f))
            {
                ped.TaskRappelFromHeli();
            }
            else
            {
                
                yield break;
            }

            for (var i = 0; i < 20; i++)
            {
                yield return WaitForSeconds(1);

                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist()) yield break;
                if (ped.IsDead) yield break;

                //着地していたら監視終了
                if (!ped.IsInAir)
                {
                    break;
                }

            }

            if (ped.IsSafeExist())
            {
                ped.MarkAsNoLongerNeeded();
            }
        }

        /// <summary>
        /// ヘリの再生成
        /// </summary>
        private void ReSpawnHeli()
        {
            StopAllChaosHeliCoroutine();
            SpawnHeli();
        }

        /// <summary>
        /// ヘリ召喚
        /// </summary>
        private void SpawnHeli()
        {
            try
            {
                var player = this.GetPlayer();
                var playerPosition = player.Position;
                var SpawnHeliPosition = playerPosition;
                SpawnHeliPosition.Z += 40.0f;
                _Heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Maverick, SpawnHeliPosition);
                if (_Heli.IsSafeExist())
                {
                    _Heli.SetProofs(false, false, true, true, false, false, false, false);
                    _Heli.MaxHealth = 3000;
                    _Heli.Health = 3000;

                    _HeliDrive = _Heli.CreateRandomPedAsDriver();
                    _HeliDrive.SetProofs(true, true, true, true, true, true, true, true);
                    _HeliDrive.SetNotChaosPed(true);

                    CreatePassenger(VehicleSeat.Passenger);
                    CreatePassenger(VehicleSeat.LeftRear);
                    CreatePassenger(VehicleSeat.RightRear);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }
        }

        /// <summary>
        /// ヘリとドライバーの開放
        /// </summary>
        private void ReleasePedAndHeli()
        {
            if (_Heli.IsSafeExist())
            {
                _Heli.MarkAsNoLongerNeeded();
            }
            if (_HeliDrive.IsSafeExist())
            {
                _HeliDrive.MarkAsNoLongerNeeded();
            }
            _Heli = null;
            _HeliDrive = null;
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
            ReleasePedAndHeli();
        }

        /// <summary>
        /// 同乗者作成
        /// </summary>
        /// <param name="seat"></param>
        private void CreatePassenger(VehicleSeat seat)
        {
            int i = (int)seat;
            _Passenger[i] = _Heli.CreateRandomPedOnSeat(seat);
        }

    }
}
