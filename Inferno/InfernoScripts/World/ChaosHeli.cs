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

        private HashSet<VehicleSeat> rapelingToPedInSeatList = new HashSet<VehicleSeat>();
        private List<uint> coroutineIds = new List<uint>();

        //ヘリのドライバー以外の座席
        private readonly List<VehicleSeat> vehicleSeat = new List<VehicleSeat> { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    rapelingToPedInSeatList.Clear();
                    StopAllChaosHeliCoroutine();
                    DrawText("ChaosHeli:" + (_isActive ? "ON" : "OFF"), 3.0f);
                    if (_isActive){
                        CreateChaosHeli();
                    }else{
                        ReleasePedAndHeli();
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    _isActive = true; 
                    if (!_Heli.IsSafeExist()){
                        rapelingToPedInSeatList.Clear();
                        CreateChaosHeli();
                    }
                });

            //ヘリのリセット処理
            var onPlayerRevivalAsObservable = CreateTickAsObservable(2000)
             .Select(_ => this.GetPlayer())
             .Where(p => p.IsSafeExist() && _isActive)
             .Select(p => p.IsAlive)
             .DistinctUntilChanged()
             .Where(isAlive => isAlive);

            var intervalCheck = CreateTickAsObservable(40000).Where(_ => _isActive);

            intervalCheck 
                .Merge(onPlayerRevivalAsObservable.Select(_ => System.Reactive.Unit.Default))
                .Subscribe(_ =>
                {
                    var player = this.GetPlayer();
                    var playerPosition = player.Position;
                    if (_Heli.IsDead || !_Heli.IsInRangeOf(player.Position, 200.0f))
                    {
                        ResetHeli();
                    }
                });
        }

        private IEnumerable<Object> ChaosHeliCoroutine()
        {
            //ヘリが存在かつMODが有効の間回り続けるコルーチン
            while (true)
            {
                if (!_isActive) break;

                if (!_Heli.IsSafeExist() || _Heli.IsDead) break;

                MoveHeli();
                SpawnPassengersToEmptySeat();

                //プレイヤーの位置取得
                var playerPosition = this.GetPlayer().Position;

                //各座席ごとの処理
                foreach (var seat in vehicleSeat)
                {
                    //座席にいる市民取得
                    var ped = _Heli.GetPedOnSeat(seat);

                    //ラペリング降下の判定
                    if (seat != VehicleSeat.Passenger && ped.IsSafeExist() && _Heli.IsSafeExist() && Random.Next(0, 100) > 5 && _Heli.IsInRangeOf(playerPosition, 30.0f) && !rapelingToPedInSeatList.Contains(seat))
                    {
                        rapelingToPedInSeatList.Add(seat);
                        //ラペリング降下のコルーチン
                        var id = StartCoroutine(PassengerRapeling(ped,seat));
                        coroutineIds.Add(id);
                    }
                }

                yield return WaitForSeconds(1);
            }
            ReleasePedAndHeli();
        }

        /// <summary>
        /// ヘリ移動
        /// </summary>
        private void MoveHeli(){
            try
            {
                if (_heliDriver.IsSafeExist() && _heliDriver.IsAlive)
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
        private IEnumerable<Object> PassengerRapeling(Ped ped, VehicleSeat seat)
        {

            //ラペリング降下させる
            ped.TaskRappelFromHeli();

            //一定時間降下するのを見守る
            for (var i = 0; i < 20; i++)
            {
                yield return WaitForSeconds(1);

                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist() || ped.IsDead) break;

                //着地していたら監視終了
                if (!ped.IsInAir)
                {
                    break;
                }
            }

            yield return WaitForSeconds(3);

            if (ped.IsSafeExist())
            {
                ped.MarkAsNoLongerNeeded();
            }

            rapelingToPedInSeatList.Remove(seat);
        }

        /// <summary>
        /// ヘリのリセット
        /// </summary>
        private void ResetHeli()
        {
            StopAllChaosHeliCoroutine();
            ReleasePedAndHeli();
            CreateChaosHeli();
        }

        /// <summary>
        /// 同乗者生成
        /// </summary>
        private void SpawnPassengersToEmptySeat()
        {
            foreach (var seat in vehicleSeat)
            {
                //ヘリが存在し座席に誰もいなかったら市民再生成
                if (_Heli.IsSafeExist() && _Heli.IsSeatFree(seat))
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

                //ドライバー開放
                if (_heliDriver.IsSafeExist())
                {
                    //無敵化解除
                    _heliDriver.SetProofs(false, false, false, false, false, false, false, false);
                    //ヘリのドライバー解放
                    _heliDriver.MarkAsNoLongerNeeded();
                }

                //ヘリ解放
                _Heli.MarkAsNoLongerNeeded();
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
            rapelingToPedInSeatList.Clear();
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
