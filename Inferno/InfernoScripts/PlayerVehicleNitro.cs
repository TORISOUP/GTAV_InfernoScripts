using System;
using System.Collections;
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
    /// <summary>
    /// プレイヤ用ニトロ
    /// TODO:発動条件を変える
    /// </summary>
    public class PlayerVehicleNitro : InfernoScript
    {
        void PlayerVehicleNitro_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode != Keys.O) return;;

            var vehicle = this.GetPlayerVehicle();
            //LogWrite("PlayerVehicleNitro\r\n");   
            //NitroVehicle(vehicle,Game.Player.Character);
        }

        void NitroVehicle(Vehicle vehicle,Ped driver)
        {
            if (vehicle == null || !vehicle.Exists()) { return; }

            vehicle.Speed += 70;

            StartCoroutine(InvincibleSwitch(driver, vehicle));


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

        /// <summary>
        /// 市民と車両を無敵にして３秒後に戻す
        /// </summary>
        IEnumerator InvincibleSwitch(Ped driver,Vehicle vehicle)
        {
            if (driver.IsSafeExist())
            {
                driver.IsInvincible = true;
            }
            if (vehicle.IsSafeExist())
            {
                vehicle.IsInvincible = true;
            }

            //3秒待機
            foreach (var s in WaitForSecond(3))
            {
                yield return s;
            }

            if (driver.IsSafeExist())
            {
                driver.IsInvincible = false;
            }
            if (vehicle.IsSafeExist())
            {
                vehicle.IsInvincible = false;
            }
        }


        protected override void Setup()
        {
            KeyDown += PlayerVehicleNitro_KeyDown;
        }
    }
}
