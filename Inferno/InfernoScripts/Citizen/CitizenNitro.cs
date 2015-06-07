using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using Reactive.Bindings;
using System.Reactive.Concurrency;

namespace Inferno
{
    /// <summary>
    /// 市ニトロ
    /// </summary>
    public class CitizenNitro : InfernoScript
    {
        private readonly string Keyword = "cnitro";
        private readonly int probability = 5;

        public bool _isActive = false;
        private readonly int[] _velocities = {-70, -50, -30, 30, 50, 70, 100};

        /// <summary>
        /// スクリプトの実行間隔　３秒
        /// </summary>
        protected override int TickInterval
        {
            get { return 3000; }
        }

        protected override void Setup()
        {
            //キーワードが入力されたらON／OFFを切り替える
            CreateInputKeywordAsObservable(Keyword)
                .Subscribe(_ =>
                {
                    _isActive = !_isActive;
                    DrawText("CitizenNitro:" + _isActive, 3.0f);
                });

            OnAllOnCommandObservable.Subscribe(_ => _isActive = true);


            //interval間隔で実行
            OnTickAsObservable
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

                var nitroAvailableVeles = CachedVehicles
                    .Where(
                        x => x != playerVehicle && x.GetPedOnSeat(VehicleSeat.Driver).IsSafeExist() && !x.IsPersistent);

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
                LogWrite("CitizenNitroAction()nice catch!\r\n");
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
