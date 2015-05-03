using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace Inferno
{
    /// <summary>
    /// 市ニトロ
    /// </summary>
    public class CitizenNitro : InfernoScript
    {
        private readonly string Keyword = "cnitro";
        private readonly int interval = 3000;
        private readonly int probability = 5;

        private bool _isActive = false;
        private readonly int[] _velocities = {-70, -50, -30, 30, 50, 70, 100};

        protected override void Setup()
        {
            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                });

            //interval間隔で実行
            CreateTickAsObservable(interval)
                .Where(_ => _isActive)
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

                var nitroAvailableVeles = CachedPeds
                    .Where(x => x.IsSafeExist() && x.IsAlive && x.IsInVehicle())
                    .Select(x => x.CurrentVehicle)
                    .Where(x => x.IsSafeExist() && x.IsAlive && x.IsSameEntity(playerVehicle));

                foreach (var veh in nitroAvailableVeles)
                {
                    if (Random.Next(0, 100) <= probability)
                    {
                        NitroVehicle(veh);
                    }
                }
            }
            catch
            {
                //nice catch!
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
                0,
                0.0f,
                true,
                true,
                0.1f
            });
        }
    }
}
