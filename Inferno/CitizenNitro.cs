using System;
using System.Linq;
using System.Reactive.Linq;
using GTA;
using GTA.Native;

namespace Inferno
{
    /// <summary>
    /// 市ニトロ
    /// </summary>
    public class CitizenNitro : InfernoScript
    {
        private bool IsActive = false;

        private readonly int[] VelocityList = new[] {-70, -50, -30, 30, 50, 70, 100};

        protected override void Setup()
        {
            CreateInputKeywordAsObservable("cnitro")
                .Subscribe(_ =>
                {
                    IsActive = !IsActive;
                });

            CreateTickAsObservable(5000)
                .Where(_ => IsActive)
                .Subscribe(_ => CitizenNitroAction());
        }

        /// <summary>
        /// ニトロ処理本体
        /// </summary>
        private void CitizenNitroAction()
        {
            try
            {
                var nitroAvailableVeles =
                    CachedPeds
                        .Where(x => x != null && x.Exists() && x.IsAlive && x.IsInVehicle())
                        .Select(x => x.CurrentVehicle)
                        .Where(x => x != null && x.Exists() && x.IsDriveable);

                foreach (var veh in nitroAvailableVeles)
                {
                    if (random.Next(0, 100) <= 30)
                    {
                        NitroVehicle(veh);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        void NitroVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists()) { return; }

            vehicle.Speed += VelocityList[random.Next(0,VelocityList.Length)];

            Function.Call(Hash.ADD_EXPLOSION, new InputArgument[]
            {
                vehicle.Position.X,
                vehicle.Position.Y,
                vehicle.Position.Z,
                0,
                0.0f,
                true,
                true,
                1.0f
            });
        }
    }
}
