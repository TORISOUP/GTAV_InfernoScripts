using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using Inferno.Utilities;
using UniRx;

namespace Inferno
{
    /// <summary>
    /// プレイヤ用ニトロ
    /// </summary>
    public class PlayerVehicleNitro : InfernoScript
    {

        class PlayerNitroConf : InfernoConfig
        {
            public float StraightAccelerationSpeed { get; set; } = 50;
            public float JumpAccelerationSpeed { get; set; } = 20;
            public float CoolDownSeconds { get; set; } = 10.0f;

            public override bool Validate()
            {
                if (CoolDownSeconds <= 0) return false;
                return true;
            }
        }

        protected override string ConfigFileName { get; } = "PlayerNitro.conf";

        private PlayerNitroConf conf;

        private bool _isNitroOk = true;

        protected override int TickInterval => 50;

        private float StraightAccelerationSpeed => conf?.StraightAccelerationSpeed ?? 50;
        private float JumpAccelerationSpeed => conf?.JumpAccelerationSpeed ?? 20;
        private float CoolDownSeconds => conf?.CoolDownSeconds ?? 10.0f;

        protected override void Setup()
        {
            conf = LoadConfig<PlayerNitroConf>();

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
            var driver = PlayerPed;
            if (!driver.IsSafeExist()) return;
            var vehicle = this.GetPlayerVehicle();
            if (!vehicle.IsSafeExist())
            {
                return;
            }

            float rotation;
            if (this.IsGamePadPressed(GameKey.VehicleAccelerateKey))
            {
                rotation = this.IsGamePadPressed(GameKey.VehicleForwardTiltKey) ? 0.5f : 0.0f;
            }
            else
            {
                //車体回転時用にスティック入力を-127～127で取得して-0.5～0.5の値になるように調整
                rotation = this.GetStickValue().Y / 250.0f;
            }


            vehicle.Quaternion = Quaternion.RotationAxis(vehicle.RightVector, rotation) * vehicle.Quaternion;

            var deadZone = 0.25f;
            var addSpeed = ((rotation > deadZone || rotation < -deadZone) ? JumpAccelerationSpeed : StraightAccelerationSpeed);
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



        /// <summary>
        /// ニトロ処理
        /// </summary>
        private void NitroAction(Ped driver, Vehicle vehicle)
        {
            _isNitroOk = false;

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

        private IEnumerable<Object> NitroAfterTreatment(Ped driver, Vehicle vehicle)
        {
            //カウンタ作成
            var counter = new ReduceCounter((int)(CoolDownSeconds * 1000));
            //カウンタを描画
            RegisterProgressBar(
                new ProgressBarData(counter, new Point(0, 30),
                Color.FromArgb(200, 0, 255, 125),
                Color.FromArgb(128, 0, 0, 0),
                DrawType.RightToLeft, 100, 10, 2));

            //カウンタを自動カウント
            RegisterCounter(counter);

            foreach (var s in WaitForSeconds(CoolDownSeconds))
            {
                if (!PlayerPed.IsInVehicle() || PlayerPed.IsDead)
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
