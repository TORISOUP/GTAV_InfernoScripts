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
                    if (_isActive)
                    {
                        SpawnHeli();
                    }
                });

            OnAllOnCommandObservable.Subscribe(_ => {
                _isActive = true;
                SpawnHeli();
            });

            CreateTickAsObservable(0)
                .Where(_ => _isActive)
                .Subscribe(_ => MoveHeli());
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
                    //離れ過ぎたらワープ
                    if (playerPosition.Length() - _Heli.Position.Length() > 100.0f || playerPosition.Length() - _Heli.Position.Length() < -100.0f)
                    {
                        playerPosition.Z += 20.0f;
                        _Heli.Position = playerPosition;
                        _Heli.Rotation = player.Rotation;
                        _Heli.Speed += player.Velocity.Length();
                    }
                    //市民が降りてたら新たに乗車させ直し
                    if (_Heli.IsSeatFree(VehicleSeat.Any)){
                        CreatePedIntoHeli(playerPosition);
                    }
                }
                else
                {
                    //壊れていたら再度作成
                    SpawnHeli();
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
                playerPosition.Z += 40.0f;
                _Heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Maverick, playerPosition);
                _HeliDrive = _Heli.CreateRandomPedAsDriver();
                for(int i = 0; i < 4; i++)
                {
                    CreatePedIntoHeli(playerPosition);
                }
                _HeliDrive.MarkAsNoLongerNeeded();
                _HeliDrive.SetNotChaosPed(false);
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
            var ped = this.CreateRandomPed(playerPosition);
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
