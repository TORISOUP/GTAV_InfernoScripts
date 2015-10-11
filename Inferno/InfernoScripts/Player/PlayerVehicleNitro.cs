using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
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

            //車体回転時用にスティック入力を-127～127で取得して-0.5～0.5の値になるように調整
            var rotation = this.GetStickValue().Y / 250.0f;
            if (Game.Player.WantedLevel == 0)
            {
                vehicle.Quaternion = Quaternion.RotationAxis(vehicle.RightVector, rotation) * vehicle.Quaternion;
            }
            var deadZone = 0.25f;
            var addSpeed = ((rotation > deadZone || rotation < -deadZone) ? 20.0f : 50.0f);
            if (this.IsGamePadPressed(GameKey.VehicleHorn))
            {
                vehicle.Speed -= addSpeed;
            }
            else
            {
                vehicle.Speed += addSpeed;
            }

            NitroAction(driver, vehicle);
        }

        private void ChangeDirverAndVehicleState(Ped driver, Vehicle vehicle, bool isInvincible)
        {

            if (driver.IsSafeExist())
            {
                driver.IsInvincible = isInvincible;
            }
            if (vehicle.IsSafeExist())
            {
                vehicle.IsInvincible = isInvincible;
            }
        }


        /// <summary>
        /// ニトロ処理
        /// </summary>
        void NitroAction(Ped driver,Vehicle vehicle)
        {
            _isNitroOk = false;

            ChangeDirverAndVehicleState(driver, vehicle, true);

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
            //カウンタ作成
            var counter = new ReduceCounter(10000);
            //カウンタを描画
            RegisterProgressBar(
                new ProgressBarData(counter, new Point(0, 30),
                Color.FromArgb(200, 0, 255, 125),
                Color.FromArgb(128, 0, 0, 0), 
                DrawType.RightToLeft, 100, 10, 2));

            //カウンタを自動カウント
            RegisterCounter(counter);

            //3秒まつ

            foreach (var s in WaitForSeconds(3))
            {
                if (!playerPed.IsInVehicle() || playerPed.IsDead)
                {
                    //死んだりクルマから降りたらリセット
                    counter.Finish();
                    _isNitroOk = true;
                    ChangeDirverAndVehicleState(driver, vehicle, false);
                    yield break;
                }
                yield return s;
            }

            ChangeDirverAndVehicleState(driver, vehicle, false);

            //７秒まつ

            foreach (var s in WaitForSeconds(7))
            {
                if (!playerPed.IsInVehicle() || playerPed.IsDead)
                {
                    //死んだりクルマから降りたらリセット
                    counter.Finish();
                    _isNitroOk = true;
                    yield break;
                }
                yield return s;
            }

            counter.Finish();
            _isNitroOk = true;
            DrawText("Nitro:OK", 2.0f);
        }


    }
}
