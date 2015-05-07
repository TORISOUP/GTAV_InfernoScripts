using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            get { return 2500; }
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
        }

        private void ShootMeteo()
        {
            try
            {
                var player = this.GetPlayer();
                var playerPosition = player.Position;
                var createPosition = playerPosition + new Vector3(Random.Next(-40, 40), Random.Next(-40, 40), 100);
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
