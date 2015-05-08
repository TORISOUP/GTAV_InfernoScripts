using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;

namespace Inferno
{
    internal class Meteo : InfernoScript
    {
        private int _rpgHash;
        private bool _isActive = false;
        protected override int TickInterval
        {
            get { return 5000; }
        }

        protected override void Setup()
        {
            _rpgHash = this.GetGTAObjectHashKey("WEAPON_RPG");

            CreateInputKeywordAsObservable("meteo")
                .Subscribe(_ => _isActive = !_isActive);

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);

            OnTickAsObservable
                .Where(_ => _isActive)
                .Subscribe(_ => ShootMeteo());

            CreateInputKeywordAsObservable("killme")
                .Subscribe(_ => this.GetPlayer().Kill());
        }

        private void ShootMeteo()
        {
            try
            {
                var player = this.GetPlayer();
                var playerPosition = player.Position;

                var addPosition = new Vector3(Random.Next(-40, 40), Random.Next(-40, 40), 100);

                if (player.Velocity.Length() < 1.0f && addPosition.Length() < 10.0f)
                {
                    addPosition.Normalize();
                    addPosition *= Random.Next(10, 40);
                }

                var createPosition = playerPosition + addPosition;

                var ped = World.CreatePed(player.Model, createPosition);
                ped.MarkAsNoLongerNeeded();
                ped.SetDropWeaponWhenDead(false); //武器を落とさない
                ped.GiveWeapon(_rpgHash, 1000); //指定武器所持
                ped.EquipWeapon(_rpgHash); //武器装備

                ped.FreezePosition = true;
                ped.TaskShootAtCoord(createPosition+ new Vector3(0,0,-100), 1000);
   
            }
            catch (Exception ex)
            {

                LogWrite(ex.ToString());
            }
        }
    }
}
