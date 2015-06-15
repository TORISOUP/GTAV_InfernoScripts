using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using Inferno.ChaosMode;

namespace Inferno.InfernoScripts.World
{
    class ChaosHeli : InfernoScript
    {

        private bool _isActive = false;
        private Vehicle _heli = null;
        private Ped _heliDriver = null;

        private HashSet<VehicleSeat> rapelingToPedInSeatList = new HashSet<VehicleSeat>();
        private List<uint> coroutineIds = new List<uint>();

        //ヘリのドライバー以外の座席
        private readonly List<VehicleSeat> vehicleSeat = new List<VehicleSeat> { VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear };

        protected override int TickInterval => 40*1000;

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
                        ResetHeli();
                    }
                    else{
                        ReleasePedAndHeli();
                    }
                });

            OnAllOnCommandObservable
                .Subscribe(_ =>
                {
                    _isActive = true;
                    if (_heli.IsSafeExist()) return;
                    rapelingToPedInSeatList.Clear();
                    ResetHeli();
                });

            //ヘリのリセット処理
           CreateTickAsObservable(2000)
                .Where(_=> _isActive && playerPed.IsSafeExist())
                .Select(_ => playerPed.IsAlive)
                .Where(x => !x)
                .Subscribe(_ => ResetHeli());

            OnTickAsObservable
                .Where(_=>_isActive) 
                .Subscribe(_ =>
                {
                    var player = playerPed;
                    if(!player.IsSafeExist()) return;
                    if (!_heli.IsSafeExist() || _heli.IsDead || !_heli.IsInRangeOf(player.Position, 200.0f))
                    {
                        ResetHeli();
                    }
                });
        }

        private IEnumerable<Object> ChaosHeliCoroutine()
        {
            yield return RandomWait();

            //ヘリが存在かつMODが有効の間回り続けるコルーチン
            while (_isActive && _heli.IsSafeExist() && _heli.IsAlive)
            {
                if(!playerPed.IsSafeExist()) break;

                var playerPos = playerPed.Position;

                //ヘリがプレイヤから離れすぎていた場合は追いかける
                MoveHeli(_heliDriver, playerPos);

                yield return WaitForSeconds(1);

                SpawnPassengersToEmptySeat();

                yield return WaitForSeconds(1);

                //各座席ごとの処理
                foreach (var seat in vehicleSeat)
                {
                    if (!CheckRapeling(_heli, seat)) continue;
                    var ped = _heli.GetPedOnSeat(seat);
                    if (!ped.IsSafeExist()) continue;

                    if (Random.Next(100) <= 30 && rapelingToPedInSeatList.Add(seat))
                    {
                        //ラペリング降下のコルーチン
                        var id = StartCoroutine(PassengerRapeling(ped, seat));
                        coroutineIds.Add(id);
                    }
                }

                yield return WaitForSeconds(1);
            }
            ReleasePedAndHeli();
        }

        /// <summary>
        /// 現在ラペリング可能な状態であるか調べる
        /// </summary>
        /// <returns>trueでラペリング許可</returns>
        private bool CheckRapeling(Vehicle heli,VehicleSeat seat)
        {
            //ヘリが壊れていたり存在しないならラペリングできない
            if (!heli.IsSafeExist() || heli.IsDead) return false;

            //ヘリが早過ぎる場合はラペリングしない
            if (heli.Velocity.Length() > 30) return false;

            //助手席はラペリングできない
            if(seat== VehicleSeat.Passenger) return false;
            //現在ラペリング中ならできない
            if (rapelingToPedInSeatList.Contains(seat)) return false;
            var ped = heli.GetPedOnSeat(seat);
            if (!ped.IsSafeExist() || !ped.IsAlive || !playerPed.IsSafeExist()) return false;

            var playerPosition = playerPed.Position;
            
            //プレイヤの近くならラペリングする
            return heli.IsInRangeOf(playerPosition, 50.0f);
        }

        /// <summary>
        /// ヘリを指定座標に移動させる
        /// </summary>
        /// <param name="heliDriver">ドライバ</param>
        /// <param name="targetPosition">目標地点</param>
        private void MoveHeli(Ped heliDriver,Vector3 targetPosition)
        {
            var player = playerPed;
            if (!_heli.IsSafeExist() || !player.IsSafeExist() || !heliDriver.IsSafeExist() || !heliDriver.IsAlive)
                return;       
            
            var playerPosition = player.Position;


            if (_heli.IsInRangeOf(targetPosition, 30))
            {
               //プレイヤに本当に近い場合は何もしない
               _heliDriver.Task.ClearAll();
            }
            else
            { 
                //プレイヤの近くにいる場合はゆっくり飛行
                var speed = _heli.IsInRangeOf(targetPosition, 100) ? 10 : 100;
                _heliDriver.Task.ClearSecondary();
                _heli.DriveTo(_heliDriver, playerPosition, speed, DrivingStyle.Normal);
            }
        }

        /// <summary>
        /// ラペリング中の市民を監視するコルーチン
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        private IEnumerable<Object> PassengerRapeling(Ped ped, VehicleSeat seat)
        {
            yield return RandomWait();

            if(!ped.IsSafeExist()) yield break;
            //ラペリング降下させる
            ped.TaskRappelFromHeli();
            //降下中は無敵
            ped.IsInvincible = true;

            //一定時間降下するのを見守る
            for (var i = 0; i < 20; i++)
            {
                //市民が消えていたり死んでたら監視終了
                if (!ped.IsSafeExist() || ped.IsDead) break;

                //着地していたら監視終了
                if (!ped.IsInAir)
                {
                    break;
                }
                yield return WaitForSeconds(1);
            }

            yield return WaitForSeconds(1);
            rapelingToPedInSeatList.Remove(seat);

            if (ped.IsSafeExist())
            {
                ped.IsInvincible = false;
                ped.Health = 100;
                ped.MarkAsNoLongerNeeded();
            }

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
                if(!playerPed.IsSafeExist()) return;
                var player = playerPed;
                var playerPosition = player.Position;
                var spawnHeliPosition = playerPosition + new Vector3(0,0,40);
                var heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Maverick, spawnHeliPosition);
                if (!heli.IsSafeExist()) return;
                heli.SetProofs(false, false, true, true, false, false, false, false);
                heli.MaxHealth = 3000;
                heli.Health = 3000;
                _heli = heli;

                _heliDriver = _heli.CreateRandomPedAsDriver();
                if (_heliDriver.IsSafeExist())
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
            if (_heli.IsSafeExist())
            {
                //ヘリの解放前に座席に座っている市民を解放する
                foreach (var seat in vehicleSeat)
                {
                    //座席にいる市民取得
                    var ped = _heli.GetPedOnSeat(seat);
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
                _heli.SetProofs(false, false, false, false, false, false, false, false);
                _heli.MarkAsNoLongerNeeded();
                _heli.PetrolTankHealth = -100;
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
            rapelingToPedInSeatList.Clear();
        }

        /// <summary>
        /// 同乗者作成
        /// </summary>
        /// <param name="seat"></param>
        private void CreatePassenger(VehicleSeat seat)
        {
            if(!_heli.IsSafeExist()) return;
            _heli.CreateRandomPedOnSeat(seat);
        }

    }
}
