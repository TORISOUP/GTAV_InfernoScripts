using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using Inferno.ChaosMode;
using Inferno.Utilities;

namespace Inferno
{
    /// <summary>
    /// 市ニトロ
    /// </summary>
    public class CitizenNitro : InfernoScript
    {
        private readonly int[] _velocities = { -100, -70, -50, 50, 70, 100 };

        private readonly string Keyword = "cnitro";
        private CitizenNitroConfig config;

        protected override string ConfigFileName { get; } = "CitizenNitro.conf";
        private int Probability => config?.Probability ?? 7;

        protected override void Setup()
        {
            config = LoadConfig<CitizenNitroConfig>();

            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                    DrawText("CitizenNitro:" + IsActive);
                });

            OnAllOnCommandObservable.Subscribe(_ => IsActive = true);

            //interval間隔で実行
            CreateTickAsObservable(TimeSpan.FromSeconds(3))
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
                    .Where(x => (!playerVehicle.IsSafeExist() || x != playerVehicle) &&
                                x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist() && !x.IsPersistent);

                foreach (var veh in nitroAvailableVeles)
                {
                    if (Random.Next(0, 100) <= Probability)
                    {
                        DelayCoroutine(veh, ActivationCancellationToken).Forget();
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
        private async ValueTask DelayCoroutine(Vehicle v, CancellationToken ct)
        {
            var waitSeconds = Random.Next(0, 5);
            await DelayAsync(TimeSpan.FromSeconds(waitSeconds), ct);
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
                vehicle.Quaternion = Quaternion.RotationAxis(vehicle.RightVector, Random.Next(20, 60) / 100.0f) *
                                     vehicle.Quaternion;
            }

            vehicle.SetForwardSpeed(vehicle.Speed + velocitiesSpeed);

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
            if (!ped.IsSafeExist())
            {
                return;
            }

            DelayParachute(ped, ActivationCancellationToken).Forget();
        }

        private async ValueTask DelayParachute(Ped ped, CancellationToken ct)
        {
            ped.SetNotChaosPed(true);
            ped.ClearTasksImmediately();
            ped.Position += new Vector3(0, 0, 0.5f);
            ped.SetToRagdoll();

            await DelayAsync(TimeSpan.FromSeconds(0.1f), ct);

            ped.ApplyForce(new Vector3(0, 0, 40.0f));
            ped.IsInvincible = true;

            try
            {
                await DelayAsync(TimeSpan.FromSeconds(1.5f), ct);
                if (!ped.IsSafeExist())
                {
                    return;
                }
            }
            finally
            {
                if (ped.IsSafeExist())
                {
                    ped.IsInvincible = false;
                }
            }

            ped.ParachuteTo(PlayerPed.Position);

            await DelayAsync(TimeSpan.FromSeconds(15), ct);

            if (!ped.IsSafeExist())
            {
                return;
            }

            ped.SetNotChaosPed(false);
        }

        private class CitizenNitroConfig : InfernoConfig
        {
            public int Probability { get; } = 7;

            public override bool Validate()
            {
                return Probability > 0 && Probability <= 100;
            }
        }
    }
}