using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using Inferno;

namespace ToriScript.Inferno
{
    public class PlayerVehicleNitro : Script
    {
        public PlayerVehicleNitro()
        {
            KeyDown += PlayerVehicleNitro_KeyDown;
        }

 

        void PlayerVehicleNitro_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode != Keys.O) return;;

            var vehicle = this.GetPlayerVehicle();
            
            NitroVehicle(vehicle,Game.Player.Character);
         
        }

        void NitroVehicle(Vehicle vehicle,Ped driver)
        {
            if (vehicle == null || !vehicle.Exists()) { return; }

            vehicle.Speed += 70;

     
            Observable.Return(Unit.Default)
                .Do(_ =>
                {
                    if (driver != null)
                    {
                        driver.IsInvincible = true;
                    }
                    vehicle.IsInvincible = true;
                })
                .Delay(TimeSpan.FromSeconds(3))
                .Subscribe(_ =>
                {
                    if (driver != null)
                    {
                        driver.IsInvincible = false;
                    }
                    vehicle.IsInvincible = false;
                });


            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                vehicle.Position.X,
                vehicle.Position.Y,
                vehicle.Position.Z,
                0,
                1.0f,
                true,
                true,
                1.0f
            });
        }


    }
}
