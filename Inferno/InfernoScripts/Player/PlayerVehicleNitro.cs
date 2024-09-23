using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.InfernoScripts.InfernoCore.UI;
using Inferno.Utilities;
using LemonUI;
using LemonUI.Menus;

namespace Inferno
{
    /// <summary>
    /// プレイヤ用ニトロ
    /// </summary>
    public class PlayerVehicleNitro : InfernoScript
    {
        private bool _isNitroOk = true;

        private PlayerNitroConf conf;

        protected override string ConfigFileName { get; } = "PlayerNitro.conf";

        private float StraightAccelerationSpeed => conf?.StraightAccelerationSpeed ?? 50;
        private float JumpAccelerationSpeed => conf?.JumpAccelerationSpeed ?? 20;
        private float CoolDownSeconds => conf?.CoolDownSeconds ?? 10.0f;

        protected override void Setup()
        {
            conf = LoadConfig<PlayerNitroConf>();
            IsActive = true;
            CreateTickAsObservable(TimeSpan.FromMilliseconds(50))
                .Where(
                    _ =>
                        IsActive &&
                        _isNitroOk &&
                        Game.IsControlPressed(Control.VehicleAccelerate) &&
                        Game.IsControlPressed(Control.VehicleHandbrake) &&
                        Game.IsControlPressed(Control.VehicleDuck))
                .Subscribe(_ => NitroVehicle());
        }

        private void NitroVehicle()
        {
            var driver = PlayerPed;
            if (!driver.IsSafeExist())
            {
                return;
            }

            var vehicle = this.GetPlayerVehicle();
            if (!vehicle.IsSafeExist())
            {
                return;
            }

            float rotation;
            if (Game.IsControlPressed(Control.ScriptPadUp))
            {
                rotation = Game.IsControlPressed(Control.FrontendLs) ? 0.5f : 0.0f;
            }
            else
                //車体回転時用にスティック入力を-127～127で取得して-0.5～0.5の値になるように調整
            {
                rotation = this.GetStickValue().Y / 250.0f;
            }


            vehicle.Quaternion = Quaternion.RotationAxis(vehicle.RightVector, rotation) * vehicle.Quaternion;

            var deadZone = 0.25f;
            var addSpeed = rotation > deadZone || rotation < -deadZone
                ? JumpAccelerationSpeed
                : StraightAccelerationSpeed;
            if (Game.IsControlPressed(Control.VehicleHorn))
            {
                vehicle.SetForwardSpeed(vehicle.Speed - addSpeed);
            }
            else
            {
                vehicle.SetForwardSpeed(vehicle.Speed + addSpeed);
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

            NitroAfterTreatmentAsync(ActivationCancellationToken).Forget();
        }

        private async ValueTask NitroAfterTreatmentAsync(CancellationToken ct)
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

            while (!ct.IsCancellationRequested && !counter.IsCompleted)
            {
                if (!PlayerPed.IsInVehicle() || PlayerPed.IsDead)
                {
                    //死んだりクルマから降りたらリセット
                    counter.Finish();
                    _isNitroOk = true;
                    return;
                }

                await YieldAsync(ct);
            }

            counter.Finish();
            _isNitroOk = true;
            DrawText("Nitro:OK", 2.0f);
        }

        [Serializable]
        private class PlayerNitroConf : InfernoConfig
        {
            public float StraightAccelerationSpeed = 50;
            public float JumpAccelerationSpeed = 20;
            public float CoolDownSeconds = 10.0f;

            public override bool Validate()
            {
                if (CoolDownSeconds <= 0)
                {
                    return false;
                }

                return true;
            }
        }

        #region UI

        public override bool UseUI => true;
        public override string DisplayText => IsLangJpn ? "ニトロ" : "Player Vehicle NOS";

        public override bool CanChangeActive => true;

        public override MenuIndex MenuIndex => MenuIndex.Player;

        public override void OnUiMenuConstruct(ObjectPool pool, NativeMenu menu)
        {
            // 直線上の加速
            menu.AddSlider(
                $"Straight: {conf.StraightAccelerationSpeed}[ms]",
                IsLangJpn
                    ? "直線での加速量"
                    : "Amount of acceleration in a straight line",
                (int)conf.StraightAccelerationSpeed,
                150,
                x => x.Multiplier = 10, item =>
                {
                    conf.StraightAccelerationSpeed = item.Value;
                    item.Title = $"Straight: {conf.StraightAccelerationSpeed}[ms]";
                });
            
            // ジャンプ時の加速
            menu.AddSlider(
                $"Jump: {conf.JumpAccelerationSpeed}[ms]",
                IsLangJpn
                    ? "ジャンプ時の加速量"
                    : "Amount of acceleration when jumping",
                (int)conf.JumpAccelerationSpeed,
                150,
                x => x.Multiplier = 10, item =>
                {
                    conf.JumpAccelerationSpeed = item.Value;
                    item.Title = $"Jump: {conf.JumpAccelerationSpeed}[ms]";
                });
            
            // クールダウン時間
            menu.AddSlider(
                $"CoolDown: {conf.CoolDownSeconds}[s]",
                IsLangJpn
                    ? "クールダウン時間"
                    : "Cool down time",
                (int)conf.CoolDownSeconds,
                60,
                x => x.Multiplier = 1, item =>
                {
                    conf.CoolDownSeconds = item.Value;
                    item.Title = $"CoolDown: {conf.CoolDownSeconds}[s]";
                });
        }

        #endregion
    }
}