using System;
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
        private Ped _heliDriver = null;

        /// <summary>
        /// ラぺリング降下関連のコルーチン
        /// </summary>
        private HashSet<int> chaosHeliIntoPedList = new HashSet<int>();
        private List<uint> coroutineIds = new List<uint>();

        //ヘリのドライバー以外の座席
        private readonly List<VehicleSeat> vehicleSeat = new List<VehicleSeat> { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    chaosHeliIntoPedList.Clear();
                    StopAllChaosHeliCoroutine();
                    DrawText("ChaosHeli:" + (_isActive ? "ON" : "OFF"), 3.0f);
                    if (_isActive){
                        SpawnHeli();
                    }else{
                        ReleasePedAndHeli();
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    _isActive = true; 
                    if (!_Heli.IsSafeExist()){
                        chaosHeliIntoPedList.Clear();
                        SpawnHeli();
                    }
                });

            //ヘリの各種処理
            StartCoroutine(ChaosHeliTasks());

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

        private IEnumerable<Object> ChaosHeliTasks()
        {
            while (true)
            {
                yield return WaitForSeconds(1);
                if (!_isActive) continue;
                if (_Heli.IsSafeExist() && _Heli.IsAlive) continue;
                MoveHeli();
                ReSpawnPassenger();
                //各座席ごとの処理
                foreach(var seat in vehicleSeat)
                {
                    //座席にいる市民取得
                    var ped = _Heli.GetPedOnSeat(seat);

                    if (ped.IsSafeExist()) {
                        var id = StartCoroutine(PassengerRapeling(ped));
                        coroutineIds.Add(id);
                    }
                }
            }
        }

        /// <summary>
        /// ヘリ移動
        /// </summary>
        private void MoveHeli(){
            try
            {
                if (_heliDriver.IsAlive)
                {
                    var player = this.GetPlayer();
                    var playerPosition = player.Position;
                    _Heli.DriveTo(_heliDriver, playerPosition, 100.0f);
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

            var pedId = ped.Handle;

            //プレイヤーの位置取得
            var playerPosition = this.GetPlayer().Position;

            //一定時間降下するのを見守る
            for (var i = 0; i < 20; i++)
            {
                yield return WaitForSeconds(1);

                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist()) break;
                if (ped.IsDead) break;

                //着地していたら監視終了
                if (!ped.IsInAir)
                {
                    break;
                }

                //プレイヤーの近くなら降下させる
                if (_Heli.IsInRangeOf(playerPosition, 30.0f))
                {
                    ped.TaskRappelFromHeli();
                }
                else
                {
                    break;
                }

            }

            if (ped.IsSafeExist())
            {
                ped.MarkAsNoLongerNeeded();
            }

            chaosHeliIntoPedList.Remove(pedId);

        }

        /// <summary>
        /// ヘリの再生成
        /// </summary>
        private void ReSpawnHeli()
        {
            StopAllChaosHeliCoroutine();
            ReleasePedAndHeli();
            SpawnHeli();
        }

        /// <summary>
        /// 同乗者再生成
        /// </summary>
        private void ReSpawnPassenger()
        {
            foreach (var seat in vehicleSeat)
            {
                //座席に誰もいなかったら市民再生成
                if (_Heli.IsSeatFree(seat))
                {
                    CreatePassenger(seat);
                }
            }
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
                if (!_Heli.IsSafeExist()) return;
                    _Heli.SetProofs(false, false, true, true, false, false, false, false);
                    _Heli.MaxHealth = 3000;
                    _Heli.Health = 3000;

                    _heliDriver = _Heli.CreateRandomPedAsDriver();
                    _heliDriver.SetProofs(true, true, true, true, true, true, true, true);
                    _heliDriver.SetNotChaosPed(true);

                    CreatePassenger(VehicleSeat.Passenger);
                    CreatePassenger(VehicleSeat.LeftRear);
                    CreatePassenger(VehicleSeat.RightRear);
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
            if (_Heli.IsSafeExist())
            {
                //ヘリの解放前に座席に座っている市民を解放する
                foreach (var seat in vehicleSeat)
                {
                    //座席にいる市民取得
                    var ped = _Heli.GetPedOnSeat(seat);
                    if (ped.IsSafeExist())
                    {
                        ped.MarkAsNoLongerNeeded();
                    }
                }
                //ヘリ解放
                _Heli.MarkAsNoLongerNeeded();
            }
            if (_heliDriver.IsSafeExist())
            {
                //無敵化解除
                _heliDriver.SetProofs(false, false, false, false, false, false, false, false);
                //ヘリのドライバー解放
                _heliDriver.MarkAsNoLongerNeeded();
            }
            _Heli = null;
            _heliDriver = null;
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
            int i = (int)seat;
            _Heli.CreateRandomPedOnSeat(seat);
        }

    }
}
