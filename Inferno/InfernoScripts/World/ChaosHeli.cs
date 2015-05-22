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
        private WeaponProvider weaponProvider;

        protected override void Setup()
        {

            weaponProvider = new WeaponProvider();

            CreateInputKeywordAsObservable("heli")
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    DrawText("ChaosHeli:" + _isActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);

            //ヘリの移動処理
            CreateTickAsObservable(0)
                .Where(_ => _isActive)
                .Subscribe(_ => MoveHeli());

            //40秒ごとにヘリが壊れてないかor離れすぎてないかを調べる
            CreateTickAsObservable(40000)
                .Where(_ => _isActive)
                .Subscribe(_ =>
                {
                    if (!_Heli.IsAlive)
                    {
                        _HeliDrive.Health = -1;
                        _HeliDrive.MarkAsNoLongerNeeded();
                        _Heli.MarkAsNoLongerNeeded();
                        _HeliDrive.MarkAsNoLongerNeeded();
                        SpawnHeli();
                    }
                    else
                    {
                        var player = this.GetPlayer();
                        var playerPosition = player.Position;
                        //離れ過ぎてたら生成し直し
                        if (playerPosition.Length() - _Heli.Position.Length() > 200.0f || playerPosition.Length() - _Heli.Position.Length() < -200.0f)
                        {
                            _HeliDrive.Health = -1;
                            _HeliDrive.MarkAsNoLongerNeeded();
                            _Heli.PetrolTankHealth = -1;
                            _Heli.MarkAsNoLongerNeeded();
                            SpawnHeli();
                        }
                    }
                });

            OnTickAsObservable
                    .Select(_ => this.GetPlayer())
                    .Where(p => p.IsSafeExist() && _Heli.IsSafeExist() && _isActive)
                    .Select(p => !p.IsAlive)
                    .DistinctUntilChanged()
                    .Where(isAlive => !isAlive)
                    .Subscribe(_ => {
                        _HeliDrive.Health = -1;
                        _HeliDrive.MarkAsNoLongerNeeded();
                        _Heli.PetrolTankHealth = -1;
                        _Heli.MarkAsNoLongerNeeded();
                        SpawnHeli();
                    });
        }

        /// <summary>
        /// ヘリ移動
        /// </summary>
        private void MoveHeli(){
            try
            {
                if (_Heli.IsAlive && _HeliDrive.IsAlive)
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
                _Heli.SetProofs(false, false, true, true, false, false, false, false);
                _HeliDrive = _Heli.CreateRandomPedAsDriver();
                _HeliDrive.SetProofs(true, true, true, true, true, true, true, true);
                _Heli.MaxHealth = 3000;
                _Heli.Health = 3000;
                for (int i = 0; i < 4; i++)
                {
                    CreatePedIntoHeli(SpawnHeliPosition);
                }
                _HeliDrive.SetNotChaosPed(true);
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }
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
            var weapon = weaponProvider.GetRandomInVehicleWeapon();
            var weaponhash = (int)weapon;

            ped.SetDropWeaponWhenDead(false); //武器を落とさない
            ped.GiveWeapon(weaponhash, 1000); //指定武器所持
            ped.EquipWeapon(weaponhash); //武器装備
        }
    }
}
