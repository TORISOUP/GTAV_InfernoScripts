using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using Inferno.ChaosMode;
using Inferno.Utilities;
using UniRx;

namespace Inferno
{

    /// <summary>
    /// 市ニトロ
    /// </summary>
    public class CitizenNitro : InfernoScript
    {
        class CitizenNitroConfig : InfernoConfig
        {
            public int Probability { get; set; } = 7;

            public override bool Validate()
            {
                return Probability > 0 && Probability <= 100;
            }
        }

        protected override string ConfigFileName { get; } = "CitizenNitro.conf";
        private CitizenNitroConfig config;
        private int Probability => config?.Probability ?? 7;

        private readonly string Keyword = "cnitro";
        private readonly int[] _velocities = { -100, -70, -50, 50, 70, 100 };

        protected override void Setup()
        {
            config = LoadConfig<CitizenNitroConfig>();

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenNitro:" + IsActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //interval間隔で実行
            CreateTickAsObservable(3000)
                .Where(_ => IsActive)
                .Subscribe(_ => CitizenNitroAction());
        }

        /// <summary>
        /// ニトロ対象の選別
        /// </summary>
        private void CitizenNitroAction()
        {
            try
            {
                var playerVehicle = this.GetPlayerVehicle();

                var nitroAvailableVeles = CachedVehicles
                    .Where(x => (!playerVehicle.IsSafeExist() || x != playerVehicle) && x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist() && !x.IsPersistent);

                foreach (var veh in nitroAvailableVeles)
                {
                    if (Random.Next(0, 100) <= Probability)
                    {
                        StartCoroutine(DelayCoroutine(veh));
                    }
                }
            }
            catch (Exception e)
            {
                //nice catch!
                LogWrite(e.ToString());
            }
        }

        /// <summary>
        /// ニトロの発動を遅延させる
        /// </summary>
        private IEnumerable<object> DelayCoroutine(Vehicle v)
        {
            yield return WaitForSeconds(Random.Next(0, 5));
            var driver = v.GetPedOnSeat(VehicleSeat.Driver);
            EscapeVehicle(driver);
            NitroVehicle(v);
        }

        /// <summary>
        /// 車をニトロする
        /// </summary>
        /// <param name="vehicle"></param>
        private void NitroVehicle(Vehicle vehicle)
        {
            if (!vehicle.IsSafeExist())
            {
                return;
            }

            var velocitiesSpeed = _velocities[Random.Next(0, _velocities.Length)];

            if (velocitiesSpeed > 0 && Random.Next(0, 100) <= 15)
            {
                vehicle.Quaternion = Quaternion.RotationAxis(vehicle.RightVector, (Random.Next(20, 60) / 100.0f)) * vehicle.Quaternion;
            }

            vehicle.Speed += velocitiesSpeed;

            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                vehicle.Position.X,
                vehicle.Position.Y,
                vehicle.Position.Z,
                -1,
                0.0f,
                true,
                false,
                0.1f
            });
        }


        //車に乗ってたら緊急脱出する
        private void EscapeVehicle(Ped ped)
        {
            if (!ped.IsSafeExist()) return;

            StartCoroutine(DelayParachute(ped));
        }

        private IEnumerable<object> DelayParachute(Ped ped)
        {
            ped.SetNotChaosPed(true);
            ped.ClearTasksImmediately();
            ped.Position += new Vector3(0, 0, 0.5f);
            ped.SetToRagdoll();
            yield return null;
            ped.ApplyForce(new Vector3(0, 0, 40.0f));

            ped.IsInvincible = true;
            yield return WaitForSeconds(1.5f);
            if (!ped.IsSafeExist()) yield break;
            ped.IsInvincible = false;
            ped.ParachuteTo(PlayerPed.Position);
            yield return WaitForSeconds(15);
            if (!ped.IsSafeExist()) yield break;
            ped.SetNotChaosPed(false);
        }
    }
}
