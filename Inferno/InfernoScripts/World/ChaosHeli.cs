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
        private Ped _HeliDrive = null;

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cheli")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
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

            OnAllOnCommandObservable.Subscribe(_ => {
                _isActive = true;
                if (!_Heli.IsSafeExist())
                {
                    ReSpawnHeli();
                }
            });

            //ヘリの移動処理
            CreateTickAsObservable(500)
                .Where(_ => _isActive)
                .Subscribe(_ => MoveHeli());

            //40秒ごとにヘリが壊れてないかor離れすぎてないかを調べる
            CreateTickAsObservable(40000)
                .Where(_ => _isActive)
                .Subscribe(_ =>
                {
                    if (!_Heli.IsSafeExist() || _Heli.IsDead)
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
                    //市民が降りてたら新たに乗車させ直し
                    if (_Heli.IsSeatFree(VehicleSeat.Any)){
                        CreatePedIntoHeli(playerPosition);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }          
        }

        /// <summary>
        /// ヘリの再生成
        /// </summary>
        private void ReSpawnHeli()
        {
            ReleasePedAndHeli();
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
                    CreatePedIntoHeli(SpawnHeliPosition);
                    _HeliDrive.SetNotChaosPed(true);
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
        /// ヘリ内にランダムな市民作成と乗車
        /// </summary>
        /// <param name="playerPosition"></param>
        private void CreatePedIntoHeli(GTA.Math.Vector3 playerPosition)
        {
            var ped = NativeFunctions.CreateRandomPed(playerPosition);
            ped.MarkAsNoLongerNeeded();
            ped.SetIntoVehicle(_Heli, GTA.VehicleSeat.Any);
            ped.SetNotChaosPed(false);
        }
    }
}
