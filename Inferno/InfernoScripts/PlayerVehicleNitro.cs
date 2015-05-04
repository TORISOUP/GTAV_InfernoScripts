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
        private bool IsNitroOK = true;
        var vehicle = this.GetPlayerVehicle();

        protected override int TickInterval
        {
            get { return 50; }
        }

        protected override void Setup()
        {

            OnTickAsObservable
                .Where(
                    _ =>
                        IsNitroOK && this.IsGamePadPressed(GameKey.Attack) && this.IsGamePadPressed(GameKey.SeekCover) &&
                        this.IsGamePadPressed(GameKey.Sprint))
                .Subscribe(_ => NitroVehicle());
        }


        void NitroVehicle()
        {
            var driver = this.GetPlayer();
            var vehicle = this.GetPlayerVehicle();
            if (!vehicle.IsSafeExist()) { return;}
            vehicle.Speed += 50;

            NitroAction(driver, vehicle);
        }

        /// <summary>
        /// ニトロ処理
        /// </summary>
        void NitroAction(Ped driver,Vehicle vehicle)
        {
            IsNitroOK = false;

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

            //無敵にしたあと３秒待機
            Wait(3*1000);


            if (driver.IsSafeExist())
            {
                driver.IsInvincible = false;
            }
            if (vehicle.IsSafeExist())
            {
                vehicle.IsInvincible = false;
            }

            //元に戻して7秒後にOKにする
            Wait(7*1000);

            IsNitroOK = true;
        }


    }
}
