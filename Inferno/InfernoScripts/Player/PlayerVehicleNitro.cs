using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno;

namespace Inferno
{
    /// <summary>
    /// プレイヤ用ニトロ
    /// </summary>
    public class PlayerVehicleNitro : InfernoScript
    {
        private bool _isNitroOk = true;

        protected override int TickInterval => 50;

        protected override void Setup()
        {
            IsActive = true;
            OnTickAsObservable
                .Where(
                    _ =>
                        _isNitroOk && this.IsGamePadPressed(GameKey.VehicleAccelerate) && this.IsGamePadPressed(GameKey.VehicleHandbrake) &&
                        this.IsGamePadPressed(GameKey.VehicleDuck))
                .Subscribe(_ => NitroVehicle());
        }


        private void NitroVehicle()
        {
            var driver = playerPed;
            if (!driver.IsSafeExist()) return;
            var vehicle = this.GetPlayerVehicle();
            if (!vehicle.IsSafeExist())
            {
                return;
            }

            //車体回転時用にスティック入力を0～254の範囲から-127～127に変換。後は4のニトロの計算そのまま
            var rotation = ((this.GetStickValue(GameKey.LStickDown) - 127) * 30.0f) / 126.0f;
            if (Game.Player.WantedLevel == 0)
            {
                vehicle.Rotation = new Vector3(vehicle.Rotation.X + rotation, vehicle.Rotation.Y, vehicle.Rotation.Z);
            }
            var deadZone = 20.0f;
            if (this.IsGamePadPressed(GameKey.VehicleHorn))
            {
                vehicle.Speed -= (rotation > deadZone ? 20.0f : 50.0f);
            }
            else
            {
                vehicle.Speed += (rotation > deadZone ? 20.0f : 50.0f);
            }

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
            DrawText("Nitro:OK", 2.0f);
        }


    }
}
