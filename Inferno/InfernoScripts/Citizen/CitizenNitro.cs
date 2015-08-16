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
        private readonly string Keyword = "cnitro";
        private readonly int probability = 7;
        private readonly int[] _velocities = {-100,-70, -50, 50, 70, 100};

        protected override void Setup()
        {
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
                //車の運転手を取れないので市民から車を取得する
                var playerVehicle = this.GetPlayerVehicle();

                var nitroAvailableVeles = CachedVehicles
                    .Where(x => (!playerVehicle.IsSafeExist() || x != playerVehicle) && x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist() && !x.IsPersistent);

                foreach (var veh in nitroAvailableVeles)
                {
                    if (Random.Next(0, 100) <= probability)
                    {
                        
                        NitroVehicle(veh);
                    }
                }
            }
            catch(Exception e)
            {
                //nice catch!
                LogWrite(e.ToString());
            }
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

            vehicle.Speed += _velocities[Random.Next(0, _velocities.Length)];

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
    }
}
