using System;
using System.Collections;
using System.Collections.Generic;
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
        private bool _isNitroOk = true;

        protected override int TickInterval
        {
            get { return 50; }
        }

        protected override void Setup()
        {

            OnTickAsObservable
                .Where(
                    _ =>
                        _isNitroOk && this.IsGamePadPressed(GameKey.Attack) && this.IsGamePadPressed(GameKey.SeekCover) &&
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
            _isNitroOk = false;

            if (driver.IsSafeExist())
            {
                driver.IsInvincible = true;
            }
            if (vehicle.IsSafeExist())
            {
                vehicle.IsInvincible = true;
            }

            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                vehicle.Position.X,
                vehicle.Position.Y,
                vehicle.Position.Z,
                -1,
                1.0f,
                true,
                false,
                1.0f
            });

            StartCoroutine(NitroAfterTreatment(driver, vehicle));
        }

        IEnumerable<Object> NitroAfterTreatment(Ped driver,Vehicle vehicle)
        {
            yield return WaitForSeconds(3);
            
            if (driver.IsSafeExist())
            {
                driver.IsInvincible = false;
            }
            if (vehicle.IsSafeExist())
            {
                vehicle.IsInvincible = false;
            }

            yield return WaitForSeconds(7);

            _isNitroOk = true;
        }


    }
}
