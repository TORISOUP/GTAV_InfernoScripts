using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace Inferno.InfernoScripts.World
{
    class ChaosHeli : InfernoScript
    {

        private bool _isActive = false;
        private Vehicle _Heli = null;
        private Ped _HeliDrive = null;

        protected override void Setup()
        {
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
                if (_Heli.IsAlive)
                {
                    var player = this.GetPlayer();
                    var playerPosition = player.Position;
                    _Heli.DriveTo(_HeliDrive, playerPosition, 200.0f);
                }
                else
                {
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
                playerPosition.Z += 35.0f;
                _Heli = GTA.World.CreateVehicle(GTA.Native.VehicleHash.Maverick, playerPosition);
                _HeliDrive = _Heli.CreateRandomPedAsDriver();
            }
            catch (Exception ex)
            {
                LogWrite(ex.ToString());
            }
        }
    }
}
